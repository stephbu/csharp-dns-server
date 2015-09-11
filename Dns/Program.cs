// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="Program.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using Dns.ZoneProvider.AP;

    internal class Program
    {
        private static APZoneProvider _zoneProvider; // reloads Zones from machineinfo.csv changes
        private static SmartZoneResolver _zoneResolver; // resolver and delegated lookup for unsupported zones;
        private static DnsServer _dnsServer; // resolver and delegated lookup for unsupported zones;
        private static HttpServer _httpServer;
        private static ManualResetEvent _exit = new ManualResetEvent(false);
        private static ManualResetEvent _exitTimeout = new ManualResetEvent(false);

        private static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            _zoneProvider = new APZoneProvider("d:\\data\\machineinfo.csv", ".foo.bar");
            _zoneResolver = new SmartZoneResolver();
            _dnsServer = new DnsServer();
            _httpServer = new HttpServer();

            _zoneResolver.SubscribeTo(_zoneProvider);

            _dnsServer.Initialize(_zoneResolver);
            _httpServer.Initialize("http://+:8080/");
            _httpServer.OnProcessRequest += _httpServer_OnProcessRequest;
            _httpServer.OnHealthProbe += _httpServer_OnHealthProbe;

            _zoneProvider.Start();
            _dnsServer.Start();
            _httpServer.Start();

            _exit.WaitOne();

            _httpServer.Stop();
            _dnsServer.Stop();
            _zoneProvider.Stop();

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
            _exit.Set();
            _exitTimeout.WaitOne(5000);
        }
    }
}