// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="DnsProtocol.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class DnsProtocol
    {
        /// <summary>Maximum length for a DNS name (255 bytes per RFC 1035).</summary>
        private const int MaxDnsNameLength = 255;

        /// <summary>Try to parse a DNS message from a byte array.</summary>
        /// <param name="bytes">The buffer containing the DNS message.</param>
        /// <param name="dnsMessage">The parsed DNS message if successful.</param>
        /// <returns>True if parsing succeeded, false otherwise.</returns>
        public static bool TryParse(byte[] bytes, out DnsMessage dnsMessage)
        {
            return TryParse(bytes, bytes.Length, out dnsMessage);
        }

        /// <summary>Try to parse a DNS message from a byte array with explicit length.</summary>
        /// <param name="bytes">The buffer containing the DNS message.</param>
        /// <param name="length">The number of valid bytes in the buffer.</param>
        /// <param name="dnsMessage">The parsed DNS message if successful.</param>
        /// <returns>True if parsing succeeded, false otherwise.</returns>
        public static bool TryParse(byte[] bytes, int length, out DnsMessage dnsMessage)
        {
            if (!DnsMessage.TryParse(bytes, length, out dnsMessage))
            {
                return false;
            }

            return true;
        }

        /// <summary>Read a ushort from the buffer (native endian, caller handles swap if needed).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUshort(byte[] bytes, ref int offset)
        {
            // NOTE: Preserves original semantics - returns native-endian value.
            // Callers that need network byte order must call .SwapEndian().
            ushort ret = BitConverter.ToUInt16(bytes, offset);
            offset += sizeof(ushort);
            return ret;
        }

        /// <summary>Read a big-endian ushort from a span (already network byte order).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUshortBigEndian(ReadOnlySpan<byte> bytes, ref int offset)
        {
            ushort ret = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(offset));
            offset += sizeof(ushort);
            return ret;
        }

        /// <summary>Read a uint from the buffer (native endian, caller handles swap if needed).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUint(byte[] bytes, ref int offset)
        {
            // NOTE: Preserves original semantics - returns native-endian value.
            // Callers that need network byte order must call .SwapEndian().
            uint ret = BitConverter.ToUInt32(bytes, offset);
            offset += sizeof(uint);
            return ret;
        }

        /// <summary>Read a big-endian uint from a span (already network byte order).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUintBigEndian(ReadOnlySpan<byte> bytes, ref int offset)
        {
            uint ret = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(offset));
            offset += sizeof(uint);
            return ret;
        }

        /// <summary>
        /// Reads a DNS domain name from the byte buffer, handling compression pointers.
        /// Optimized to minimize allocations using stackalloc and string.Create.
        /// </summary>
        /// <param name="bytes">The DNS message buffer.</param>
        /// <param name="currentOffset">The current read position, updated after reading.</param>
        /// <returns>The domain name as a string (without trailing dot).</returns>
        public static string ReadString(byte[] bytes, ref int currentOffset)
        {
            return ReadStringOptimized(bytes.AsSpan(), ref currentOffset);
        }

        /// <summary>
        /// Reads a DNS domain name from a span, handling compression pointers.
        /// Uses stackalloc for intermediate storage to minimize heap allocations.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadStringOptimized(ReadOnlySpan<byte> bytes, ref int currentOffset)
        {
            // Stack-allocate buffer for the domain name (max 255 chars per RFC 1035)
            Span<char> nameBuffer = stackalloc char[MaxDnsNameLength];
            int nameLength = 0;

            int compressionOffset = -1;
            int readOffset = currentOffset;
            HashSet<int> pointerVisitedOffsets = null;

            while (true)
            {
                if (readOffset >= bytes.Length)
                {
                    throw new IndexOutOfRangeException("DNS label offset exceeded buffer length.");
                }

                int segmentLength = bytes[readOffset];

                // Compressed name pointer (top 2 bits = 11)
                if ((segmentLength & 0xC0) == 0xC0)
                {
                    if (readOffset + 1 >= bytes.Length)
                    {
                        throw new IndexOutOfRangeException("DNS compression pointer exceeds buffer length.");
                    }

                    pointerVisitedOffsets ??= new HashSet<int>();
                    if (!pointerVisitedOffsets.Add(readOffset))
                    {
                        throw new InvalidDataException("DNS compression pointer cycle detected.");
                    }

                    int pointer = ((segmentLength & 0x3F) << 8) | bytes[readOffset + 1];
                    if (compressionOffset == -1)
                    {
                        // Remember where to resume after following the pointer
                        compressionOffset = readOffset + 2;
                    }

                    if (pointer >= bytes.Length)
                    {
                        throw new IndexOutOfRangeException("DNS compression pointer targets invalid offset.");
                    }

                    readOffset = pointer;
                    continue;
                }

                // Null terminator - end of name
                if (segmentLength == 0x00)
                {
                    readOffset++;
                    break;
                }

                readOffset++;

                if (segmentLength > 63)
                {
                    throw new InvalidDataException("DNS label length exceeds maximum of 63 bytes.");
                }

                if (readOffset + segmentLength > bytes.Length)
                {
                    throw new IndexOutOfRangeException("DNS label exceeds buffer length.");
                }

                // Check total name length won't exceed max
                int requiredLength = nameLength + segmentLength + (nameLength > 0 ? 1 : 0);
                if (requiredLength > MaxDnsNameLength)
                {
                    throw new InvalidDataException("DNS name exceeds maximum length of 255 characters.");
                }

                // Add separator if not the first label
                if (nameLength > 0)
                {
                    nameBuffer[nameLength++] = '.';
                }

                // Copy label bytes directly to char buffer (ASCII only)
                ReadOnlySpan<byte> labelBytes = bytes.Slice(readOffset, segmentLength);
                for (int i = 0; i < segmentLength; i++)
                {
                    byte b = labelBytes[i];
                    if (b > 0x7F)
                    {
                        throw new InvalidDataException("DNS label contains non-ASCII characters, which are not allowed per RFC 1035.");
                    }
                    nameBuffer[nameLength++] = (char)b;
                }

                readOffset += segmentLength;
            }

            currentOffset = compressionOffset == -1 ? readOffset : compressionOffset;

            // Create string directly from the span - single allocation
            return nameLength == 0 ? string.Empty : new string(nameBuffer.Slice(0, nameLength));
        }

        /// <summary>
        /// Legacy ReadString implementation using StringBuilder.
        /// Kept for compatibility but ReadStringOptimized is preferred.
        /// </summary>
        public static string ReadStringLegacy(byte[] bytes, ref int currentOffset)
        {
            StringBuilder resourceName = new StringBuilder();
            int compressionOffset = -1;
            int readOffset = currentOffset;
            HashSet<int> pointerVisitedOffsets = null;

            while (true)
            {
                if (readOffset >= bytes.Length)
                {
                    throw new IndexOutOfRangeException("DNS label offset exceeded buffer length.");
                }

                int segmentLength = bytes[readOffset];

                // compressed name pointer
                if ((segmentLength & 0xC0) == 0xC0)
                {
                    if (readOffset + 1 >= bytes.Length)
                    {
                        throw new IndexOutOfRangeException("DNS compression pointer exceeds buffer length.");
                    }

                    pointerVisitedOffsets ??= new HashSet<int>();
                    if (!pointerVisitedOffsets.Add(readOffset))
                    {
                        throw new InvalidDataException("DNS compression pointer cycle detected.");
                    }

                    int pointer = ((segmentLength & 0x3F) << 8) | bytes[readOffset + 1];
                    if (compressionOffset == -1)
                    {
                        // remember where to resume after following the pointer
                        compressionOffset = readOffset + 2;
                    }

                    if (pointer >= bytes.Length)
                    {
                        throw new IndexOutOfRangeException("DNS compression pointer targets invalid offset.");
                    }
                    // RFC 1035 §4.1.4: Pointers must reference a prior occurrence of the same name,
                    // must point to the start of a label, and forward references are prohibited.

                    readOffset = pointer;
                    continue;
                }

                if (segmentLength == 0x00)
                {
                    readOffset++;
                    break;
                }

                readOffset++;
                if (segmentLength > 63)
                {
                    throw new InvalidDataException("DNS label length exceeds maximum of 63 bytes.");
                }
                if (readOffset + segmentLength > bytes.Length)
                {
                    throw new IndexOutOfRangeException("DNS label exceeds buffer length.");
                }
                // RFC 1035: DNS labels must be ASCII.
                // This is an intentional breaking change; validate against existing usage if upgrading.
                // Check for non-ASCII bytes before decoding
                for (int i = 0; i < segmentLength; i++)
                {
                    if (bytes[readOffset + i] > 0x7F)
                    {
                        throw new InvalidDataException("DNS label contains non-ASCII characters, which are not allowed per RFC 1035.");
                    }
                }
                string label = Encoding.ASCII.GetString(bytes, readOffset, segmentLength);
                resourceName.Append(label).Append('.');
                readOffset += segmentLength;
            }

            currentOffset = compressionOffset == -1 ? readOffset : compressionOffset;

            return resourceName.ToString().TrimEnd('.');
        }
    }
}
