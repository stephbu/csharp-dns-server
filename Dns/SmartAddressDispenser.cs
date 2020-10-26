// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="SmartAddressDispenser.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using Dns.Contracts;

    /// <summary>Address Dispenser enables round-robin ordering for the specified zone record</summary>
    public class SmartAddressDispenser : IAddressDispenser
    {
        private ulong _sequence = 0;

        private readonly ZoneRecord _zoneRecord;
        private readonly ushort _maxAddressesReturned;

        public SmartAddressDispenser(ZoneRecord record, ushort maxAddressesReturned = 4)
        {
            _zoneRecord = record;
            _maxAddressesReturned = maxAddressesReturned;
        }

        string IAddressDispenser.HostName
        {
            get { return this._zoneRecord.Host; }
        }

        /// <summary>Returns round-robin rotated set of IP addresses</summary>
        /// <returns>Set of IP Addresses</returns>
        public IEnumerable<IPAddress> GetAddresses()
        {
            IPAddress[] addresses = _zoneRecord.Addresses;

            if(addresses.Length == 0)
            {
                yield break;
            }

            // starting position in rollover list
            int start = (int) (_sequence % (ulong) addresses.Length);
            int offset = start;
            
            uint count = 0;
            while (true)
            {

                yield return addresses[offset];
                offset++;

                // rollover to start of list
                if (offset == addresses.Length)
                {
                    offset = 0;
                }

                // if back at starting position then exit
                if (offset == start)
                {
                    break;
                }

                // manage max number of dns entries returned
                count++;
                if (count == _maxAddressesReturned)
                {
                    break;
                }
            }
            // advance sequence
            _sequence++;
        }

        public void DumpHtml(TextWriter writer)
        {
            writer.WriteLine("Sequence:{0}", this._sequence);
            foreach (var address in this._zoneRecord.Addresses)
            {
                writer.WriteLine(address);
            }
        }
    }
}