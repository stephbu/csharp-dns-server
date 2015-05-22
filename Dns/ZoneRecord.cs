// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="ZoneRecord.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System.Net;

    public class ZoneRecord
    {
        public string Host;
        public ResourceClass Class = ResourceClass.IN;
        public ResourceType Type = ResourceType.A;
        public IPAddress[] Addresses;
        public int Count;
    }
}