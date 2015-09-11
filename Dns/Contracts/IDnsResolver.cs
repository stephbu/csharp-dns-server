// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="IDnsResolver.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns.Contracts
{
    using System.Net;

    /// <summary>Provides domain name resolver capabilities</summary>
    internal interface IDnsResolver : IHtmlDump
    {
        string GetZoneName();

        uint GetZoneSerial();

        bool TryGetHostEntry(string hostname, ResourceClass resClass, ResourceType resType, out IPHostEntry entry);
    }
}