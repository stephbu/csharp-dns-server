namespace Dns.ZoneProvider.IPProbe
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using System.Net.NetworkInformation;



    public class Strategy
    {

        // Probe Strategy Dictionary, maps configuration to implemented functions
        private static Dictionary<string, Func<IPAddress,ushort, bool>> probeFunctions = new Dictionary<string, Func<IPAddress,ushort, bool>>();

        static Strategy()
        {
            // New probe strategies and enhancements can be added here
            probeFunctions["ping"] = Strategy.Ping;
            probeFunctions["noop"] = Strategy.NoOp;
        }

        public static Func<IPAddress, ushort, bool> Get(string name)
        {
            return probeFunctions.GetValueOrDefault(name, Strategy.NoOp);
        }

        private static bool Ping(IPAddress address, ushort timeout)
        {
            Console.WriteLine("Ping: pinging {0}", address);
            Ping sender = new Ping();
            PingOptions options = new PingOptions(64, true);
            var pingReply = sender.Send(address, timeout);
            return (pingReply.Status == IPStatus.Success);
        }

        private static bool NoOp(IPAddress address, ushort _)
        {
            return true;
        }
    }
}
