// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="APZoneResolver.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.Collections.Frozen;
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

        /// <summary>
        /// Zone lookup map using struct keys for zero-allocation lookups.
        /// Phase 4: Uses DnsZoneLookupKey struct + FrozenDictionary for optimized read-only access.
        /// FrozenDictionary is optimized for scenarios where the dictionary is created once and read many times.
        /// It provides faster lookups than Dictionary by optimizing the internal structure at creation time.
        /// </summary>
        private FrozenDictionary<DnsZoneLookupKey, IAddressDispenser> _zoneMap;
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
                if (value == null) throw new ArgumentNullException("value");

                this._zone = value;
                this._zoneReload = DateTime.Now;
                // Build new zone map with struct keys, then freeze for optimal read performance
                var builder = new Dictionary<DnsZoneLookupKey, IAddressDispenser>();
                foreach (var record in this._zone)
                {
                    var key = new DnsZoneLookupKey(record.Host, record.Class, record.Type);
                    builder.TryAdd(key, new SmartAddressDispenser(record));
                }
                // ToFrozenDictionary optimizes internal structure for fast lookups
                // Atomic swap via volatile read/write semantics
                Volatile.Write(ref _zoneMap, builder.ToFrozenDictionary());
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

            var zoneMap = _zoneMap;
            if (zoneMap != null)
            {
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><td>Key</td><td>Value</td></tr>");
                foreach (var kvp in zoneMap)
                {
                    writer.WriteLine("<tr><td>");
                    writer.WriteLine(kvp.Key.ToString());
                    writer.WriteLine("</td><td>");
                    kvp.Value.DumpHtml(writer);
                    writer.WriteLine("</td></tr>");
                }
                writer.WriteLine("</table>");
            }
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

            // lookup locally using struct key (zero allocation)
            var key = new DnsZoneLookupKey(hostName, resClass, resType);
            var zoneMap = _zoneMap;
            if (zoneMap != null && zoneMap.TryGetValue(key, out var dispenser))
            {
                Interlocked.Increment(ref this._hits);
                entry = new IPHostEntry { AddressList = dispenser.GetAddresses().ToArray(), Aliases = new string[] { }, HostName = hostName };
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

        /// <summary>
        /// Creates a lookup key from zone record components.
        /// Legacy method preserved for compatibility - prefer using DnsZoneLookupKey struct directly.
        /// </summary>
        [Obsolete("Use new DnsZoneLookupKey(host, resClass, resType) for zero-allocation key creation")]
        private string GenerateKey(ZoneRecord record)
        {
            return GenerateKey(record.Host, record.Class, record.Type);
        }

        /// <summary>
        /// Creates a lookup key from zone record components.
        /// Legacy method preserved for compatibility - prefer using DnsZoneLookupKey struct directly.
        /// </summary>
        [Obsolete("Use new DnsZoneLookupKey(host, resClass, resType) for zero-allocation key creation")]
        private string GenerateKey(string host, ResourceClass resClass, ResourceType resType)
        {
            return string.Format("{0}|{1}|{2}", host, resClass, resType);
        }
    }
}
