// //-------------------------------------------------------------------------------------------------
// // <copyright file="DnsProtocolBenchmarks.cs" company="stephbu">
// // Copyright (c) Steve Butler. All rights reserved.
// // </copyright>
// //-------------------------------------------------------------------------------------------------

namespace DnsBench
{
    using BenchmarkDotNet.Attributes;
    using Dns;

    /// <summary>
    /// Benchmarks for DnsProtocol parsing methods.
    /// Focus: ReadString allocation overhead and string parsing performance.
    /// </summary>
    [MemoryDiagnoser]
    public class DnsProtocolBenchmarks
    {
        // Simple domain: www.msn.com (3 labels)
        private byte[] _simpleDomain;
        private int _simpleDomainOffset;

        // Medium domain: www.msn.com.redmond.corp.microsoft.com (7 labels)
        private byte[] _mediumDomain;
        private int _mediumDomainOffset;

        // Domain with compression pointer
        private byte[] _compressedDomain;
        private int _compressedDomainOffset;

        // Full DNS query message containing domain
        private byte[] _fullQueryMessage;

        [GlobalSetup]
        public void Setup()
        {
            // Simple domain: www.msn.com
            // Format: [3]www[3]msn[3]com[0]
            _simpleDomain = new byte[]
            {
                0x03, 0x77, 0x77, 0x77,  // 3, w, w, w
                0x03, 0x6D, 0x73, 0x6E,  // 3, m, s, n
                0x03, 0x63, 0x6F, 0x6D,  // 3, c, o, m
                0x00                      // null terminator
            };
            _simpleDomainOffset = 0;

            // Medium domain: www.msn.com.redmond.corp.microsoft.com
            _mediumDomain = new byte[]
            {
                0x03, 0x77, 0x77, 0x77,              // www
                0x03, 0x6D, 0x73, 0x6E,              // msn
                0x03, 0x63, 0x6F, 0x6D,              // com
                0x07, 0x72, 0x65, 0x64, 0x6D, 0x6F, 0x6E, 0x64,  // redmond
                0x04, 0x63, 0x6F, 0x72, 0x70,        // corp
                0x09, 0x6D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74,  // microsoft
                0x03, 0x63, 0x6F, 0x6D,              // com
                0x00                                 // null terminator
            };
            _mediumDomainOffset = 0;

            // Compressed domain: Response with pointer to earlier label
            // This is from the actual test: www.msn.com response with CNAME
            _compressedDomain = new byte[]
            {
                0x00, 0x04, 0x81, 0x80, 0x00, 0x01, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00,
                0x03, 0x77, 0x77, 0x77,  // www
                0x03, 0x6D, 0x73, 0x6E,  // msn
                0x03, 0x63, 0x6F, 0x6D,  // com
                0x00,                    // null terminator
                0x00, 0x01, 0x00, 0x01,  // type A, class IN
                0xC0, 0x0C,              // compression pointer to offset 12 (www.msn.com)
                0x00, 0x05, 0x00, 0x01,  // type CNAME, class IN
                0x00, 0x00, 0x02, 0x35,  // TTL
                0x00, 0x1E,              // data length
                0x02, 0x75, 0x73,        // us
                0x03, 0x63, 0x6F, 0x31,  // co1
                0x03, 0x63, 0x62, 0x33,  // cb3
                0x06, 0x67, 0x6C, 0x62, 0x64, 0x6E, 0x73,  // glbdns
                0x09, 0x6D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74,  // microsoft
                0xC0, 0x14,              // pointer to .com
                0xC0, 0x29,              // pointer to us.co1.cb3...
                0x00, 0x01, 0x00, 0x01,
                0x00, 0x00, 0x00, 0x53,
                0x00, 0x04,
                0x83, 0xFD, 0x0D, 0x8C
            };
            // Point to the compression pointer at offset 29 (0xC0, 0x0C)
            _compressedDomainOffset = 29;

            // Full query message for parsing benchmark
            _fullQueryMessage = new byte[]
            {
                0xD3, 0x03, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x07, 0x64, 0x64, 0x63, 0x64, 0x73, 0x30, 0x31,  // ddcds01
                0x07, 0x72, 0x65, 0x64, 0x6D, 0x6F, 0x6E, 0x64,  // redmond
                0x04, 0x63, 0x6F, 0x72, 0x70,                    // corp
                0x09, 0x6D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74,  // microsoft
                0x03, 0x63, 0x6F, 0x6D,                          // com
                0x00,
                0x00, 0x01, 0x00, 0x01  // type A, class IN
            };
        }

        [Benchmark(Description = "ReadString: Simple (www.msn.com)")]
        public string ReadString_Simple()
        {
            int offset = _simpleDomainOffset;
            return DnsProtocol.ReadString(_simpleDomain, ref offset);
        }

        [Benchmark(Description = "ReadString: Medium (7 labels)")]
        public string ReadString_Medium()
        {
            int offset = _mediumDomainOffset;
            return DnsProtocol.ReadString(_mediumDomain, ref offset);
        }

        [Benchmark(Description = "ReadString: Compressed pointer")]
        public string ReadString_Compressed()
        {
            int offset = _compressedDomainOffset;
            return DnsProtocol.ReadString(_compressedDomain, ref offset);
        }

        [Benchmark(Description = "ReadUshort")]
        public ushort ReadUshort()
        {
            int offset = 0;
            return DnsProtocol.ReadUshort(_fullQueryMessage, ref offset);
        }

        [Benchmark(Description = "ReadUint")]
        public uint ReadUint()
        {
            int offset = 0;
            return DnsProtocol.ReadUint(_fullQueryMessage, ref offset);
        }
    }
}
