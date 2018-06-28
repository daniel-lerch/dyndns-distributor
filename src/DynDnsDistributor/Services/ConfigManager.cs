using DynDnsDistributor.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DynDnsDistributor.Services
{
    public class ConfigManager : IConfigManager
    {
#if DEBUG
        private const string ConfigFilePath = "dyndnsconfig.Development.json";
#else
        private const string ConfigFilePath = "dyndnsconfig.json";
#endif
        private const int EventDelay = 500;
        private ILogger<ConfigManager> _logger;
        private FileSystemWatcher watcher;
        private Timer pollingTimer;
        private string configPath;

        public ConfigManager(ILogger<ConfigManager> logger)
        {
            _logger = logger;
            pollingTimer = new Timer(async o => await UpdateLocalAccounts(), null, -1, -1);
            configPath = Path.Combine(Environment.CurrentDirectory, ConfigFilePath);

            Load();

            watcher = new FileSystemWatcher()
            {
                Path = Environment.CurrentDirectory,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = ConfigFilePath
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

        public ConfigFile CurrentConfig { get; private set; }

        public bool ValidConfigFile { get; private set; }

        public async Task UpdateAccount(ConfigFile.Account account, IPAddress ipaddr, bool @override)
        {
            account.CurrentIpAddress = ipaddr;

            if (@override || account.CurrentIpAddress.Equals(account.PublishedIpAddress)) return;

            Task<bool>[] tasks = new Task<bool>[account.UpdateUrls.Count];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = UpdateDns(account.UpdateUrls[i], account.CurrentIpAddress.ToString());
            }
            bool success = true;
            for (int i = 0; i < tasks.Length; i++)
            {
                if (!await tasks[i]) success = false;
            }

            if (success) account.PublishedIpAddress = account.CurrentIpAddress;
        }

        private void Load()
        {
            try
            {
                string config = File.ReadAllText(configPath);
                CurrentConfig = JsonConvert.DeserializeObject<ConfigFile>(config) ?? throw new InvalidDataException();
                ValidConfigFile = true;
                _logger.LogInformation("Loaded new config:" + Environment.NewLine +
                    JsonConvert.SerializeObject(CurrentConfig, Formatting.Indented));
                pollingTimer.Change(0, CurrentConfig.IpPollingInterval ?? -1);
            }
            catch (Exception ex)
            {
                ValidConfigFile = false;
                _logger.LogError(ex, "Error while loading config");
            }
        }

        private async Task UpdateLocalAccounts()
        {
            WebClient webClient = new WebClient();
            try
            {
                string ipstr = await webClient.DownloadStringTaskAsync(CurrentConfig.IpRetrieveUrl);
                IPAddress ipaddr = IPAddress.Parse(ipstr);

                await Task.WhenAll(CurrentConfig.Accounts
                    .Where(x => CurrentConfig.LocalAccounts.Contains(x.Username))
                    .Select(x => UpdateAccount(x, ipaddr, false)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating local accounts");
            }
            finally
            {
                webClient.Dispose();
            }
        }

        public async Task<bool> UpdateDns(string url, string ipaddr)
        {
            Uri dest = new Uri(url.Replace("<ipaddr>", ipaddr));
            try
            {
                HttpWebRequest webRequest = WebRequest.CreateHttp(dest);
                if (!string.IsNullOrWhiteSpace(dest.UserInfo))
                {
                    string encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(dest.UserInfo));
                    webRequest.Headers.Add(HttpRequestHeader.Authorization, $"Basic {encoded}");
                    webRequest.Headers.Add(HttpRequestHeader.UserAgent, CurrentConfig.UserAgent);
                }
                HttpWebResponse webResponse = await webRequest.GetResponseAsync() as HttpWebResponse;
                using (Stream responseStream = webResponse.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string responseContent = await reader.ReadToEndAsync();
                    bool result = responseContent.StartsWith("good", StringComparison.OrdinalIgnoreCase) ||
                        responseContent.StartsWith("nochg", StringComparison.OrdinalIgnoreCase);
                    if (result)
                    {
                        _logger.LogInformation($"Successfully updated {dest.Host}: {webResponse.StatusCode}" +
                            Environment.NewLine + responseContent);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning($"Updating {dest.Host} returned {webResponse.StatusCode}" +
                            Environment.NewLine + responseContent);
                        return false;
                    }
                }
            }
            catch (WebException ex)
            {
                _logger.LogError(ex, $"Error while DNS update to {dest}");
                return false;
            }
        }
    }
}
