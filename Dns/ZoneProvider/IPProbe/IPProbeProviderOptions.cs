namespace Dns.ZoneProvider.IPProbe
{
    public class IPProbeProviderOptions
    {
        public ushort PollingIntervalSeconds { get; set; }
        public HostOptions[] Hosts { get; set; }
    }

    public class HostOptions
    {
        /// <summary>Host name</summary>
        public string Name { get; set; }

        /// <summary>Probe strategy</summary>
        public string Probe { get; set; }

        /// <summary>Host probe timeout</summary>
        public ushort Timeout { get; set; }

        public AvailabilityMode AvailabilityMode { get; set; }

        public string[] Ip { get; set; }
    }

    public enum AvailabilityMode
    {
        All,
        First,
    }
}