// // //-------------------------------------------------------------------------------------------------
// // // <copyright file="BindZoneProviderTests.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace DnsTest
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Dns;
    using Dns.ZoneProvider.Bind;
    using DnsTest.Integration;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class BindZoneProviderTests
    {
        [Fact]
        public void GenerateZone_ReturnsZoneRecordsFromBindFile()
        {
            string zoneFile = Path.Combine(TestProjectPaths.TestDataDirectory, "Bind", "simple.zone");

            using var provider = this.CreateProvider(zoneFile);
            Zone zone = provider.GenerateZone();

            Assert.NotNull(zone);
            Assert.Equal(".example.com", zone.Suffix);
            Assert.Equal(0u, zone.Serial);

            ZoneRecord wwwA = Assert.Single(zone.Where(record => record.Host == "www.example.com" && record.Type == ResourceType.A));
            Assert.Equal(IPAddress.Parse("192.0.2.10"), Assert.Single(wwwA.Addresses));

            ZoneRecord wwwAaaa = Assert.Single(zone.Where(record => record.Host == "www.example.com" && record.Type == ResourceType.AAAA));
            Assert.Equal(IPAddress.Parse("2001:db8::10"), Assert.Single(wwwAaaa.Addresses));

            ZoneRecord apex = Assert.Single(zone.Where(record => record.Host == "example.com" && record.Type == ResourceType.A));
            Assert.Contains(IPAddress.Parse("192.0.2.20"), apex.Addresses);

            ZoneRecord api = Assert.Single(zone.Where(record => record.Host == "api.example.com"));
            Assert.Equal(IPAddress.Parse("192.0.2.30"), Assert.Single(api.Addresses));
        }

        [Fact]
        public void GenerateZone_InvalidZoneReturnsNull()
        {
            string zoneFile = Path.Combine(TestProjectPaths.TestDataDirectory, "Bind", "invalid_missing_ttl.zone");

            using var provider = this.CreateProvider(zoneFile);
            Zone zone = provider.GenerateZone();

            Assert.Null(zone);
        }

        [Fact]
        public void GenerateZone_ReturnsNullWhenCNameConflictsWithAddress()
        {
            string tempZone = this.WriteTempZoneFile(new[]
            {
                "$TTL 1h",
                "$ORIGIN example.com.",
                "@ IN SOA ns1.example.com. hostmaster.example.com. (",
                "    2024010101",
                "    7200",
                "    3600",
                "    1209600",
                "    3600 )",
                "@ IN NS ns1.example.com.",
                "www IN CNAME api",
                "www IN A 192.0.2.40",
                "api IN A 192.0.2.50"
            });

            try
            {
                using var provider = this.CreateProvider(tempZone);
                Zone zone = provider.GenerateZone();

                Assert.Null(zone);
            }
            finally
            {
                File.Delete(tempZone);
            }
        }

        private BindZoneProvider CreateProvider(string zoneFile)
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "FileName", zoneFile }
                })
                .Build();

            var provider = new BindZoneProvider();
            provider.Initialize(config, ".example.com");
            return provider;
        }

        private string WriteTempZoneFile(IEnumerable<string> lines)
        {
            string path = Path.GetTempFileName();
            File.WriteAllLines(path, lines);
            return path;
        }
    }
}
