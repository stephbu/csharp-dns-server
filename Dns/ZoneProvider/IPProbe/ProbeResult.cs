namespace Dns.ZoneProvider.IPProbe
{
    using System;

    public partial class IPProbeZoneProvider
    {
        internal class ProbeResult
        {
            internal DateTime StartTime;
            internal TimeSpan Duration;
            internal bool Available;
        }


    }
}