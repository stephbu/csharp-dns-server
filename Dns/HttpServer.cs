// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="HttpServer.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Dns.Contracts;

    internal delegate void OnHttpRequestHandler(HttpListenerContext context);
    internal delegate void OnHandledException(Exception ex);

    /// <summary>HTTP data receiver</summary>
    internal class HttpServer : IHtmlDump
    {
        private HttpListener _listener;
        private bool _running;

        private int _requestCounter = 0;
        private int _request200;
        private int _request300;
        private int _request400;
        private int _request500;
        private int _request600;

        private readonly string _machineName = Environment.MachineName;

        public event OnHttpRequestHandler OnProcessRequest;
        public event OnHttpRequestHandler OnHealthProbe;
        public event OnHandledException OnHandledException;

        /// <summary>Configure listener</summary>
        public void Initialize(params string[] prefixes)
        {
            _listener = new HttpListener();
            foreach (string prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
        }

        /// <summary>Start listening</summary>
        public async void Start(CancellationToken ct)
        {
            ct.Register(this.Stop);

            while (true)
            {
                try
                {
                    HttpListenerContext context = await this._listener.GetContextAsync();
                    var processRequest = Task.Run(() => this.ProcessRequest(context));
                }
                catch (HttpListenerException ex)
                {
                    if (this.OnHandledException != null)
                    {
                        this.OnHandledException(ex);
                    }
                    break;
                }
                catch (InvalidOperationException ex)
                {
                    if (this.OnHandledException != null)
                    {
                        this.OnHandledException(ex);
                    }
                    break;
                }
            }
        }

        /// <summary>Stop listening</summary>
        public void Stop()
        {
            if (_running == true)
            {
                _running = false;
            }

            _listener.Stop();
        }

        /// <summary>Process incoming request</summary>
        private void ProcessRequest(HttpListenerContext context)
        {

            // log
            // performance counters
            try
            {
                // special case health probes
                if (context.Request.RawUrl.Equals("/health/keepalive", StringComparison.InvariantCultureIgnoreCase))
                {
                    HealthProbe(context);
                }
                else
                {
                    if (this.OnProcessRequest != null)
                    {
                        this.OnProcessRequest(context);
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: log exception
                if (this.OnHandledException != null)
                {
                    this.OnHandledException(ex);
                }
                context.Response.StatusCode = 500;
            }

            context.Response.OutputStream.Dispose();

            int statusCode = context.Response.StatusCode;

            if ((200 <= statusCode) && (statusCode < 300)) _request200++;
            if ((300 <= statusCode) && (statusCode < 400)) _request300++;
            if ((400 <= statusCode) && (statusCode < 500)) _request400++;
            if ((500 <= statusCode) && (statusCode < 600)) _request500++;
            if ((600 <= statusCode) && (statusCode < 700)) _request600++;

            _requestCounter++;
        }

        /// <summary>Process health probe request</summary>
        private void HealthProbe(HttpListenerContext context)
        {
            
            if (this.OnHealthProbe != null)
            {
                this.OnHealthProbe(context);
            }
            else
            {
                context.Response.StatusCode = 200;
                context.Response.ContentEncoding = System.Text.Encoding.UTF8;
                context.Response.ContentType = "text/html";
                using (TextWriter writer = context.Response.OutputStream.CreateWriter())
                {

                }
            }
        }

        public void DumpHtml(TextWriter writer)
        {
            writer.WriteLine("Health Probe<br/>");
            writer.WriteLine("Machine: {0}<br/>", this._machineName);
            writer.WriteLine("Count: {0}<br/>", this._requestCounter);
            writer.WriteLine("200: {0}<br/>", this._request200);
            writer.WriteLine("300: {0}<br/>", this._request300);
            writer.WriteLine("400: {0}<br/>", this._request400);
            writer.WriteLine("500: {0}<br/>", this._request500);
            writer.WriteLine("600: {0}<br/>", this._request600);
        }
    }
}