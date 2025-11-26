// //-------------------------------------------------------------------------------------------------
// // <copyright file="DnsMessageBenchmarks.cs" company="stephbu">
// // Copyright (c) Steve Butler. All rights reserved.
// // </copyright>
// //-------------------------------------------------------------------------------------------------

namespace DnsBench
{
    using System.IO;
    using System.Net;
    using BenchmarkDotNet.Attributes;
    using Dns;

    /// <summary>
    /// Benchmarks for DnsMessage parsing and serialization.
    /// Focus: Full message parse/write cycle, MemoryStream allocations.
    /// </summary>
    [MemoryDiagnoser]
    public class DnsMessageBenchmarks
    {
        // Simple A query (ddcds01.redmond.corp.microsoft.com)
        private byte[] _simpleQuery;

        // Query with AAAA type
        private byte[] _aaaaQuery;

        // Response with single A record
        private byte[] _simpleResponse;

        // Response with CNAME + A records (compression)
        private byte[] _cnameResponse;

        // Large response: google-analytics with 12 answers
        private byte[] _largeResponse;

        // Pre-built message for serialization benchmarks
        private DnsMessage _queryMessage;
        private DnsMessage _responseMessage;

        // Reusable MemoryStream for write benchmarks
        private MemoryStream _reusableStream;

        [GlobalSetup]
        public void Setup()
        {
            // Simple query: ddcds01.redmond.corp.microsoft.com A IN
            _simpleQuery = new byte[]
            {
                0xD3, 0x03, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x07, 0x64, 0x64, 0x63, 0x64, 0x73, 0x30, 0x31,
                0x07, 0x72, 0x65, 0x64, 0x6D, 0x6F, 0x6E, 0x64,
                0x04, 0x63, 0x6F, 0x72, 0x70,
                0x09, 0x6D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74,
                0x03, 0x63, 0x6F, 0x6D,
                0x00, 0x00, 0x01, 0x00, 0x01
            };

            // AAAA query: www.msn.com.redmond.corp.microsoft.com AAAA IN
            _aaaaQuery = new byte[]
            {
                0x00, 0x03, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x03, 0x77, 0x77, 0x77,
                0x03, 0x6D, 0x73, 0x6E,
                0x03, 0x63, 0x6F, 0x6D,
                0x07, 0x72, 0x65, 0x64, 0x6D, 0x6F, 0x6E, 0x64,
                0x04, 0x63, 0x6F, 0x72, 0x70,
                0x09, 0x6D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74,
                0x03, 0x63, 0x6F, 0x6D,
                0x00, 0x00, 0x1C, 0x00, 0x01
            };

            // Simple response: example.com A 192.0.2.10
            _simpleResponse = new byte[]
            {
                0x12, 0x34, 0x81, 0x80, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
                0x07, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65,
                0x03, 0x63, 0x6F, 0x6D, 0x00,
                0x00, 0x01, 0x00, 0x01,
                0xC0, 0x0C, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x04,
                0xC0, 0x00, 0x02, 0x0A
            };

            // CNAME response: www.msn.com with CNAME + A record
            _cnameResponse = new byte[]
            {
                0x00, 0x04, 0x81, 0x80, 0x00, 0x01, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00,
                0x03, 0x77, 0x77, 0x77,
                0x03, 0x6D, 0x73, 0x6E,
                0x03, 0x63, 0x6F, 0x6D,
                0x00, 0x00, 0x01, 0x00, 0x01,
                0xC0, 0x0C, 0x00, 0x05, 0x00, 0x01, 0x00, 0x00, 0x02, 0x35, 0x00, 0x1E,
                0x02, 0x75, 0x73, 0x03, 0x63, 0x6F, 0x31, 0x03, 0x63, 0x62, 0x33,
                0x06, 0x67, 0x6C, 0x62, 0x64, 0x6E, 0x73,
                0x09, 0x6D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74,
                0xC0, 0x14,
                0xC0, 0x29, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x53, 0x00, 0x04,
                0x83, 0xFD, 0x0D, 0x8C
            };

            // Large response: www.google-analytics.com with 12 answer records
            _largeResponse = new byte[]
            {
                0x44, 0xFD, 0x81, 0x80, 0x00, 0x01, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x00,
                0x03, 0x77, 0x77, 0x77,
                0x10, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65, 0x2D, 0x61, 0x6E, 0x61, 0x6C, 0x79, 0x74, 0x69, 0x63, 0x73,
                0x03, 0x63, 0x6F, 0x6D, 0x00,
                0x00, 0x01, 0x00, 0x01,
                0xC0, 0x0C, 0x00, 0x05, 0x00, 0x01, 0x00, 0x00, 0x89, 0x89, 0x00, 0x20,
                0x14, 0x77, 0x77, 0x77, 0x2D, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65, 0x2D, 0x61, 0x6E, 0x61, 0x6C, 0x79, 0x74, 0x69, 0x63, 0x73,
                0x01, 0x6C, 0x06, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65, 0xC0, 0x21,
                0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x25,
                0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x21,
                0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x28,
                0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x29,
                0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x20,
                0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x2E,
                0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x26,
                0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x24,
                0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x27,
                0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x22,
                0xC0, 0x36, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x19, 0x00, 0x04, 0xAD, 0xC2, 0x21, 0x23
            };

            // Build query message for serialization
            _queryMessage = new DnsMessage
            {
                QueryIdentifier = 0xFEED,
                QR = false,
                Opcode = (byte)OpCode.QUERY,
                AA = false,
                TC = false,
                RD = true,
                RA = false,
                QuestionCount = 1,
                AnswerCount = 0,
                NameServerCount = 0,
                AdditionalCount = 0
            };
            _queryMessage.Questions.Add(new Question
            {
                Name = "www.example.com",
                Class = ResourceClass.IN,
                Type = ResourceType.A
            });

            // Build response message for serialization
            _responseMessage = new DnsMessage
            {
                QueryIdentifier = 0xFEED,
                QR = true,
                Opcode = (byte)OpCode.QUERY,
                AA = true,
                TC = false,
                RD = true,
                RA = false,
                QuestionCount = 1,
                AnswerCount = 2,
                NameServerCount = 0,
                AdditionalCount = 0
            };
            _responseMessage.Questions.Add(new Question
            {
                Name = "www.example.com",
                Class = ResourceClass.IN,
                Type = ResourceType.A
            });
            var rdata1 = new ANameRData { Address = IPAddress.Parse("192.0.2.1") };
            _responseMessage.Answers.Add(new ResourceRecord
            {
                Name = "www.example.com",
                Class = ResourceClass.IN,
                Type = ResourceType.A,
                TTL = 300,
                RData = rdata1,
                DataLength = rdata1.Length
            });
            var rdata2 = new ANameRData { Address = IPAddress.Parse("192.0.2.2") };
            _responseMessage.Answers.Add(new ResourceRecord
            {
                Name = "www.example.com",
                Class = ResourceClass.IN,
                Type = ResourceType.A,
                TTL = 300,
                RData = rdata2,
                DataLength = rdata2.Length
            });

            _reusableStream = new MemoryStream(512);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _reusableStream?.Dispose();
        }

