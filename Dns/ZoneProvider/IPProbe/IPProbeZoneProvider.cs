namespace Dns.ZoneProvider.IPProbe
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Configuration;


    /// <summary>
    /// IPProbeZoneProvider map via configuration a set of monitored IPs to host A records.
    /// Various monitoring strategies are implemented to detect IP health.
    /// Health IP addresses are added to the Zone.
    /// </summary>
    public class IPProbeZoneProvider : BaseZoneProvider
    {
        private IPProbeProviderOptions options;

        private State state { get; set; }
        private CancellationToken ct { get; set; }
        private Task runningTask { get; set; }

        /// <summary>Initialize ZoneProvider</summary>
        /// <param name="config">ZoneProvider Configuration Section</param>
        /// <param name="zoneName">Zone suffix</param>
        public override void Initialize(IConfiguration config, string zoneName)
        {
            this.options = config.Get<IPProbeProviderOptions>();
            if (options == null)
            {
                throw new Exception("Error loading IPProbeProviderOptions");
            }

            // load up initial state from options
            this.state = new State(options);
            this.Zone = zoneName;

            return;
        }

        public void ProbeLoop(CancellationToken ct)
        {
            Console.WriteLine("Probe loop started");

            ParallelOptions options = new ParallelOptions();
            options.CancellationToken = ct;
            options.MaxDegreeOfParallelism = 4;

            while (!ct.IsCancellationRequested)
            {
                var batchStartTime = DateTime.UtcNow;

                Parallel.ForEach(this.state.Targets, options, (probe) =>
                {
                    var startTime = DateTime.UtcNow;
                    var result = probe.ProbeFunction(probe.Address, probe.TimeoutMilliseconds);
                    var duration = DateTime.UtcNow - startTime;
                    probe.AddResult(new ProbeResult { StartTime = startTime, Duration = duration, Available = result });
                });

                Task.Run(() => this.GetZone(state)).ContinueWith(t => this.Notify(t.Result));

                var batchDuration = DateTime.UtcNow - batchStartTime;
                Console.WriteLine("Probe batch duration {0}", batchDuration);

                // wait remainder of Polling Interval
                var remainingWaitTimeout = (this.options.PollingIntervalSeconds * 1000) -(int)batchDuration.TotalMilliseconds;
                if(remainingWaitTimeout > 0)
                {
                    ct.WaitHandle.WaitOne(remainingWaitTimeout);
                }
            }
        }

        public override void Dispose()
        {
            // cleanup
        }

        public override void Start(CancellationToken ct)
        {
            ct.Register(this.Stop);
            this.runningTask = Task.Run(()=>ProbeLoop(ct));
        }

        private void Stop()
        {
            this.runningTask.Wait();
        }

        internal IEnumerable<ZoneRecord>GetZoneRecords(State state)
        {
            foreach(var host in state.Hosts)
            {
                var availableAddresses = host.AddressProbes
                    .Where(addr => addr.IsAvailable)
                    .Select(addr => addr.Address);

                if(host.AvailabilityMode == AvailabilityMode.First)
                {
                    availableAddresses = availableAddresses.Take(1);
                }

                // materialize query
                var addresses = availableAddresses.ToArray();

                if (addresses.Length == 0)
                {
                    // no hosts with empty recordsets
                    continue;
                }


                yield return new ZoneRecord
                {
                    Host = host.Name + this.Zone,
                    Addresses = addresses,
                    Count = addresses.Length,
                    Type = ResourceType.A,
                    Class = ResourceClass.IN,
                };
            }
        }

        internal Zone GetZone(State state)
        {
            var zoneRecords = GetZoneRecords(state);

            Zone zone = new Zone();
            zone.Suffix = this.Zone;
            zone.Serial = _serial;
            zone.Initialize(zoneRecords);

            // increment serial number
            _serial++;
            return zone;
        }
    }
}