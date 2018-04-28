using DynDnsDistributor.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DynDnsDistributor.Config
{
    public static class Manager
    {
        public static ConfigFile CurrentConfig { get; private set; }
        private static FileSystemWatcher watcher;
        private static Timer pollingTimer;
        private const int EventDelay = 500;
        private static string configPath;

        public static void Initialize()
        {
            pollingTimer = new Timer(async o => await UpdateLocalAccounts(), null, -1, -1);
            configPath = Path.Combine(Environment.CurrentDirectory, "dyndnsconfig.json");

            Load();

            watcher = new FileSystemWatcher()
            {
                Path = Environment.CurrentDirectory,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "dyndnsconfig.json"
            };

            Observable.FromEventPattern<EventHandler<FileSystemEventArgs>, FileSystemEventArgs>(
                x => watcher.Changed += new FileSystemEventHandler(x),
                x => watcher.Changed -= new FileSystemEventHandler(x)
            )
            .Throttle(TimeSpan.FromMilliseconds(EventDelay))
            .Select(ep => ep.EventArgs)
            .Where(ev => ev.ChangeType != WatcherChangeTypes.Deleted &&
                ev.FullPath == configPath)
            .Subscribe(ev => Load());

            watcher.EnableRaisingEvents = true;
        }

        private static void Load()
        {
            try
            {
                string config = File.ReadAllText(configPath);
                CurrentConfig = JsonConvert.DeserializeObject<ConfigFile>(config) ?? throw new Exception();
                Debug.WriteLine("Loaded new config:");
                Debug.WriteLine(JsonConvert.SerializeObject(CurrentConfig));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while loading config {ex}");
                return;
            }
            pollingTimer.Change(0, CurrentConfig.IpPollingInterval ?? -1);
        }

        private static async Task UpdateLocalAccounts()
        {
            WebClient webClient = new WebClient();
            try
            {
                string ipstr = await webClient.DownloadStringTaskAsync(CurrentConfig.IpRetrieveUrl);
                IPAddress ipaddr = IPAddress.Parse(ipstr);

                await Task.WhenAll(CurrentConfig.Accounts
                    .Where(x => CurrentConfig.LocalAccounts.Contains(x.Username))
                    .Select(x => x.Update(ipaddr)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while updating local accounts {ex}");
            }
            finally
            {
                webClient.Dispose();
            }
        }
    }
}