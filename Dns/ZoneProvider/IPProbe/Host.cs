namespace Dns.ZoneProvider.IPProbe
{
    using System.Collections.Generic;

    public partial class IPProbeZoneProvider
    {
        internal class Host
        {
            internal string Name { get; set; }
            internal AvailabilityMode AvailabilityMode { get; set; }
            internal List<Target> AddressProbes = new List<Target>();
        }
    }
}