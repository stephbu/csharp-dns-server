// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="IAddressDispenser.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns.Contracts
{
    using System.Collections.Generic;
    using System.Net;

    public interface IAddressDispenser : IHtmlDump
    {
        string HostName { get; }

        IEnumerable<IPAddress> GetAddresses();
    }
}