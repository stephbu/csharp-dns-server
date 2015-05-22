// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Extensions.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
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

        public static ushort SwapEndian(this ushort val)
        {
            ushort value = (ushort) ((val << 8) | (val >> 8));
            return value;
        }

        public static uint SwapEndian(this uint val)
        {
            uint value = (val << 24) | ((val << 8) & 0x00ff0000) | ((val >> 8) & 0x0000ff00) | (val >> 24);
            return value;
        }



        public static byte[] GetResourceBytes(this string str, char delimiter = '.')
        {
            if (str == null)
            {
                str = "";
            }

            using (MemoryStream stream = new MemoryStream(str.Length + 2))
            {
                string[] segments = str.Split(new char[] {'.'});
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
            temp = tempLong/(256*256*256);
            tempLong = tempLong - (temp*256*256*256);
            b.Append(Convert.ToString(temp)).Append(".");
            temp = tempLong/(256*256);
            tempLong = tempLong - (temp*256*256);
            b.Append(Convert.ToString(temp)).Append(".");
            temp = tempLong/256;
            tempLong = tempLong - (temp*256);
            b.Append(Convert.ToString(temp)).Append(".");
            temp = tempLong;
            tempLong = tempLong - temp;
            b.Append(Convert.ToString(temp));

            return b.ToString().ToLower();
        }

        public static void WriteToStream(this ushort value, Stream stream)
        {
            stream.WriteByte((byte)(value & 0xFF));
            stream.WriteByte((byte)((value >> 8) & 0xFF));
        }


        public static void WriteToStream(this uint value, Stream stream)
        {
            stream.WriteByte((byte)(value & 0xFF));
            stream.WriteByte((byte)((value >> 8) & 0xFF));
            stream.WriteByte((byte)((value >> 16) & 0xFF));
            stream.WriteByte((byte)((value >> 24) & 0xFF));
        }
    }
}