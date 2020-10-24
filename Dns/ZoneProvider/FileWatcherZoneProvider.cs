// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="ZoneProvider.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns.ZoneProvider
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;

    public abstract class FileWatcherZoneProvider : BaseZoneProvider
    {
        public delegate void FileWatcherDelegate(object sender, FileSystemEventArgs e);

        public event FileWatcherDelegate OnCreated = delegate { };
        public event FileWatcherDelegate OnDeleted = delegate { };
        public event FileWatcherDelegate OnRenamed = delegate { };
        public event FileWatcherDelegate OnChanged = delegate { };
        public event FileWatcherDelegate OnSettlement = delegate {};

        private FileSystemWatcher _fileWatcher;
        private TimeSpan _settlement = TimeSpan.FromSeconds(10);
        private Timer _timer;

        public abstract Zone GenerateZone();

        /// <summary>Timespan between last file change and zone generation</summary>
        public TimeSpan FileSettlementPeriod
        {
            get { return this._settlement; }
            set { this._settlement = value; }
        }

        public string Filename { get; private set; }

        public override void Initialize(IConfiguration config, string zoneName)
        {
            var filewatcherConfig = config.Get<FileWatcherZoneProviderOptions>();

            var filename = filewatcherConfig.FileName;

            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("Null or empty", "filename");
            }

            filename = Environment.ExpandEnvironmentVariables(filename);
            filename = Path.GetFullPath(filename);

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("filename not found", filename);
            }


            string directory = Path.GetDirectoryName(filename); 
            string fileNameFilter = Path.GetFileName(filename);

            this.Filename = filename;
            this._fileWatcher = new FileSystemWatcher(directory, fileNameFilter); 

            this._fileWatcher.Created += (s, e) => this.OnCreated(s, e);
            this._fileWatcher.Changed += (s, e) => this.OnChanged(s, e);
            this._fileWatcher.Renamed += (s, e) => this.OnRenamed(s, e);
            this._fileWatcher.Deleted += (s, e) => this.OnDeleted(s, e);

            this._timer = new Timer(this.OnTimer);

            this._fileWatcher.Created += this.FileChange;
            this._fileWatcher.Changed += this.FileChange;
            this._fileWatcher.Renamed += this.FileChange;
            this._fileWatcher.Deleted += this.FileChange;

            this.Zone = zoneName;
        }

        /// <summary>Start watching and generating zone files</summary>
        public override void Start(CancellationToken ct)
        {
            ct.Register(this.Stop);

            // fire first zone generation event on startup
            this._timer.Change(TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);
            this._fileWatcher.EnableRaisingEvents = true;
        }

        /// <summary>Handler for any file changes</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileChange(object sender, FileSystemEventArgs e)
        {
            this._timer.Change(this._settlement, Timeout.InfiniteTimeSpan);
        }

        /// <summary>Stop watching</summary>
        private void Stop()
        {
            this._fileWatcher.EnableRaisingEvents = false;
        }

        /// <summary>Handler for settlement completion</summary>
        /// <param name="state"></param>
        private void OnTimer(object state)
        {
            this._timer.Change(Timeout.Infinite, Timeout.Infinite);
            Task.Run(() => this.GenerateZone()).ContinueWith(t => this.Notify(t.Result));
        }


        public override void Dispose()
        {
            if (this._fileWatcher != null)
            {
                this._fileWatcher.EnableRaisingEvents = false;
                this._fileWatcher.Dispose();
            }

            if (this._timer != null)
            {
                this._timer.Change(Timeout.Infinite, Timeout.Infinite);
                this._timer.Dispose();
            }
        }
    }
}