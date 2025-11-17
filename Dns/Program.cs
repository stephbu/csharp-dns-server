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
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Program
    {

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
            using var serviceProvider = BuildServiceProvider(configuration, appConfig);

            var zoneProviderConfig = configuration.GetSection("zoneprovider");
            _zoneProvider = serviceProvider.GetRequiredService<ZoneProvider.BaseZoneProvider>();
            _zoneProvider.Initialize(zoneProviderConfig, appConfig.Server.Zone.Name);

            _zoneResolver = serviceProvider.GetRequiredService<SmartZoneResolver>();
            _zoneResolver.SubscribeTo(_zoneProvider);

            _dnsServer = serviceProvider.GetRequiredService<DnsServer>();

            _httpServer = serviceProvider.GetRequiredService<HttpServer>();

            _dnsServer.Initialize(_zoneResolver);

            _zoneProvider.Start(ct);
            _dnsServer.Start(ct);

            if (appConfig.Server.WebServer.Enabled)
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
                context.Response.Headers.Add("Content-Type", "text/html");
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

        private static ServiceProvider BuildServiceProvider(IConfiguration configuration, Config.AppConfig appConfig)
        {
            var services = new ServiceCollection();
            services.AddSingleton(configuration);
            services.AddSingleton(appConfig);
            services.AddSingleton<SmartZoneResolver>();
            services.AddSingleton<HttpServer>();
            services.AddSingleton(provider => new DnsServer(appConfig.Server.DnsListener.Port));
            services.AddSingleton<ZoneProvider.BaseZoneProvider>(provider =>
            {
                var zoneProviderType = ByName(appConfig.Server.Zone.Provider);
                if (zoneProviderType == null || !typeof(ZoneProvider.BaseZoneProvider).IsAssignableFrom(zoneProviderType))
                {
                    throw new InvalidOperationException(string.Format("Unable to locate zone provider type '{0}'.", appConfig.Server.Zone.Provider));
                }

                return (ZoneProvider.BaseZoneProvider)ActivatorUtilities.CreateInstance(provider, zoneProviderType);
            });

            return services.BuildServiceProvider();
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
