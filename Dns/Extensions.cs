// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Extensions.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.Buffers.Binary;
    using System.IO;
    using System.Text;

    public static class Extensions
    {
        public static TextWriter CreateWriter(this Stream stream, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            // write all data using UTF-8
            return new StreamWriter(stream, encoding);
        }

        /// <summary>
        /// Converts a ushort between host byte order and network byte order (big-endian).
        /// On little-endian systems, this swaps the bytes. On big-endian systems, this is a no-op.
        /// </summary>
        /// <remarks>
        /// DNS protocol uses network byte order (big-endian) for all multi-byte values.
        /// This method handles the conversion regardless of host architecture.
        /// </remarks>
        public static ushort SwapEndian(this ushort val)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (ushort)((val << 8) | (val >> 8));
            }
            return val;
        }

        /// <summary>
        /// Converts a uint between host byte order and network byte order (big-endian).
        /// On little-endian systems, this swaps the bytes. On big-endian systems, this is a no-op.
        /// </summary>
        /// <remarks>
        /// DNS protocol uses network byte order (big-endian) for all multi-byte values.
        /// This method handles the conversion regardless of host architecture.
        /// </remarks>
        public static uint SwapEndian(this uint val)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (val << 24) | ((val << 8) & 0x00ff0000) | ((val >> 8) & 0x0000ff00) | (val >> 24);
            }
            return val;
        }

        /// <summary>
        /// Converts a ushort from network byte order (big-endian) to host byte order.
        /// Equivalent to SwapEndian but semantically clearer for reading operations.
        /// </summary>
        public static ushort NetworkToHost(this ushort val) => val.SwapEndian();

        /// <summary>
        /// Converts a uint from network byte order (big-endian) to host byte order.
        /// Equivalent to SwapEndian but semantically clearer for reading operations.
        /// </summary>
        public static uint NetworkToHost(this uint val) => val.SwapEndian();

        /// <summary>
        /// Converts a ushort from host byte order to network byte order (big-endian).
        /// Equivalent to SwapEndian but semantically clearer for writing operations.
        /// </summary>
        public static ushort HostToNetwork(this ushort val) => val.SwapEndian();

        /// <summary>
        /// Converts a uint from host byte order to network byte order (big-endian).
        /// Equivalent to SwapEndian but semantically clearer for writing operations.
        /// </summary>
        public static uint HostToNetwork(this uint val) => val.SwapEndian();



        public static byte[] GetResourceBytes(this string str, char delimiter = '.')
        {
            if (str == null)
            {
                str = "";
            }

            using (MemoryStream stream = new MemoryStream(str.Length + 2))
            {
                string[] segments = str.Split(new char[] { '.' });
                foreach (string segment in segments)
                {
                    stream.WriteByte((byte)segment.Length);
                    foreach (char currentChar in segment)
                    {
                        stream.WriteByte((byte)currentChar);
                    }
                }
                // null delimiter
                stream.WriteByte(0x0);
                return stream.GetBuffer();
            }
        }

        public static void WriteToStream(this string str, Stream stream)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                string[] segments = str.Split(new char[] { '.' });
                foreach (string segment in segments)
                {
                    stream.WriteByte((byte)segment.Length);
                    foreach (char currentChar in segment)
                    {
                        stream.WriteByte((byte)currentChar);
                    }
                }
            }

            // null delimiter
            stream.WriteByte(0x0);
        }


        public static byte[] GetBytes(this string str, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.ASCII;
            return encoding.GetBytes(str);
        }

        public static string IP(long ipLong)
        {
            StringBuilder b = new StringBuilder();
            long tempLong, temp;

            tempLong = ipLong;
            temp = tempLong / (256 * 256 * 256);
            tempLong = tempLong - (temp * 256 * 256 * 256);
            b.Append(Convert.ToString(temp)).Append(".");
            temp = tempLong / (256 * 256);
            tempLong = tempLong - (temp * 256 * 256);
            b.Append(Convert.ToString(temp)).Append(".");
            temp = tempLong / 256;
            tempLong = tempLong - (temp * 256);
            b.Append(Convert.ToString(temp)).Append(".");
            temp = tempLong;
            tempLong = tempLong - temp;
            b.Append(Convert.ToString(temp));

            return b.ToString().ToLower();
        }

        /// <summary>
        /// Writes a ushort to stream in little-endian byte order.
        /// </summary>
        /// <remarks>
        /// Note: For DNS protocol, callers should use .SwapEndian().WriteToStream() 
        /// to write in network byte order (big-endian).
        /// </remarks>
        public static void WriteToStream(this ushort value, Stream stream)
        {
            stream.WriteByte((byte)(value & 0xFF));
            stream.WriteByte((byte)((value >> 8) & 0xFF));
        }

        /// <summary>
        /// Writes a uint to stream in little-endian byte order.
        /// </summary>
        /// <remarks>
        /// Note: For DNS protocol, callers should use .SwapEndian().WriteToStream() 
        /// to write in network byte order (big-endian).
        /// </remarks>
        public static void WriteToStream(this uint value, Stream stream)
        {
            stream.WriteByte((byte)(value & 0xFF));
            stream.WriteByte((byte)((value >> 8) & 0xFF));
            stream.WriteByte((byte)((value >> 16) & 0xFF));
            stream.WriteByte((byte)((value >> 24) & 0xFF));
        }
    }
}
