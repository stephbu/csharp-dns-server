// //-------------------------------------------------------------------------------------------------
// // <copyright file="EndianTests.cs" company="stephbu">
// // Copyright (c) Steve Butler. All rights reserved.
// // </copyright>
// //-------------------------------------------------------------------------------------------------

namespace DnsTest
{
    using System;
    using System.Buffers.Binary;
    using System.IO;
    using System.Net;
    using Dns;
    using Xunit;

    /// <summary>
    /// Tests for endianness handling in DNS protocol parsing and serialization.
    /// DNS uses network byte order (big-endian) for all multi-byte values.
    /// These tests verify correct behavior on both little-endian and big-endian systems.
    /// </summary>
    public class EndianTests
    {
        [Fact]
        public void SwapEndian_Ushort_ProducesNetworkByteOrder()
        {
            // DNS uses big-endian (network byte order)
            // Value 0x1234 should be serialized as bytes [0x12, 0x34]
            ushort hostValue = 0x1234;
            ushort networkValue = hostValue.SwapEndian();

            // After SwapEndian, writing to stream should produce big-endian bytes
            using var stream = new MemoryStream();
            networkValue.WriteToStream(stream);
            byte[] bytes = stream.ToArray();

            // The result should be big-endian: 0x12, 0x34
            Assert.Equal(0x12, bytes[0]);
            Assert.Equal(0x34, bytes[1]);
        }

        [Fact]
        public void SwapEndian_Uint_ProducesNetworkByteOrder()
        {
            // DNS uses big-endian (network byte order)
            // Value 0x12345678 should be serialized as bytes [0x12, 0x34, 0x56, 0x78]
            uint hostValue = 0x12345678;
            uint networkValue = hostValue.SwapEndian();

            // After SwapEndian, writing to stream should produce big-endian bytes
            using var stream = new MemoryStream();
            networkValue.WriteToStream(stream);
            byte[] bytes = stream.ToArray();

            // The result should be big-endian: 0x12, 0x34, 0x56, 0x78
            Assert.Equal(0x12, bytes[0]);
            Assert.Equal(0x34, bytes[1]);
            Assert.Equal(0x56, bytes[2]);
            Assert.Equal(0x78, bytes[3]);
        }

        [Fact]
        public void SwapEndian_RoundTrip_PreservesValue()
        {
            // Converting to network order and back should preserve the original value
            ushort originalUshort = 0xABCD;
            Assert.Equal(originalUshort, originalUshort.SwapEndian().SwapEndian());

            uint originalUint = 0x12345678;
            Assert.Equal(originalUint, originalUint.SwapEndian().SwapEndian());
        }

        [Fact]
        public void NetworkToHost_And_HostToNetwork_AreSymmetric()
        {
            ushort value16 = 0x1234;
            Assert.Equal(value16, value16.HostToNetwork().NetworkToHost());
            Assert.Equal(value16, value16.NetworkToHost().HostToNetwork());

            uint value32 = 0x12345678;
            Assert.Equal(value32, value32.HostToNetwork().NetworkToHost());
            Assert.Equal(value32, value32.NetworkToHost().HostToNetwork());
        }

        [Fact]
        public void DnsMessage_QueryIdentifier_IsNetworkByteOrder()
        {
            // Create a DNS message with a known query ID
            var message = new DnsMessage();
            message.QueryIdentifier = 0x1234;

            // Serialize to bytes
            using var stream = new MemoryStream();
            message.WriteToStream(stream);
            byte[] bytes = stream.ToArray();

            // Query ID is at offset 0, should be in network byte order (big-endian)
            Assert.Equal(0x12, bytes[0]);
            Assert.Equal(0x34, bytes[1]);
        }

