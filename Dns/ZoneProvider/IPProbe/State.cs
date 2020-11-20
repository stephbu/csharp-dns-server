namespace Dns.ZoneProvider.IPProbe
{
    using System.Net;
    using System.Collections.Generic;

    internal class State
    {
        internal HashSet<Target> Targets = new HashSet<Target>(new Target.Comparer());
        internal HashSet<Host> Hosts = new HashSet<Host>();

        internal State(IPProbeProviderOptions options)
        {
            foreach (var host in options.Hosts)
            {
                var hostResult = new Host();
                hostResult.Name = host.Name;
                hostResult.AvailabilityMode = host.AvailabilityMode;

                foreach (var address in host.Ip)
                {
                    var addressProbe = new Target
                    {
                        Address = IPAddress.Parse(address),
                        ProbeFunction = Strategy.Get(host.Probe),
                        TimeoutMilliseconds = host.Timeout,
                    };

                    Target preExisting;

                    if (this.Targets.TryGetValue(addressProbe, out preExisting))
                    {
                        hostResult.AddressProbes.Add(preExisting);
                    }
                    else
                    {
                        this.Targets.Add(addressProbe);
                        hostResult.AddressProbes.Add(addressProbe);
                    }
                }


                this.Hosts.Add(hostResult);
            }
        }
    }
}