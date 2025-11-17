// // //-------------------------------------------------------------------------------------------------
// // // <copyright file="DnsCliAuthoritativeBehaviorTests.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace DnsTest
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Dns;
    using DnsTest.Integration;
    using Xunit;

    [Collection(DnsCliIntegrationCollection.Name)]
    public sealed class DnsCliAuthoritativeBehaviorTests
    {
        private static readonly IPAddress PrimaryHostAddress = IPAddress.Parse("192.0.2.10");
        private static readonly IPAddress[] RoundRobinAddresses =
        {
            IPAddress.Parse("192.0.2.11"),
            IPAddress.Parse("192.0.2.12"),
            IPAddress.Parse("192.0.2.13")
        };

        private readonly DnsCliHostFixture _fixture;

        public DnsCliAuthoritativeBehaviorTests(DnsCliHostFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task InZoneQueriesReturnAuthoritativeAnswers()
        {
            string hostName = _fixture.BuildHostName("alpha");
            DnsMessage response = await _fixture.Client.QueryAsync(hostName);

            Assert.True(response.QR);
            Assert.True(response.AA);
            Assert.False(response.RA);
            Assert.Equal(RCode.NOERROR, (RCode)response.RCode);
            Assert.Equal((ushort)1, response.AnswerCount);

            ResourceRecord answer = Assert.Single(response.Answers);
            Assert.Equal(hostName, answer.Name);
            Assert.Equal(ResourceType.A, answer.Type);
            Assert.Equal(ResourceClass.IN, answer.Class);
            Assert.Equal((uint)10, answer.TTL);
            ANameRData address = Assert.IsType<ANameRData>(answer.RData);
            Assert.Equal(PrimaryHostAddress, address.Address);
        }

        [Fact]
        public async Task RecursionDesiredFlagDoesNotGrantRecursionAvailability()
        {
            string hostName = _fixture.BuildHostName("alpha");
            DnsMessage response = await _fixture.Client.QueryAsync(hostName, recursionDesired: true);

            Assert.True(response.RD);
            Assert.False(response.RA);
            Assert.True(response.AA);
        }

        [Fact]
        public async Task RoundRobinHostsRotateAddressesAcrossQueries()
        {
            string hostName = _fixture.BuildHostName("round");
            List<IPAddress> firstAnswers = new List<IPAddress>();

            for (int iteration = 0; iteration < RoundRobinAddresses.Length; iteration++)
            {
                DnsMessage response = await _fixture.Client.QueryAsync(hostName);
                ResourceRecord firstRecord = response.Answers.First();
                firstAnswers.Add(Assert.IsType<ANameRData>(firstRecord.RData).Address);
            }

            Assert.Equal(RoundRobinAddresses, firstAnswers);
        }

        [Fact]
        public async Task PositiveResponsesKeepConfiguredTtl()
        {
            string hostName = _fixture.BuildHostName("alpha");

            DnsMessage firstResponse = await _fixture.Client.QueryAsync(hostName);
            DnsMessage secondResponse = await _fixture.Client.QueryAsync(hostName);

            Assert.Equal((uint)10, Assert.Single(firstResponse.Answers).TTL);
            Assert.Equal((uint)10, Assert.Single(secondResponse.Answers).TTL);
            Assert.True(firstResponse.AA);
            Assert.True(secondResponse.AA);
        }

        [Fact]
        public async Task NonexistentHostsReturnSoaAuthorityWithMinimumTtl()
        {
            string missingHost = _fixture.BuildHostName("missing");

            DnsMessage firstResponse = await _fixture.Client.QueryAsync(missingHost);
            DnsMessage secondResponse = await _fixture.Client.QueryAsync(missingHost);

            Assert.Equal(RCode.NXDOMAIN, (RCode)firstResponse.RCode);
            Assert.Equal((ushort)0, firstResponse.AnswerCount);
            Assert.Equal((ushort)1, firstResponse.NameServerCount);
            Assert.True(firstResponse.AA);
            Assert.False(firstResponse.RA);

            ResourceRecord soaRecord = Assert.Single(firstResponse.Authorities);
            Assert.Equal(ResourceType.SOA, soaRecord.Type);
            Assert.Equal((uint)300, soaRecord.TTL);
            StatementOfAuthorityRData soaData = Assert.IsType<StatementOfAuthorityRData>(soaRecord.RData);
            Assert.Equal((uint)300, soaData.MinimumTTL);

            ResourceRecord secondSoaRecord = Assert.Single(secondResponse.Authorities);
            Assert.Equal((uint)300, secondSoaRecord.TTL);
            StatementOfAuthorityRData secondSoaData = Assert.IsType<StatementOfAuthorityRData>(secondSoaRecord.RData);
            Assert.Equal((uint)300, secondSoaData.MinimumTTL);
        }
    }
}
