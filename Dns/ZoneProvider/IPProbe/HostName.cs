namespace Dns.ZoneProvider.IPProbe
{
    using System.Collections.Generic;

    public partial class IPProbeZoneProvider
    {
        internal class HostName
        {
            internal string Name { get; set; }
            internal AvailabilityMode AvailabilityMode { get; set; }
            internal List<AddressProbe> AddressProbes = new List<AddressProbe>();
        }
    }
}