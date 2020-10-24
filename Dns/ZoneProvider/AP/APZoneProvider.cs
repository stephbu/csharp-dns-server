// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="ZoneProvider.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns.ZoneProvider.AP
{
    using System.IO;
    using System.Net;
    using System.Linq;
    using Dns.Utility;
    using Dns.ZoneProvider;

    using Microsoft.Extensions.Configuration;

    /// <summary>Source of Zone records</summary>
    public class APZoneProvider : FileWatcherZoneProvider
    {

        public override Zone GenerateZone()
        {
            if (!File.Exists(this.Filename))
            {
                return null;
            }

            CsvParser parser = CsvParser.Create(this.Filename);
            var machines = parser.Rows.Select(row => new {MachineFunction = row["MachineFunction"], StaticIP = row["StaticIP"], MachineName = row["MachineName"]}).ToArray();

            var zoneRecords = machines
                            .GroupBy(machine => machine.MachineFunction + this.Zone, machine => IPAddress.Parse(machine.StaticIP))
                            .Select(group => new ZoneRecord {Host = group.Key, Count = group.Count(), Addresses = group.Select(address => address).ToArray()})
                            .ToArray();

            Zone zone = new Zone();
            zone.Suffix = this.Zone;
            zone.Serial = this._serial;
            zone.Initialize(zoneRecords);

            // increment serial number
            this._serial++;
            return zone;
        }
    }
}