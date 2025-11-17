// // //-------------------------------------------------------------------------------------------------
// // // <copyright file="DnsCliHostFixture.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace DnsTest.Integration
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class DnsCliHostFixture : IAsyncLifetime, IDisposable
    {
        private const string ZoneSuffix = ".integration.test";

        private readonly ConcurrentQueue<string> _logLines = new ConcurrentQueue<string>();
        private readonly TaskCompletionSource<bool> _readyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        private Process _process;
        private Task _stdoutTask;
        private Task _stderrTask;
        private string _configPath;
        private DirectoryInfo _workingDirectory;

        public IPEndPoint DnsEndpoint { get; private set; }

        internal DnsQueryClient Client { get; private set; }

        public string[] Logs => _logLines.ToArray();

        public string BuildHostName(string hostPrefix)
        {
            if (string.IsNullOrWhiteSpace(hostPrefix))
            {
                throw new ArgumentException("Host prefix is required.", nameof(hostPrefix));
            }

            if (hostPrefix.EndsWith(ZoneSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return hostPrefix;
            }

            return $"{hostPrefix}{ZoneSuffix}";
        }

        public async Task InitializeAsync()
        {
            ValidateArtifacts();

            int dnsPort = GetAvailableUdpPort();
            int httpPort = GetAvailableTcpPort();

            DnsEndpoint = new IPEndPoint(IPAddress.Loopback, dnsPort);

            PrepareWorkingDirectory();
            CopyZoneFile();
            WriteConfigFile(dnsPort, httpPort);

            StartProcess();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await WaitForReadyAsync(timeoutCts.Token).ConfigureAwait(false);

            Client = new DnsQueryClient(DnsEndpoint);
        }

        public async Task DisposeAsync()
        {
            await StopProcessAsync().ConfigureAwait(false);
            CleanupWorkingDirectory();
        }

        public void Dispose()
        {
            StopProcessAsync().GetAwaiter().GetResult();
            CleanupWorkingDirectory();
        }

        private void ValidateArtifacts()
        {
            if (!File.Exists(TestProjectPaths.DnsCliDllPath))
            {
                throw new FileNotFoundException("dns-cli binary not found. Run dotnet build before executing the integration tests.", TestProjectPaths.DnsCliDllPath);
            }

            if (!File.Exists(GetTemplatePath()))
            {
                throw new FileNotFoundException("Integration configuration template is missing.", GetTemplatePath());
            }

            if (!File.Exists(GetZoneSourcePath()))
            {
                throw new FileNotFoundException("Integration zone data file is missing.", GetZoneSourcePath());
            }
        }

        private void PrepareWorkingDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), $"dns-cli-tests-{Guid.NewGuid():N}");
            _workingDirectory = Directory.CreateDirectory(tempDirectory);
        }

        private void CopyZoneFile()
        {
            string destination = Path.Combine(_workingDirectory.FullName, "machineinfo.csv");
            File.Copy(GetZoneSourcePath(), destination, overwrite: true);
        }

        private void WriteConfigFile(int dnsPort, int httpPort)
        {
            string template = File.ReadAllText(GetTemplatePath());
            string zoneFilePath = Path.Combine(_workingDirectory.FullName, "machineinfo.csv");

            template = template.Replace("{{DNS_PORT}}", dnsPort.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
            template = template.Replace("{{HTTP_PORT}}", httpPort.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
            template = template.Replace("{{ZONE_SUFFIX}}", ZoneSuffix, StringComparison.Ordinal);
            template = template.Replace("{{ZONE_FILE}}", JsonEncodedText.Encode(zoneFilePath).ToString(), StringComparison.Ordinal);

            _configPath = Path.Combine(_workingDirectory.FullName, "appsettings.json");
            File.WriteAllText(_configPath, template, Encoding.UTF8);
        }

        private void StartProcess()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{TestProjectPaths.DnsCliDllPath}\" \"{_configPath}\"",
                WorkingDirectory = Path.GetDirectoryName(TestProjectPaths.DnsCliDllPath),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start dns-cli.");

            if (_process.HasExited)
            {
                throw new InvalidOperationException("dns-cli exited immediately after start.");
            }

            _process.EnableRaisingEvents = true;
            _process.Exited += (_, __) =>
            {
                if (!_readyTcs.Task.IsCompleted)
                {
                    _readyTcs.TrySetException(new InvalidOperationException("dns-cli exited before it signaled readiness."));
                }
            };

            _stdoutTask = Task.Run(() => PumpStreamAsync(_process.StandardOutput, "[out]"));
            _stderrTask = Task.Run(() => PumpStreamAsync(_process.StandardError, "[err]"));
        }

        private async Task PumpStreamAsync(StreamReader reader, string prefix)
        {
            while (!reader.EndOfStream)
            {
                string line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line == null)
                {
                    break;
                }

                string formatted = $"{prefix} {line}";
                _logLines.Enqueue(formatted);

                if (line.IndexOf("Zone reloaded", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _readyTcs.TrySetResult(true);
                }
            }
        }

        private async Task WaitForReadyAsync(CancellationToken cancellationToken)
        {
            Task completed = await Task.WhenAny(_readyTcs.Task, Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken)).ConfigureAwait(false);
            if (completed != _readyTcs.Task)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new TimeoutException("dns-cli did not emit a readiness signal.");
            }

            await _readyTcs.Task.ConfigureAwait(false);
        }

        private async Task StopProcessAsync()
        {
            if (_process == null)
            {
                return;
            }

            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                }
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                await _process.WaitForExitAsync().ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
            }

            if (_stdoutTask != null)
            {
                try
                {
                    await _stdoutTask.ConfigureAwait(false);
                }
                catch
                {
                }
            }

            if (_stderrTask != null)
            {
                try
                {
                    await _stderrTask.ConfigureAwait(false);
                }
                catch
                {
                }
            }

            _process.Dispose();
            _process = null;
            _stdoutTask = null;
            _stderrTask = null;
        }

        private void CleanupWorkingDirectory()
        {
            try
            {
                if (_workingDirectory != null && _workingDirectory.Exists)
                {
                    _workingDirectory.Delete(recursive: true);
                }
            }
            catch
            {
            }
        }

        private static int GetAvailableUdpPort()
        {
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            return ((IPEndPoint)socket.LocalEndPoint).Port;
        }

        private static int GetAvailableTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static string GetTemplatePath()
        {
            return Path.Combine(TestProjectPaths.TestDataDirectory, "appsettings.template.json");
        }

        private static string GetZoneSourcePath()
        {
            return Path.Combine(TestProjectPaths.TestDataDirectory, "Zones", "integration_machineinfo.csv");
        }
    }
}
