using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DynDnsDistributor.Config
{
    public static class Manager
    {
        public static ConfigFile CurrentConfig { get; private set; }
        private static FileSystemWatcher watcher;

        public static void Initialize()
        {
            Load();
            watcher = new FileSystemWatcher(Environment.CurrentDirectory);
            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = true;
        }

        private static void Load()
        {
            System.Threading.Thread.Sleep(50);
            string config = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "dyndnsconfig.json"));
            CurrentConfig = JsonConvert.DeserializeObject<ConfigFile>(config) ?? throw new Exception();
            Debug.WriteLine("Loaded new config:");
            Debug.WriteLine(JsonConvert.SerializeObject(CurrentConfig));
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == Path.Combine(Environment.CurrentDirectory, "dyndnsconfig.json") &&
                !e.ChangeType.HasFlag(WatcherChangeTypes.Deleted))
            {
                Load();
            }
        }
    }
}
