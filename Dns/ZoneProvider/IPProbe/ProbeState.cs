namespace Dns.ZoneProvider.IPProbe
{
    using System.Collections.Generic;

    public partial class IPProbeZoneProvider
    {
        internal class ProbeState
        {
            internal HashSet<AddressProbe> AddressProbes = new HashSet<AddressProbe>(new AddressProbe.Comparer());
            internal HashSet<HostName> HostNames = new HashSet<HostName>();
        }
    }
}