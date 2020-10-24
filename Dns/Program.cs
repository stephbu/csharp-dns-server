// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Program.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using Dns.ZoneProvider.AP;

    using Ninject;
    using Microsoft.Extensions.Configuration;

    public class Program
    {

        private static IKernel container = new StandardKernel();

        private static ZoneProvider.BaseZoneProvider _zoneProvider; // reloads Zones from machineinfo.csv changes
        private static SmartZoneResolver _zoneResolver; // resolver and delegated lookup for unsupported zones;
        private static DnsServer _dnsServer; // resolver and delegated lookup for unsupported zones;
        private static HttpServer _httpServer;
        private static ManualResetEvent _exitTimeout = new ManualResetEvent(false);
        private static CancellationTokenSource cts = new CancellationTokenSource();

        // TODO: Move startup args into config file
        public static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var appConfig = configuration.Get<Config.AppConfig>();

            container.Bind<ZoneProvider.BaseZoneProvider>().To(ByName(appConfig.Server.Zone.Provider));
            var zoneProviderConfig = configuration.GetSection("zoneprovider");
            _zoneProvider = container.Get<ZoneProvider.BaseZoneProvider>();
            _zoneProvider.Initialize(zoneProviderConfig, appConfig.Server.Zone.Name);

            _zoneResolver = new SmartZoneResolver();
            _zoneResolver.SubscribeTo(_zoneProvider);

            _dnsServer = new DnsServer(appConfig.Server.DnsListener.Port);

            _httpServer = new HttpServer();

            _dnsServer.Initialize(_zoneResolver);

            _zoneProvider.Start(cts.Token);
            _dnsServer.Start(cts.Token);

            if(appConfig.Server.WebServer.Enabled)
            {
                _httpServer.Initialize(string.Format("http://+:{0}/", appConfig.Server.WebServer.Port));
                _httpServer.OnProcessRequest += _httpServer_OnProcessRequest;
                _httpServer.OnHealthProbe += _httpServer_OnHealthProbe;
                _httpServer.Start(cts.Token);
            }

            cts.Token.WaitHandle.WaitOne();

            _exitTimeout.Set();
        }

        static void _httpServer_OnHealthProbe(HttpListenerContext context)
        {
        }

        private static void _httpServer_OnProcessRequest(HttpListenerContext context)
        {
            string rawUrl = context.Request.RawUrl;
            if (rawUrl == "/dump/dnsresolver")
            {
                context.Response.Headers.Add("Content-Type","text/html");
                using (TextWriter writer = context.Response.OutputStream.CreateWriter())
                {
                    _zoneResolver.DumpHtml(writer);
                }
            }
            else if (rawUrl == "/dump/httpserver")
            {
                context.Response.Headers.Add("Content-Type", "text/html");
                using (TextWriter writer = context.Response.OutputStream.CreateWriter())
                {
                    _httpServer.DumpHtml(writer);
                }
            }
            else if (rawUrl == "/dump/dnsserver")
            {
                context.Response.Headers.Add("Content-Type", "text/html");
                using (TextWriter writer = context.Response.OutputStream.CreateWriter())
                {
                    _dnsServer.DumpHtml(writer);
                }
            }
            else if (rawUrl == "/dump/zoneprovider")
            {
                context.Response.Headers.Add("Content-Type", "text/html");
                using (TextWriter writer = context.Response.OutputStream.CreateWriter())
                {
                    _httpServer.DumpHtml(writer);
                }
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("/r/nShutting Down");
            cts.Cancel();
            _exitTimeout.WaitOne(5000);
        }

        private static Type ByName(string name)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
            {
                var tt = assembly.GetType(name);
                if (tt != null)
                {
                    return tt;
                }
            }

            return null;
        }
    }
}