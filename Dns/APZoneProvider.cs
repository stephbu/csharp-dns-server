// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="ZoneProvider.cs" company="stephbu">
// // // Copyright (c) Steve Butler. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace Dns
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;

    /// <summary>Source of Zone records</summary>
    public class APZoneProvider : IObservable<Zone>, IDisposable
    {
        private string _machineInfoFile;
        private FileSystemWatcher _machineWatcher;
        private readonly List<IObserver<Zone>> _observers = new List<IObserver<Zone>>();
        private TimeSpan _settlement = TimeSpan.FromSeconds(10);
        private Timer _timer;
        private string _zoneSuffix;
        private uint _serial;

        /// <summary>Timespan between last file change and zone generation</summary>
        public TimeSpan FileSettlementPeriod
        {
            get { return this._settlement; }
            set { this._settlement = value; }
        }

        void IDisposable.Dispose()
        {
            if (_machineWatcher != null)
            {
                _machineWatcher.EnableRaisingEvents = false;
                _machineWatcher.Dispose();
            }
        }

        /// <summary>Subscribe Observer to zone publishing</summary>
        /// <param name="observer">Observer</param>
        /// <returns>Subscription object that subscriber must maintain, and dispose when subscription cancellation is required</returns>
        IDisposable IObservable<Zone>.Subscribe(IObserver<Zone> observer)
        {
            _observers.Add(observer);
            return new Subscription(this, observer);
        }

        /// <summary>Initialize ZoneProvider</summary>
        /// <param name="machineInfoFile"></param>
        /// <param name="zoneSuffix"></param>
        public void Initialize(string machineInfoFile, string zoneSuffix)
        {
            if (string.IsNullOrWhiteSpace(machineInfoFile))
            {
                throw new ArgumentException("Null or empty", "machineInfoFile");
            }

            machineInfoFile = Environment.ExpandEnvironmentVariables(machineInfoFile);
            machineInfoFile = Path.GetFullPath(machineInfoFile);

            if (!File.Exists(machineInfoFile))
            {
                throw new FileNotFoundException("machineInfoFile not found", machineInfoFile);
            }

            _machineInfoFile = machineInfoFile;
            _zoneSuffix = zoneSuffix;

            string directory = Path.GetDirectoryName(machineInfoFile);
            string fileNameFilter = Path.GetFileName(machineInfoFile);

            _machineWatcher = new FileSystemWatcher(directory, fileNameFilter);
            _machineWatcher.Created += _machineWatcher_OnChanged;
            _machineWatcher.Changed += _machineWatcher_OnChanged;
            _machineWatcher.Renamed += _machineWatcher_OnChanged;
            _machineWatcher.Deleted += _machineWatcher_OnChanged;
            _timer = new Timer(OnTimer);
        }

        /// <summary>Start watching and generating zone files</summary>
        public void Start()
        {
            // fire first zone generation event on startup
            _timer.Change(TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);
            _machineWatcher.EnableRaisingEvents = true;
        }

        /// <summary>Stop watching</summary>
        public void Stop()
        {
            _machineWatcher.EnableRaisingEvents = false;
        }

        /// <summary>Publish zone to all subscribers</summary>
        /// <param name="zone"></param>
        private void Notify(Zone zone)
        {
            int remainingRetries = 3;

            while (remainingRetries > 0)
            {
                ParallelLoopResult result = Parallel.ForEach(_observers, observer => observer.OnNext(zone));
                if (result.IsCompleted)
                {
                    break;
                }
                remainingRetries--;
            }
        }

        /// <summary>Removes observer subscription</summary>
        /// <param name="observer"></param>
        private void Unsubscribe(IObserver<Zone> observer)
        {
            this._observers.Remove(observer);
        }

        /// <summary>Handler for any file changes</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _machineWatcher_OnChanged(object sender, FileSystemEventArgs e)
        {
            _timer.Change(_settlement, Timeout.InfiniteTimeSpan);
        }

        /// <summary>Handler for settlement completion</summary>
        /// <param name="state"></param>
        private void OnTimer(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            Task.Run(() => CreateZone())
                .ContinueWith(task => this.Notify(task.Result));
        }

        /// <summary>Generates zone</summary>
        /// <returns></returns>
        private Zone CreateZone()
        {
            if (!File.Exists(_machineInfoFile))
            {
                return null;
            }
            
            CsvParser parser = CsvParser.Create(_machineInfoFile);
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

        /// <summary>Subscription memento for IObservable interface</summary>
        public class Subscription : IDisposable
        {
            private IObserver<Zone> _observer;
            private APZoneProvider _provider;

            public Subscription(APZoneProvider provider, IObserver<Zone> observer)
            {
                this._provider = provider;
                this._observer = observer;
            }

            void IDisposable.Dispose()
            {
                this._provider.Unsubscribe(this._observer);
            }
        }
    }
}