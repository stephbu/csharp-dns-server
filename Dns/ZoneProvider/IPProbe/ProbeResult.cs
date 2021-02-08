namespace Dns.ZoneProvider.IPProbe
{
    using System;

    internal class ProbeResult
    {
        internal DateTime StartTime;
        internal TimeSpan Duration;
        internal bool Available;
    }
}