namespace Dns.ZoneProvider.IPProbe
{
    using System;
    using System.Net;
    using System.Collections.Generic;
    using System.Linq;

    public partial class IPProbeZoneProvider
    {
        internal class AddressProbe
        {
            internal IPAddress Address;
            internal Func<IPAddress, ushort, bool> ProbeFunction;
            internal ushort TimeoutMilliseconds;
            internal List<ProbeResult> Results = new List<ProbeResult>();

            public override int GetHashCode()
            {
                return string.Format("{0}|{1}|{2}", this.Address, this.ProbeFunction, this.TimeoutMilliseconds).GetHashCode();
            }

            internal bool IsAvailable
            {
                get
                {
                    // Endpoint is available if 3 of the last 5 results were positive
                    return this.Results.TakeLast(5).Where(r => r.Available).Count() > 3;
                }
            }

            internal class Comparer : IEqualityComparer<AddressProbe>
            {
                public bool Equals(AddressProbe x, AddressProbe y)
                {
                    //Check whether the objects are the same object. 
                    if (x.Equals(y)) return true;

                    return x.GetHashCode() == y.GetHashCode();

                }

                public int GetHashCode(AddressProbe obj)
                {
                    return obj.GetHashCode();
                }
            }

            internal void AddResult(ProbeResult result)
            {
                this.Results.Add(result);
                if (this.Results.Count > 10)
                {
                    this.Results.RemoveAt(0);
                }
            }

        }


    }
}