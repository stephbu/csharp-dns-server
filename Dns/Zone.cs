// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Zone.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System.Collections.Generic;

    public class Zone : List<ZoneRecord>
    {
        public string Suffix { get; set; }

        public uint Serial { get; set; }

        public void Initialize(IEnumerable<ZoneRecord> nameRecords)
        {
            this.Clear();
            this.AddRange(nameRecords);
        }
    }
}