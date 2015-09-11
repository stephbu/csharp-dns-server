// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="IHtmlDump.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns.Contracts
{
    using System.IO;

    public interface IHtmlDump
    {
        void DumpHtml(TextWriter writer);
    }
}