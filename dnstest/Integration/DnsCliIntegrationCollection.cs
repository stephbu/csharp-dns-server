// // //-------------------------------------------------------------------------------------------------
// // // <copyright file="DnsCliIntegrationCollection.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace DnsTest.Integration
{
    using Xunit;

    [CollectionDefinition(Name)]
    public sealed class DnsCliIntegrationCollection : ICollectionFixture<DnsCliHostFixture>
    {
        public const string Name = "DnsCliIntegration";
    }
}
