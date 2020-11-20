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

        /// <summary>
        /// DNS Server entrypoint
        /// </summary>
        /// <param name="configFile">Fully qualified configuration filename</param>
        /// <param name="cts">Cancellation Token Source</param>
        public static void Run(string configFile, CancellationToken ct)
        {

            if (!File.Exists(configFile))
            {
                throw new FileNotFoundException(null, configFile);
            }

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(configFile, true, true)
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

            _zoneProvider.Start(ct);
            _dnsServer.Start(ct);

            if(appConfig.Server.WebServer.Enabled)
            {
                _httpServer.Initialize(string.Format("http://+:{0}/", appConfig.Server.WebServer.Port));
                _httpServer.OnProcessRequest += _httpServer_OnProcessRequest;
                _httpServer.OnHealthProbe += _httpServer_OnHealthProbe;
                _httpServer.Start(ct);
            }

            ct.WaitHandle.WaitOne();

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