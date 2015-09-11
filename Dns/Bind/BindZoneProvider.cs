// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="BindZoneProvider.cs" >
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns.Bind
{
    using System;

    public class BindZoneProvider : FileWatcherZoneProvider
    {
        public BindZoneProvider(string filename) : base(filename)
        {
        }

        public override Zone GenerateZone()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}