        // ========== Parsing Benchmarks ==========

        [Benchmark(Description = "Parse: Simple query")]
        public DnsMessage Parse_SimpleQuery()
        {
            DnsMessage.TryParse(_simpleQuery, out var message);
            return message;
        }

        [Benchmark(Description = "Parse: AAAA query")]
        public DnsMessage Parse_AAAAQuery()
        {
            DnsMessage.TryParse(_aaaaQuery, out var message);
            return message;
        }

        [Benchmark(Description = "Parse: Simple response (1 A)")]
        public DnsMessage Parse_SimpleResponse()
        {
            DnsMessage.TryParse(_simpleResponse, out var message);
            return message;
        }

        [Benchmark(Description = "Parse: CNAME response (2 records)")]
        public DnsMessage Parse_CnameResponse()
        {
            DnsMessage.TryParse(_cnameResponse, out var message);
            return message;
        }

        [Benchmark(Description = "Parse: Large response (12 records)")]
        public DnsMessage Parse_LargeResponse()
        {
            DnsMessage.TryParse(_largeResponse, out var message);
            return message;
        }

        // ========== Serialization Benchmarks ==========

        [Benchmark(Description = "Write: Query (new MemoryStream)")]
        public byte[] Write_Query_NewStream()
        {
            using var stream = new MemoryStream(512);
            _queryMessage.WriteToStream(stream);
            return stream.GetBuffer();
        }

        [Benchmark(Description = "Write: Response (new MemoryStream)")]
        public byte[] Write_Response_NewStream()
        {
            using var stream = new MemoryStream(512);
            _responseMessage.WriteToStream(stream);
            return stream.GetBuffer();
        }

        [Benchmark(Description = "Write: Query (reused MemoryStream)")]
        public byte[] Write_Query_ReusedStream()
        {
            _reusableStream.Position = 0;
            _reusableStream.SetLength(0);
            _queryMessage.WriteToStream(_reusableStream);
            return _reusableStream.GetBuffer();
        }

        [Benchmark(Description = "Write: Response (reused MemoryStream)")]
        public byte[] Write_Response_ReusedStream()
        {
            _reusableStream.Position = 0;
            _reusableStream.SetLength(0);
            _responseMessage.WriteToStream(_reusableStream);
            return _reusableStream.GetBuffer();
        }

        // ========== Round-trip Benchmarks ==========

        [Benchmark(Description = "Round-trip: Parse + Write query")]
        public byte[] RoundTrip_Query()
        {
            DnsMessage.TryParse(_simpleQuery, out var message);
            using var stream = new MemoryStream(512);
            message.WriteToStream(stream);
            return stream.GetBuffer();
        }

        [Benchmark(Description = "Round-trip: Parse + Write response")]
        public byte[] RoundTrip_Response()
        {
            DnsMessage.TryParse(_cnameResponse, out var message);
            using var stream = new MemoryStream(512);
            message.WriteToStream(stream);
            return stream.GetBuffer();
        }
    }
}
