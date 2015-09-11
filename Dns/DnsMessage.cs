// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="DnsMessage.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.IO;

    public class DnsMessage
    {
        public ResourceList Additionals = new ResourceList();
        public ResourceList Answers = new ResourceList();
        public ResourceList Authorities = new ResourceList();
        public QuestionList Questions = new QuestionList();

        private ushort _additionalCount;
        private ushort _answerCount;
        private ushort _flags;
        private byte[] _header = new byte[12];
        private ushort _nameServerCount;
        private ushort _queryIdentifier;
        private ushort _questionCount;

        /// <summary>Provides direct access to the Flags WORD</summary>
        public ushort Flags
        {
            get { return _flags.SwapEndian(); }
            set
            {
                _flags = value.SwapEndian();
                byte[] bytes = BitConverter.GetBytes(_flags);
                bytes.CopyTo(_header, 2);
            }
        }

        /// <summary>Is Query Response</summary>
        public bool QR
        {
            get { return (this.Flags & 0x8000) == 0x8000; }
            set
            {
                if (value)
                {
                    this.Flags = (ushort)(this.Flags | 0x8000);
                }
                else
                {
                    this.Flags = (ushort)(this.Flags & (~0x8000));
                }
            }
        }

        /// <summary>Opcode</summary>
        public byte Opcode
        {
            get { return (byte) ((this.Flags & 0x7800) >> 11); }
            set { this.Flags = (ushort) ((this.Flags & ~0x7800) | (value << 11)); }
        }

        /// <summary>Is Authorative Answer</summary>
        public bool AA
        {
            get { return (this.Flags & 0x0400) == 0x0400; }
            set
            {
                if (value)
                {
                    this.Flags = (ushort)(this.Flags | 0x0400);
                }
                else
                {
                    this.Flags = (ushort)(this.Flags & (~0x0400));
                }
            }
        }

        /// <summary>Is Truncated</summary>
        public bool TC
        {
            get { return (this.Flags & 0x0200) == 0x0200; }
            set
            {
                if (value)
                {
                    this.Flags = (ushort)(this.Flags | 0x0200);
                }
                else
                {
                    this.Flags = (ushort)(this.Flags & (~0x0200));
                }
            }
        }

        /// <summary>Is Recursive Desired</summary>
        public bool RD
        {
            get { return (this.Flags & 0x0100) == 0x0100; }
            set
            {
                if (value)
                {
                    this.Flags = (ushort)(this.Flags | 0x0100);
                }
                else
                {
                    this.Flags = (ushort)(this.Flags & (~0x0100));
                }
            }
        }

        /// <summary>Is Recursive Allowable</summary>
        public bool RA
        {
            get { return (this.Flags & 0x0080) == 0x0080; }
            set
            {
                if (value)
                {
                    this.Flags = (ushort)(this.Flags | 0x0080);
                }
                else
                {
                    this.Flags = (ushort)(this.Flags & (~0x0080));
                }
            }
        }

        /// <summary>Reserved for future use</summary>
        public bool Zero
        {
            get { return (this.Flags & 0x0040) == 0x0040; }
            set
            {
                if (value)
                {
                    this.Flags = (ushort)(this.Flags | 0x0040);
                }
                else
                {
                    this.Flags = (ushort)(this.Flags & (~0x0040));
                }
            }
        }

        public bool AuthenticatingData
        {
            get { return (this.Flags & 0x0020) == 0x0020; }
            set
            {
                if (value)
                {
                    this.Flags = (ushort)(this.Flags | 0x0020);
                }
                else
                {
                    this.Flags = (ushort)(this.Flags & (~0x0020));
                }
            }
        }

        public bool CheckingDisabled
        {
            get { return (this.Flags & 0x0010) == 0x0010; }
            set
            {
                if (value)
                {
                    this.Flags = (ushort)(this.Flags | 0x0010);
                }
                else
                {
                    this.Flags = (ushort)(this.Flags & (~0x0010));
                }
            }
        }

        public byte RCode
        {
            get { return (byte) (this.Flags & 0x000F); }
            set { this.Flags = (ushort) ((this.Flags & ~0x000F) | value); }
        }

        public ushort AdditionalCount
        {
            get { return _additionalCount; }
            set
            {
                _additionalCount = value;
                byte[] bytes = BitConverter.GetBytes(_additionalCount.SwapEndian());
                bytes.CopyTo(_header, 10);
            }
        }

        public ushort AnswerCount
        {
            get { return _answerCount; }
            set
            {
                _answerCount = value;
                byte[] bytes = BitConverter.GetBytes(_answerCount.SwapEndian());
                bytes.CopyTo(_header, 6);
            }
        }

        public ushort NameServerCount
        {
            get { return _nameServerCount; }
            set
            {
                _nameServerCount = value;
                byte[] bytes = BitConverter.GetBytes(_nameServerCount.SwapEndian());
                bytes.CopyTo(_header, 8);
            }
        }

        public ushort QueryIdentifier
        {
            get { return _queryIdentifier; }
            set
            {
                _queryIdentifier = value;
                byte[] bytes = BitConverter.GetBytes(_queryIdentifier.SwapEndian());
                bytes.CopyTo(_header, 0);
            }
        }

        public ushort QuestionCount
        {
            get { return _questionCount; }
            set
            {
                _questionCount = value;
                byte[] bytes = BitConverter.GetBytes(_questionCount.SwapEndian());
                bytes.CopyTo(_header, 4);
            }
        }

        public bool IsQuery()
        {
            return this.QR == false;
        }

        /// <summary></summary>
        /// <param name="bytes"></param>
        private static DnsMessage Parse(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            DnsMessage result = new DnsMessage();

            int byteOffset = 0;
            byteOffset = byteOffset + result.ParseHeader(bytes, byteOffset);
            byteOffset += result.Questions.LoadFrom(bytes, byteOffset, result.QuestionCount);
            byteOffset += result.Answers.LoadFrom(bytes, byteOffset, result.AnswerCount);
            byteOffset += result.Authorities.LoadFrom(bytes, byteOffset, result.NameServerCount);
            byteOffset += result.Additionals.LoadFrom(bytes, byteOffset, result.AdditionalCount);

            // Console.WriteLine("Bytes read: {0}", byteOffset);

            return result;
        }

        private int ParseHeader(byte[] bytes, int offset)
        {
            if (bytes.Length < 12 + offset)
            {
                throw new InvalidDataException("bytes");
            }

            Buffer.BlockCopy(bytes, 0, _header, 0, 12);
            _queryIdentifier = BitConverter.ToUInt16(_header, 0).SwapEndian();
            _flags = BitConverter.ToUInt16(_header, 2);
            _questionCount = BitConverter.ToUInt16(_header, 4).SwapEndian();
            _answerCount = BitConverter.ToUInt16(_header, 6).SwapEndian();
            _nameServerCount = BitConverter.ToUInt16(_header, 8).SwapEndian();
            _additionalCount = BitConverter.ToUInt16(_header, 10).SwapEndian();

            return 12;
        }

        public void Dump()
        {
            Console.WriteLine("QueryIdentifier:   0x{0:X4}", this.QueryIdentifier);
            Console.WriteLine("QR:                ({0}... .... .... ....) {1}", this.QR ? 1 : 0, this.QR ? "Response" : "Query");
            Console.WriteLine("Opcode:            (.{0}{1}{2} {3}... .... ....) {4}", (this.Opcode & 1) > 1 ? 1 : 0, (this.Opcode & 2) > 1 ? 1 : 0, (this.Opcode & 4) > 1 ? 1 : 0, (this.Opcode & 8) > 1 ? 1 : 0, (OpCode) (this.Opcode));
            Console.WriteLine("AA:                (.... .{0}.. .... ....) {1}", this.AA ? 1 : 0, this.AA ? "Authoritative" : "Not Authoritative");
            Console.WriteLine("TC:                (.... ..{0}. .... ....) {1}", this.TC ? 1 : 0, this.TC ? "Truncated" : "Not Truncated");
            Console.WriteLine("RD:                (.... ...{0} .... ....) {1}", this.RD ? 1 : 0, this.RD ? "Recursion Desired" : "Recursion not desired");
            Console.WriteLine("RA:                (.... .... {0}... ....) {1}", this.RA ? 1 : 0, this.RA ? "Recursive Query Support Available" : "Recursive Query Support Not Available");
            Console.WriteLine("Zero:              (.... .... .0.. ....) 0");
            Console.WriteLine("AuthenticatedData: (.... .... ..{0}. ....) {1}", this.AuthenticatingData ? 1 : 0, this.AuthenticatingData ? "AuthenticatingData" : "Not AuthenticatingData");
            Console.WriteLine("CheckingDisabled:  (.... .... ...{0} ....) {1}", this.CheckingDisabled ? 1 : 0, this.CheckingDisabled ? "Checking Disabled" : "Not CheckingEnabled");
            Console.WriteLine("RCode:             (.... .... .... {0}{1}{2}{3}) {4}", (this.RCode & 1) > 1 ? 1 : 0, (this.RCode & 2) > 1 ? 1 : 0, (this.RCode & 4) > 1 ? 1 : 0, (this.RCode & 8) > 1 ? 1 : 0, (RCode) (this.RCode));
            Console.WriteLine("QuestionCount:     0x{0:X4}", this.QuestionCount);
            Console.WriteLine("AnswerCount:       0x{0:X4}", this.AnswerCount);
            Console.WriteLine("NameServerCount:   0x{0:X4}", this.NameServerCount);
            Console.WriteLine("AdditionalCount:   0x{0:X4}", this.AdditionalCount);
            Console.WriteLine();

            if (Questions != null)
            {
                foreach (Question question in this.Questions)
                {
                    Console.WriteLine("QRecord: {0} of type {1} on class {2}", question.Name, (ResourceType)question.Type, (ResourceClass)question.Class);
                }
                Console.WriteLine();
            }

            if (Answers != null)
            {
                foreach (ResourceRecord resource in this.Answers)
                {
                    Console.WriteLine("Record: {0} of type {1} on class {2}", resource.Name, (ResourceType) resource.Type, (ResourceClass)resource.Class);
                    resource.Dump();
                    Console.WriteLine();
                }
            }

            if (Authorities != null)
            {
                foreach (ResourceRecord resource in this.Authorities)
                {
                    Console.WriteLine("Record: {0} of type {1} on class {2}", resource.Name, (ResourceType)resource.Type, (ResourceClass)resource.Class);
                    resource.Dump();
                    Console.WriteLine();
                }
            }

        }

        public byte[] GetBytes()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                this.WriteToStream(stream);
                return stream.GetBuffer();
            }
        }

        public void WriteToStream(Stream stream)
        {
            // write header
            stream.Write(this._header, 0, _header.Length);
            Questions.WriteToStream(stream);
            Answers.WriteToStream(stream);
            Authorities.WriteToStream(stream);
            Additionals.WriteToStream(stream);
        }

        public static bool TryParse(byte[] bytes, out DnsMessage query)
        {
            try
            {
                query = Parse(bytes);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                query = null;
                return false;
            }
        }
    }
}