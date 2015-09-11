// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="ZoneProvider.cs" company="stephbu">
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
    using System.Linq;

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
        private readonly Timer _timer;

        public abstract Zone GenerateZone();

        /// <summary>Timespan between last file change and zone generation</summary>
        public TimeSpan FileSettlementPeriod
        {
            get { return this._settlement; }
            set { this._settlement = value; }
        }

        public string Filename { get; private set; }

        protected FileWatcherZoneProvider(string filename)
        {
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

            _timer = new Timer(OnTimer);

            _fileWatcher.Created += this.FileChange;
            _fileWatcher.Changed += this.FileChange;
            _fileWatcher.Renamed += this.FileChange;
            _fileWatcher.Deleted += this.FileChange;
        }

        /// <summary>Start watching and generating zone files</summary>
        public void Start()
        {
            // fire first zone generation event on startup
            _timer.Change(TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);
            _fileWatcher.EnableRaisingEvents = true;
        }

        /// <summary>Handler for any file changes</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileChange(object sender, FileSystemEventArgs e)
        {
            _timer.Change(_settlement, Timeout.InfiniteTimeSpan);
        }

        /// <summary>Stop watching</summary>
        public void Stop()
        {
            _fileWatcher.EnableRaisingEvents = false;
        }

        /// <summary>Handler for settlement completion</summary>
        /// <param name="state"></param>
        private void OnTimer(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            Task.Run(() => this.GenerateZone()).ContinueWith(t => this.Notify(t.Result));
        }


        public override void Dispose()
        {
            if (this._fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = false;
                _fileWatcher.Dispose();
            }

            if (this._timer != null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                this._timer.Dispose();
            }
        }
    }

    /// <summary>Source of Zone records</summary>
    public class APZoneProvider : FileWatcherZoneProvider
    {
        private string _machineInfoFile;
        private string _zoneSuffix;
        private uint _serial;

        public APZoneProvider(string machineInfoFile, string zoneSuffix) : base(machineInfoFile)
        {
            this.Initialize(machineInfoFile, zoneSuffix);
        }

        /// <summary>Initialize ZoneProvider</summary>
        /// <param name="machineInfoFile"></param>
        /// <param name="zoneSuffix"></param>
        public void Initialize(string machineInfoFile, string zoneSuffix)
        {
            _zoneSuffix = zoneSuffix;
        }

        public override Zone GenerateZone()
        {
            if (!File.Exists(this.Filename))
            {
                return null;
            }

            CsvParser parser = CsvParser.Create(this.Filename);
            var machines = parser.Rows.Select(row => new {MachineFunction = row["MachineFunction"], StaticIP = row["StaticIP"], MachineName = row["MachineName"]}).ToArray();

            var zoneRecords = machines
                            .GroupBy(machine => machine.MachineFunction + _zoneSuffix, machine => IPAddress.Parse(machine.StaticIP))
                            .Select(group => new ZoneRecord {Host = group.Key, Count = group.Count(), Addresses = group.Select(address => address).ToArray()})
                            .ToArray();

            Zone zone = new Zone();
            zone.Suffix = _zoneSuffix;
            zone.Serial = _serial;
            zone.Initialize(zoneRecords);

            // increment serial number
            _serial++;
            return zone;
        }
    }
}