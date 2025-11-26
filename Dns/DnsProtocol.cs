// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="DnsProtocol.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class DnsProtocol
    {
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

        public static ushort ReadUshort(byte[] bytes, ref int offset)
        {
            ushort ret = BitConverter.ToUInt16(bytes, offset);
            offset += sizeof(ushort);
            return ret;
        }

        public static uint ReadUint(byte[] bytes, ref int offset)
        {
            uint ret = BitConverter.ToUInt32(bytes, offset);
            offset += sizeof(uint);
            return ret;
        }


        public static string ReadString(byte[] bytes, ref int currentOffset)
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
