using System;
using Dns.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Xunit;

namespace DnsTest
{
    public class ConfigTests
    {
        public ConfigTests()
        {
        }

        [Fact]
        public void LoadConfig()
        {
            var jsonSource = new JsonConfigurationSource
            {
                Path = "./Data/appsettings.json"
            };
            var jsonConfig = new JsonConfigurationProvider(jsonSource);
        }
    }
}
