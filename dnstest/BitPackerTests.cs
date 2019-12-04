using System;
using Xunit;

namespace DnsTest
{
    using Dns.Utility;

    public class BitPackerTests
    {
        [Fact]
        public void Test1()
        {
            byte[] bytes = BitConverter.GetBytes(0xAA);

            BitPacker packer = new BitPacker(bytes);
            Assert.False(packer.GetBoolean());
            Assert.True(packer.GetBoolean());
            Assert.False(packer.GetBoolean());
            Assert.True(packer.GetBoolean());
            Assert.False(packer.GetBoolean());
            Assert.True(packer.GetBoolean());
            Assert.False(packer.GetBoolean());
            Assert.True(packer.GetBoolean());

            bytes = BitConverter.GetBytes(0x0A);
            packer = new BitPacker(bytes);
            Assert.False(packer.GetBoolean());
            Assert.True(packer.GetBoolean());
            Assert.False(packer.GetBoolean());
            Assert.True(packer.GetBoolean());
            Assert.False(packer.GetBoolean());
            Assert.False(packer.GetBoolean());
            Assert.False(packer.GetBoolean());
            Assert.False(packer.GetBoolean());
        }

        [Fact]
        public void Test2()
        {
            byte[] bytes;
            BitPacker packer;

            bytes = BitConverter.GetBytes(0xAFFF);
            packer = new BitPacker(bytes);
            
            Assert.True(packer.GetBoolean());
            Assert.True(packer.GetBoolean());
            Assert.Equal(15, packer.GetByte(4));
            Assert.True(packer.GetBoolean());
            Assert.Equal(95, packer.GetByte(7));
            Assert.False(packer.GetBoolean());
            Assert.True(packer.GetBoolean());
        }

        [Fact]
        public void Test3()
        {
            byte[] bytes;
            BitPacker packer;

            bytes = BitConverter.GetBytes(0xAFFF);
            packer = new BitPacker(bytes);

            Assert.Equal(15, packer.GetByte(4));
            Assert.Equal(15, packer.GetByte(4));
            Assert.Equal(0xAF,packer.GetUshort(8));

            bytes = BitConverter.GetBytes(0x0CD000);
            packer = new BitPacker(bytes);

            Assert.Equal(0x00, packer.GetByte(8));
            Assert.Equal(0x0CD0, packer.GetUshort(16));

            bytes = BitConverter.GetBytes(0x000F << 1) ;
            packer = new BitPacker(bytes);
            Assert.False(packer.GetBoolean());
            Assert.Equal(0xF, packer.GetUshort(8));

            bytes = BitConverter.GetBytes(0xAABB);
            packer = new BitPacker(bytes);
            Assert.Equal(0xAABB, packer.GetUshort(16, BitPacker.Endian.LoHi));

            packer.Reset();
            Assert.Equal(0xBBAA, packer.GetUshort(16, BitPacker.Endian.HiLo));

            packer.Reset();
            Assert.Equal(0xBBAA, packer.GetUshort(16, BitPacker.Endian.HiLo));

            bytes = BitConverter.GetBytes(0x0100);
            packer = new BitPacker(bytes);
            Assert.Equal(0x0001, packer.GetUshort(16, BitPacker.Endian.HiLo));
        }

        [Fact]
        public void TestEndian()
        {
            uint intValue = 0xAABBCCDD;
            BitPacker.SwapEndian(ref intValue);
            Assert.Equal(0xDDCCBBAA, intValue);

            ushort ushortValue = 0xAABB;
            BitPacker.SwapEndian(ref ushortValue);
            Assert.Equal(0xBBAA, ushortValue);
        }
    }
}
