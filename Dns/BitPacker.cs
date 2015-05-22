// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="BitPacker.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;

    public class BitPacker
    {
        private readonly byte[] _buffer;
        private int _bitOffset = 0;
        private int[] _mask = new[] {1, 2, 4, 8, 16, 32, 64, 128};

        public BitPacker(byte[] buffer)
        {
            _buffer = buffer;
        }

        /// <param name="count"></param>
        /// <returns></returns>
        public byte GetByte(int count)
        {
            if (count > 8) throw new ArgumentOutOfRangeException("count");

            int bit = _bitOffset;

            if (((bit + count) / 8) > this._buffer.Length) throw new ArgumentOutOfRangeException("count");

            int byteNumber;
            ushort bitOffset;

            GenerateInitialOffset(out byteNumber, out bitOffset);

            ushort span = BitConverter.ToUInt16(_buffer, byteNumber);

            int mask = GetMask(count, bitOffset);
            int value = (span & mask) >> bitOffset;

            _bitOffset += count;
            return (byte) value;
        }

        public enum Endian
        {
            HiLo,
            LoHi
        }

        public ushort GetUshort(int count = 16, Endian endian = Endian.LoHi)
        {
            if (count > 16) throw new ArgumentOutOfRangeException("count");

            int bit = _bitOffset;

            if (((bit + count) / 8) > this._buffer.Length) throw new ArgumentOutOfRangeException("count");

            int byteNumber;
            ushort bitOffset;

            GenerateInitialOffset(out byteNumber, out bitOffset);

            uint span;
            if (byteNumber + 4 <= _buffer.Length)
            {
                span = BitConverter.ToUInt32(_buffer, byteNumber);
            }
            else
            {
                // buffer too small - clone bytes into an empty buffer
                byte[] copy = new byte[8];
                Array.Copy(_buffer, byteNumber, copy, byteNumber, _buffer.Length - byteNumber);
                span = BitConverter.ToUInt32(copy, byteNumber);
            }

            int mask = GetMask(count, bitOffset);
            ushort value = (ushort) ((span & mask) >> bitOffset);

            if (endian == Endian.HiLo)
            {
                SwapEndian(ref value);
            }

            _bitOffset += count;
            return value;
        }

        private static int GetMask(int count, ushort bitOffset)
        {
            int mask = 0;
            for (int index = bitOffset; index < bitOffset + count; index++)
            {
                mask = mask + (int) Math.Pow(2, index);
            }
            return mask;
        }

        public bool GetBoolean()
        {
            int byteNumber;
            ushort bitOffset;
            
            GenerateInitialOffset(out byteNumber, out bitOffset);
            int bitMask = _mask[bitOffset];

            bool value = (_buffer[byteNumber] & bitMask) > 0;

            _bitOffset++;
            return value;
        }

        public void Reset()
        {
            this._bitOffset = 0;
        }

        public int Write(byte value, uint count)
        {
            
            // generate bit
            return 0;
        }

        private void GenerateInitialOffset(out int index, out ushort offset)
        {
            index = _bitOffset == 0 ? 0 : (_bitOffset / 8);
            offset = (ushort)(_bitOffset - (index * 8));
        }

        public static void SwapEndian(ref ushort val)
        {
            val = (ushort)((val << 8) | (val >> 8));
        }

        public static void SwapEndian(ref uint val)
        {
	        val = (val<<24) | ((val<<8) & 0x00ff0000) | ((val>>8) & 0x0000ff00) | (val>>24);
        }
    }
}