// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="DnsProtocolTest.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace DnsTest
{
    using System.IO;
    using System.Linq;
    using System.Net;
    using Dns;
    using Xunit;

    public class DnsProtocolTest
    {
        [Fact]
        public void DnsQuery()
        {
            byte[] sampleQuery = new byte[] {0xD3, 0x03, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x64, 0x64, 0x63, 0x64, 0x73, 0x30, 0x31, 0x07, 0x72, 0x65, 0x64, 0x6D, 0x6F, 0x6E, 0x64, 0x04, 0x63, 0x6F, 0x72, 0x70, 0x09, 0x6D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x03, 0x63, 0x6F, 0x6D, 0x00, 0x00, 0x01, 0x00, 0x01};
            DnsMessage query;
            Assert.True(DnsMessage.TryParse(sampleQuery, out query));
            query.Dump();
        }

        [Fact]
        public void DnsQuery2()
        {
            byte[] sampleQuery = new byte[] {0x00, 0x03, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x77, 0x77, 0x77, 0x03, 0x6D, 0x73, 0x6E, 0x03, 0x63, 0x6F, 0x6D, 0x07, 0x72, 0x65, 0x64, 0x6D, 0x6F, 0x6E, 0x64, 0x04, 0x63, 0x6F, 0x72, 0x70, 0x09, 0x6D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x03, 0x63, 0x6F, 0x6D, 0x00, 0x00, 0x1C, 0x00, 0x01};
            DnsMessage query;
            Assert.True(DnsMessage.TryParse(sampleQuery, out query));

            // Header Checks
            Assert.Equal(0x3, query.QueryIdentifier);
            Assert.False(query.QR);
            Assert.Equal(0x0000, query.Opcode);
            Assert.False(query.AA);
            Assert.False(query.TC);
            Assert.True(query.RD);
            Assert.False(query.RA);
            Assert.False(query.Zero);
            Assert.False(query.AuthenticatingData);
            Assert.False(query.CheckingDisabled);
            Assert.Equal(0x0000, query.RCode);
            Assert.Equal(0x0001, query.QuestionCount);
            Assert.Equal(0x0000, query.AnswerCount);
            Assert.Equal(0x0000, query.NameServerCount);
            Assert.Equal(0x0000, query.AdditionalCount);

            // Question Checks
            Assert.Equal(query.QuestionCount, query.Questions.Count());

            // Q1
            Assert.Equal("www.msn.com.redmond.corp.microsoft.com", query.Questions[0].Name);
            Assert.Equal(ResourceType.AAAA, query.Questions[0].Type);
            Assert.Equal(ResourceClass.IN, query.Questions[0].Class);

            // dump results
            query.Dump();
        }

        [Fact]
        public void DnsResponse1()
        {
            byte[] sampleQuery = new byte[] {0x44, 0xFD, 0x81, 0x80, 0x00, 0x01, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x03, 0x77, 0x77, 0x77, 0x10, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65, 0x2D, 0x61, 0x6E, 0x61, 0x6C, 0x79, 0x74, 0x69, 0x63, 0x73, 0x03, 0x63, 0x6F, 0x6D, 0x00, 0x00, 0x01, 0x00, 0x01, 0xC0, 0x0C, 0x00, 0x05, 0x00, 0x01, 0x00, 0x00, 0x89, 0x89, 0x00, 0x20, 0x14, 0x77, 0x77, 0x77, 0x2D, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65, 0x2D, 0x61, 0x6E, 0x61, 0x6C, 0x79, 0x74, 0x69, 0x63, 0x73, 0x01, 0x6C, 0x06, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65, 0xC0, 0x21, 0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x25, 0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x21, 0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x28, 0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x29, 0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x20, 0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x2E, 0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x26, 0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x24, 0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x27, 0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x22, 0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x23};
            DnsMessage query;
            Assert.True(DnsMessage.TryParse(sampleQuery, out query));
            query.Dump();
        }

        // Response Contains Compression information
        [Fact]
        public void DnsResponse2()
        {
            byte[] sampleQuery = new byte[] {0x00, 0x04, 0x81, 0x80, 0x00, 0x01, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x03, 0x77, 0x77, 0x77, 0x03, 0x6D, 0x73, 0x6E, 0x03, 0x63, 0x6F, 0x6D, 0x00, 0x00, 0x01, 0x00, 0x01, 0xC0, 0x0C, 0x00, 0x05, 0x00, 0x01, 0x00, 0x00, 0x02, 0x35, 0x00, 0x1E, 0x02, 0x75, 0x73, 0x03, 0x63, 0x6F, 0x31, 0x03, 0x63, 0x62, 0x33, 0x06, 0x67, 0x6C, 0x62, 0x64, 0x6E, 0x73, 0x09, 0x6D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0xC0, 0x14, 0xC0, 0x29, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x53, 0x00, 0x04, 0x83, 0xFD, 0x0D, 0x8C};
            DnsMessage query;
            Assert.True(DnsMessage.TryParse(sampleQuery, out query));

            // Header Checks
            Assert.Equal(0x4, query.QueryIdentifier);
            Assert.True(query.QR);
            Assert.Equal(0x0000, query.Opcode);
            Assert.False(query.AA);
            Assert.False(query.TC);
            Assert.True(query.RD);
            Assert.True(query.RA);
            Assert.False(query.Zero);
            Assert.False(query.AuthenticatingData);
            Assert.False(query.CheckingDisabled);
            Assert.Equal(0x0000, query.RCode);
            Assert.Equal(0x0001, query.QuestionCount);
            Assert.Equal(0x0002, query.AnswerCount);
            Assert.Equal(0x0000, query.NameServerCount);
            Assert.Equal(0x0000, query.AdditionalCount);

            // Question Checks
            Assert.Equal(query.QuestionCount, query.Questions.Count());

            // Q1
            Assert.Equal("www.msn.com", query.Questions[0].Name);
            Assert.Equal(ResourceType.A, query.Questions[0].Type);
            Assert.Equal(ResourceClass.IN, query.Questions[0].Class);

            // dump results
            query.Dump();
        }

        // Response Contains Compression information
        [Fact]
        public void DnsResponse3()
        {
            byte[] sampleQuery = new byte[] {0xDD, 0x15, 0x81, 0x80, 0x00, 0x01, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x03, 0x61, 0x70, 0x69, 0x04, 0x62, 0x69, 0x6E, 0x67, 0x03, 0x63, 0x6F, 0x6D, 0x00, 0x00, 0x01, 0x00, 0x01, 0xC0, 0x0C, 0x00, 0x05, 0x00, 0x01, 0x00, 0x00, 0x00, 0x83, 0x00, 0x14, 0x04, 0x61, 0x31, 0x33, 0x34, 0x02, 0x6C, 0x6D, 0x06, 0x61, 0x6B, 0x61, 0x6D, 0x61, 0x69, 0x03, 0x6E, 0x65, 0x74, 0x00, 0xC0, 0x2A, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0E, 0x00, 0x04, 0xCF, 0x6D, 0x49, 0x91, 0xC0, 0x2A, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0E, 0x00, 0x04, 0xCF, 0x6D, 0x49, 0x51};
            DnsMessage query;
            Assert.True(DnsMessage.TryParse(sampleQuery, out query));

            // Header Checks
            Assert.Equal(0xDD15, query.QueryIdentifier);
            Assert.True(query.QR);
            Assert.Equal(0x0000, query.Opcode);
            Assert.False(query.AA);
            Assert.False(query.TC);
            Assert.True(query.RD);
            Assert.True(query.RA);
            Assert.False(query.Zero);
            Assert.False(query.AuthenticatingData);
            Assert.False(query.CheckingDisabled);
            Assert.Equal(0x0000, query.RCode);
            Assert.Equal(0x0001, query.QuestionCount);
            Assert.Equal(0x0003, query.AnswerCount);
            Assert.Equal(0x0000, query.NameServerCount);
            Assert.Equal(0x0000, query.AdditionalCount);

            // Question Checks
            Assert.Equal(query.QuestionCount, query.Questions.Count());

            // Q1
            Assert.Equal("api.bing.com", query.Questions[0].Name);
            Assert.Equal(ResourceType.A, query.Questions[0].Type);
            Assert.Equal(ResourceClass.IN, query.Questions[0].Class);

            // Answer Checks
            Assert.Equal(query.AnswerCount, query.Answers.Count());

            // A1
            Assert.Equal("api.bing.com", query.Answers[0].Name);
            Assert.Equal(ResourceType.CNAME, query.Answers[0].Type);
            Assert.Equal(ResourceClass.IN, query.Answers[0].Class);
            Assert.True(query.Answers[0].TTL == 0x83);
            Assert.Equal(0x14, query.Answers[0].DataLength);
            Assert.Equal(typeof (CNameRData), query.Answers[0].RData.GetType());

            // dump results
            query.Dump();
        }

        // Response Contains Compression information
        [Fact]
        public void DnsQuery3()
        {
            byte[] sampleQuery = new byte[] {0xFB, 0x65, 0x84, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x73, 0x65, 0x63, 0x75, 0x72, 0x65, 0x2D, 0x75, 0x73, 0x0C, 0x69, 0x6D, 0x72, 0x77, 0x6F, 0x72, 0x6C, 0x64, 0x77, 0x69, 0x64, 0x65, 0x03, 0x63, 0x6F, 0x6D, 0x00, 0x00, 0x1C, 0x00, 0x01};
            DnsMessage query;
            Assert.True(DnsMessage.TryParse(sampleQuery, out query));

            // Header Checks
            Assert.Equal(0xFB65, query.QueryIdentifier);
            Assert.True(query.QR);
            Assert.Equal(0x0000, query.Opcode);
            Assert.True(query.AA);
            Assert.False(query.TC);
            Assert.False(query.RD);
            Assert.False(query.RA);
            Assert.False(query.Zero);
            Assert.False(query.AuthenticatingData);
            Assert.False(query.CheckingDisabled);
            Assert.Equal(0x0000, query.RCode);
            Assert.Equal(0x0001, query.QuestionCount);
            Assert.Equal(0x0000, query.AnswerCount);
            Assert.Equal(0x0000, query.NameServerCount);
            Assert.Equal(0x0000, query.AdditionalCount);

            // Question Checks
            Assert.Equal(query.QuestionCount, query.Questions.Count());

            // Q1
            Assert.Equal("secure-us.imrworldwide.com", query.Questions[0].Name);
            Assert.Equal(ResourceType.AAAA, query.Questions[0].Type);
            Assert.Equal(ResourceClass.IN, query.Questions[0].Class);

            // dump results
            query.Dump();
        }

        [Fact]
        public void TransitiveQueryTest()
        {
            DnsMessage message = new DnsMessage();
            message.QueryIdentifier = 0xFEED;
            message.QR = false;
            message.Opcode = (byte) OpCode.QUERY;
            message.AA = false;
            message.TC = false;
            message.RD = true;
            message.RA = false;
            message.Zero = false;
            message.AuthenticatingData = false;
            message.CheckingDisabled = false;
            message.RCode = 0x0000;
            message.QuestionCount = 1;
            message.AnswerCount = 0;
            message.NameServerCount = 0;
            message.AdditionalCount = 0;
            message.Questions = new QuestionList();
            message.Questions.Add(new Question {Name = "www.msn.com", Class = ResourceClass.IN, Type = ResourceType.A});

            DnsMessage outMessage;
            using (MemoryStream stream = new MemoryStream())
            {
                message.WriteToStream(stream);
                Assert.True(DnsMessage.TryParse(stream.GetBuffer(), out outMessage));
            }

            Assert.Equal(0xFEED, outMessage.QueryIdentifier);
            Assert.False(outMessage.QR);
            Assert.Equal((byte) OpCode.QUERY, outMessage.Opcode);
            Assert.False(outMessage.AA);
            Assert.False(outMessage.TC);
            Assert.True(outMessage.RD);
            Assert.False(outMessage.RA);
            Assert.False(outMessage.Zero);
            Assert.False(outMessage.AuthenticatingData);
            Assert.False(outMessage.CheckingDisabled);
            Assert.Equal(0x0000, outMessage.RCode);
            Assert.Equal(0x0001, outMessage.QuestionCount);
            Assert.Equal(0x0000, outMessage.AnswerCount);
            Assert.Equal(0x0000, outMessage.NameServerCount);
            Assert.Equal(0x0000, outMessage.AdditionalCount);

            // Question Checks
            Assert.Equal(outMessage.QuestionCount, outMessage.Questions.Count());

            // Q1
            Assert.Equal("www.msn.com", outMessage.Questions[0].Name);
            Assert.Equal(ResourceType.A, outMessage.Questions[0].Type);
            Assert.Equal(ResourceClass.IN, outMessage.Questions[0].Class);
        }

        [Fact]
        public void TransitiveQueryTest2()
        {
            DnsMessage message = new DnsMessage();
            message.QueryIdentifier = 0xFEED;
            message.QR = false;
            message.Opcode = (byte) OpCode.QUERY;
            message.AA = false;
            message.TC = false;
            message.RD = true;
            message.RA = false;
            message.Zero = false;
            message.AuthenticatingData = false;
            message.CheckingDisabled = false;
            message.RCode = 0x0000;
            message.QuestionCount = 1;
            message.AnswerCount = 2;
            message.NameServerCount = 0;
            message.AdditionalCount = 0;
            message.Questions = new QuestionList();
            message.Questions.Add(new Question {Name = "www.msn.com", Class = ResourceClass.IN, Type = ResourceType.A});
            message.Answers.Add(new ResourceRecord {Name = "8.8.8.8", Class = ResourceClass.IN, Type = ResourceType.NS, TTL = 468, DataLength = 0, RData = null});
            RData data = new ANameRData {Address = IPAddress.Parse("8.8.8.9")};
            message.Answers.Add(new ResourceRecord {Name = "8.8.8.9", Class = ResourceClass.IN, Type = ResourceType.NS, TTL = 468, RData = data, DataLength = (ushort) data.Length});

            DnsMessage outMessage;
            using (MemoryStream stream = new MemoryStream())
            {
                message.WriteToStream(stream);
                Assert.True(DnsMessage.TryParse(stream.GetBuffer(), out outMessage));
            }

            Assert.Equal(0xFEED, outMessage.QueryIdentifier);
            Assert.False(outMessage.QR);
            Assert.Equal((byte) OpCode.QUERY, outMessage.Opcode);
            Assert.False(outMessage.AA);
            Assert.False(outMessage.TC);
            Assert.True(outMessage.RD);
            Assert.False(outMessage.RA);
            Assert.False(outMessage.Zero);
            Assert.False(outMessage.AuthenticatingData);
            Assert.False(outMessage.CheckingDisabled);
            Assert.Equal(0x0000, outMessage.RCode);
            Assert.Equal(0x0001, outMessage.QuestionCount);
            Assert.Equal(0x0002, outMessage.AnswerCount);
            Assert.Equal(0x0000, outMessage.NameServerCount);
            Assert.Equal(0x0000, outMessage.AdditionalCount);

            // Question Checks
            Assert.Equal(outMessage.QuestionCount, outMessage.Questions.Count());

            // Q1
            Assert.Equal("www.msn.com", outMessage.Questions[0].Name);
            Assert.Equal(ResourceType.A, outMessage.Questions[0].Type);
            Assert.Equal(ResourceClass.IN, outMessage.Questions[0].Class);

            Assert.Equal(outMessage.AnswerCount, outMessage.Answers.Count());
            Assert.Equal(outMessage.AnswerCount, outMessage.Answers.Count());
            Assert.Equal("8.8.8.8", outMessage.Answers[0].Name);
            Assert.Equal("8.8.8.9", outMessage.Answers[1].Name);
        }

        [Fact]
        public void Opcode()
        {
            DnsMessage message = new DnsMessage();
            message.QR = true;
            Assert.Equal(0x8000, message.Flags);
            message.Opcode = (byte) OpCode.UPDATE;
            Assert.Equal((byte) OpCode.UPDATE, message.Opcode);
            Assert.Equal(0xa800, message.Flags);
        }
    }
}