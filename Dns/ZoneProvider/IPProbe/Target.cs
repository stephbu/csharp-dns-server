namespace Dns.ZoneProvider.IPProbe
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    internal class Target
    {
        internal IPAddress Address;
        internal Strategy.Probe ProbeFunction;
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
                // Endpoint is available up-to last 3 results were successful
                return this.Results.TakeLast(3).All(r => r.Available);
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


        internal class Comparer : IEqualityComparer<Target>
        {
            public bool Equals(Target x, Target y)
            {
                //Check whether the objects are the same object. 
                if (x.Equals(y)) return true;

                return x.GetHashCode() == y.GetHashCode();

            }

            public int GetHashCode(Target obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}