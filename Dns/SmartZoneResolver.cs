// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="APZoneResolver.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using Dns.Contracts;

    public class SmartZoneResolver : IObserver<Zone>, IDnsResolver
    {
        private long _hits;
        private long _misses;
        private long _queries;
        private IDisposable _subscription;
        private Zone _zone;
        private Dictionary<string, IAddressDispenser> _zoneMap;
        private DateTime _zoneReload = DateTime.MinValue;

        public string GetZoneName()
        {
            return this.Zone.Suffix;
        }

        public uint GetZoneSerial()
        {
            return this._zone.Serial;
        }

        public Zone Zone
        {
            get { return this._zone; }
            set
            {
                if(value == null) throw new ArgumentNullException("value");

                this._zone = value;
                this._zoneReload = DateTime.Now;
                this._zoneMap = this._zone.ToDictionary(GenerateKey, zoneRecord => new SmartAddressDispenser(zoneRecord) as IAddressDispenser, StringComparer.CurrentCultureIgnoreCase);
                Console.WriteLine("Zone reloaded");
            }
        }

        public DateTime LastZoneReload
        {
            get { return _zoneReload; }
        }

        void IObserver<Zone>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        void IObserver<Zone>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        void IObserver<Zone>.OnNext(Zone value)
        {
            this.Zone = value;
        }

        public void DumpHtml(TextWriter writer)
        {
            writer.WriteLine("Type:{0}<br/>", this.GetType().Name);
            writer.WriteLine("Queries:{0}<br/>", this._queries);
            writer.WriteLine("Hits:{0}<br/>", this._hits);
            writer.WriteLine("Misses:{0}<br/>", this._misses);

            writer.WriteLine("<table>");
            writer.WriteLine("<tr><td>Key</td><td>Value</td></tr>");
            foreach (string key in _zoneMap.Keys)
            {
                writer.WriteLine("<tr><td>");
                writer.WriteLine(key);
                writer.WriteLine("</td><td>");
                _zoneMap[key].DumpHtml(writer);
                writer.WriteLine("</td></tr>");
            }
            writer.WriteLine("</table>");
        }

        public bool TryGetHostEntry(string hostName, ResourceClass resClass, ResourceType resType, out IPHostEntry entry)
        {
            if (hostName == null) throw new ArgumentNullException("hostName");
            if (hostName.Length > 126) throw new ArgumentOutOfRangeException("hostName");

            entry = null;

            Interlocked.Increment(ref this._queries);

            // fail fasts
            if (!this.IsZoneLoaded()) return false;
            if (!hostName.EndsWith(this._zone.Suffix)) return false;

            // lookup locally
            string key = GenerateKey(hostName, resClass, resType);
            IAddressDispenser dispenser;
            if (_zoneMap.TryGetValue(key, out dispenser))
            {
                Interlocked.Increment(ref this._hits);
                entry = new IPHostEntry {AddressList = dispenser.GetAddresses().ToArray(), Aliases = new string[] {}, HostName = hostName};
                return true;
            }

            Interlocked.Increment(ref this._misses);
            return false;
        }

        public bool IsZoneLoaded()
        {
            return _zone != null;
        }

        /// <summary>Subscribe to specified zone provider</summary>
        /// <param name="zoneProvider"></param>
        public void SubscribeTo(IObservable<Zone> zoneProvider)
        {
            // release previous subscription
            if (this._subscription != null)
            {
                this._subscription.Dispose();
                this._subscription = null;
            }

            this._subscription = zoneProvider.Subscribe(this);
        }

        private string GenerateKey(ZoneRecord record)
        {
            return GenerateKey(record.Host, record.Class, record.Type);
        }

        private string GenerateKey(string host, ResourceClass resClass, ResourceType resType)
        {
            return string.Format("{0}|{1}|{2}", host, resClass, resType);
        }
    }
}