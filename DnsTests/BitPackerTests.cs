using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dns;

namespace DnsTests
{
    using Dns.Utility;

    [TestClass]
    public class BitPackerTests
    {
        [TestMethod]
        public void Test1()
        {
            byte[] bytes = BitConverter.GetBytes(0xAA);

            BitPacker packer = new BitPacker(bytes);
            Assert.IsFalse(packer.GetBoolean());
            Assert.IsTrue(packer.GetBoolean());
            Assert.IsFalse(packer.GetBoolean());
            Assert.IsTrue(packer.GetBoolean());
            Assert.IsFalse(packer.GetBoolean());
            Assert.IsTrue(packer.GetBoolean());
            Assert.IsFalse(packer.GetBoolean());
            Assert.IsTrue(packer.GetBoolean());

            bytes = BitConverter.GetBytes(0x0A);
            packer = new BitPacker(bytes);
            Assert.IsFalse(packer.GetBoolean());
            Assert.IsTrue(packer.GetBoolean());
            Assert.IsFalse(packer.GetBoolean());
            Assert.IsTrue(packer.GetBoolean());
            Assert.IsFalse(packer.GetBoolean());
            Assert.IsFalse(packer.GetBoolean());
            Assert.IsFalse(packer.GetBoolean());
            Assert.IsFalse(packer.GetBoolean());
        }

        [TestMethod]
        public void Test2()
        {
            byte[] bytes;
            BitPacker packer;

            bytes = BitConverter.GetBytes(0xAFFF);
            packer = new BitPacker(bytes);
            
            Assert.IsTrue(packer.GetBoolean());
            Assert.IsTrue(packer.GetBoolean());
            Assert.AreEqual(15, packer.GetByte(4));
            Assert.IsTrue(packer.GetBoolean());
            Assert.AreEqual(95, packer.GetByte(7));
            Assert.IsFalse(packer.GetBoolean());
            Assert.IsTrue(packer.GetBoolean());
        }

        [TestMethod]
        public void Test3()
        {
            byte[] bytes;
            BitPacker packer;

            bytes = BitConverter.GetBytes(0xAFFF);
            packer = new BitPacker(bytes);

            Assert.AreEqual(15, packer.GetByte(4));
            Assert.AreEqual(15, packer.GetByte(4));
            Assert.AreEqual(0xAF,packer.GetUshort(8));

            bytes = BitConverter.GetBytes(0x0CD000);
            packer = new BitPacker(bytes);

            Assert.AreEqual(0x00, packer.GetByte(8));
            Assert.AreEqual(0x0CD0, packer.GetUshort(16));

            bytes = BitConverter.GetBytes(0x000F << 1) ;
            packer = new BitPacker(bytes);
            Assert.IsFalse(packer.GetBoolean());
            Assert.AreEqual(0xF, packer.GetUshort(8));

            bytes = BitConverter.GetBytes(0xAABB);
            packer = new BitPacker(bytes);
            Assert.AreEqual(0xAABB, packer.GetUshort(16, BitPacker.Endian.LoHi));

            packer.Reset();
            Assert.AreEqual(0xBBAA, packer.GetUshort(16, BitPacker.Endian.HiLo));

            packer.Reset();
            Assert.AreEqual(0xBBAA, packer.GetUshort(16, BitPacker.Endian.HiLo));

            bytes = BitConverter.GetBytes(0x0100);
            packer = new BitPacker(bytes);
            Assert.AreEqual(0x0001, packer.GetUshort(16, BitPacker.Endian.HiLo));
        }

        [TestMethod]
        public void TestEndian()
        {
            uint intValue = 0xAABBCCDD;
            BitPacker.SwapEndian(ref intValue);
            Assert.AreEqual(0xDDCCBBAA, intValue);

            ushort ushortValue = 0xAABB;
            BitPacker.SwapEndian(ref ushortValue);
            Assert.AreEqual(0xBBAA, ushortValue);
        }
    }
}