        [Fact]
        public void DnsMessage_Parse_HandlesNetworkByteOrder()
        {
            // Create a minimal DNS query packet in network byte order
            byte[] packet = new byte[]
            {
                0x12, 0x34, // Query ID: 0x1234 (big-endian)
                0x01, 0x00, // Flags: standard query, recursion desired
                0x00, 0x01, // Question count: 1 (big-endian)
                0x00, 0x00, // Answer count: 0
                0x00, 0x00, // Authority count: 0
                0x00, 0x00, // Additional count: 0
                // Question: "a" type A class IN
                0x01, 0x61, // length 1, 'a'
                0x00,       // null terminator
                0x00, 0x01, // Type A (big-endian)
                0x00, 0x01  // Class IN (big-endian)
            };

            Assert.True(DnsMessage.TryParse(packet, out var message));
            Assert.Equal((ushort)0x1234, message.QueryIdentifier);
            Assert.Equal((ushort)1, message.QuestionCount);
        }

        [Fact]
        public void ResourceRecord_TTL_IsNetworkByteOrder()
        {
            // TTL of 300 seconds (0x0000012C) should serialize as big-endian
            var record = new ResourceRecord
            {
                Name = "test",
                Type = ResourceType.A,
                Class = ResourceClass.IN,
                TTL = 300,
                RData = new ANameRData { Address = new IPAddress(new byte[] { 127, 0, 0, 1 }) }
            };

            using var stream = new MemoryStream();
            record.WriteToStream(stream);
            byte[] bytes = stream.ToArray();

            // Find TTL in the serialized data (after name, type, class)
            // Name "test" = [4, t, e, s, t, 0] = 6 bytes
            // Type = 2 bytes, Class = 2 bytes
            // TTL starts at offset 10
            int ttlOffset = 6 + 2 + 2; // name + type + class
            uint serializedTtl = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(ttlOffset));
            Assert.Equal(300u, serializedTtl);
        }

        [Fact]
        public void BinaryPrimitives_ReadBigEndian_MatchesDnsProtocol()
        {
            // Verify that BinaryPrimitives reads the same as our parsing
            byte[] networkData = new byte[] { 0x12, 0x34, 0x56, 0x78 };

            int offset = 0;
            ushort value16 = DnsProtocol.ReadUshort(networkData, ref offset).SwapEndian();
            Assert.Equal((ushort)0x1234, value16);

            offset = 0;
            uint value32 = DnsProtocol.ReadUint(networkData, ref offset).SwapEndian();
            Assert.Equal(0x12345678u, value32);

            // Compare with BinaryPrimitives
            Assert.Equal((ushort)0x1234, BinaryPrimitives.ReadUInt16BigEndian(networkData));
            Assert.Equal(0x12345678u, BinaryPrimitives.ReadUInt32BigEndian(networkData));
        }

        [Fact]
        public void BitConverterIsLittleEndian_ReportsSystemEndianness()
        {
            // This test documents the system's endianness for debugging
            // Most modern systems are little-endian (x86, x64, ARM in LE mode)
            bool isLittleEndian = BitConverter.IsLittleEndian;

            // Test that our SwapEndian behaves correctly for this system
            ushort testValue = 0x0102;
            byte[] directBytes = BitConverter.GetBytes(testValue);

            if (isLittleEndian)
            {
                // On little-endian: 0x0102 stored as [0x02, 0x01]
                Assert.Equal(0x02, directBytes[0]);
                Assert.Equal(0x01, directBytes[1]);
            }
            else
            {
                // On big-endian: 0x0102 stored as [0x01, 0x02]
                Assert.Equal(0x01, directBytes[0]);
                Assert.Equal(0x02, directBytes[1]);
            }
        }

        [Fact]
        public void SwapEndian_Zero_ReturnsZero()
        {
            Assert.Equal((ushort)0, ((ushort)0).SwapEndian());
            Assert.Equal(0u, 0u.SwapEndian());
        }

        [Fact]
        public void SwapEndian_MaxValue_HandlesCorrectly()
        {
            // Max values should swap correctly
            Assert.Equal(ushort.MaxValue, ushort.MaxValue.SwapEndian());
            Assert.Equal(uint.MaxValue, uint.MaxValue.SwapEndian());
        }
    }
}
