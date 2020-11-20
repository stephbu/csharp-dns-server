using System;
using System.Threading;

namespace DnsCli
{
    /// <summary>Stub program that enables DNS Server to run from the command line</summary>

    class Program
    {
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static ManualResetEvent _exitTimeout = new ManualResetEvent(false);

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            Console.WriteLine("DNS Server - Console Mode");

            if(args.Length == 0)
            {
                args = new string[] { "./appsettings.json" };
            }

            Dns.Program.Run(args[0], cts.Token);

            _exitTimeout.Set();

        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("\r\nShutting Down");
            cts.Cancel();
            _exitTimeout.WaitOne(5000);
        }
    }
}
