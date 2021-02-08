namespace Dns.ZoneProvider.IPProbe
{
    using System.Collections.Generic;

    internal class Host
    {
        internal string Name { get; set; }
        internal AvailabilityMode AvailabilityMode { get; set; }
        internal List<Target> AddressProbes = new List<Target>();
    }
}