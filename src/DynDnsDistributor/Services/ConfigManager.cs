using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynDnsDistributor.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

        public Task UpdateAccount(ConfigFile.Account account)
        {
            if (account.CurrentIpAddress == null) return Task.CompletedTask;

            return Task.WhenAll(account.UpdateUrls
                .Select(x => UpdateDns(x, account.CurrentIpAddress.ToString())));
        }

        public Task UpdateAccount(ConfigFile.Account account, IPAddress ipaddr)
        {
            account.CurrentIpAddress = ipaddr;
            return UpdateAccount(account);
        }

        private void Load()
        {
            try
            {
                string config = File.ReadAllText(configPath);
                CurrentConfig = JsonConvert.DeserializeObject<ConfigFile>(config) ?? throw new Exception();
                _logger.LogInformation("Loaded new config:" + Environment.NewLine + 
                    JsonConvert.SerializeObject(CurrentConfig, Formatting.Indented));
                pollingTimer.Change(0, CurrentConfig.IpPollingInterval ?? -1);
            }
            catch (Exception ex)
            {
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
                    .Select(x => UpdateAccount(x, ipaddr)));
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

        public async Task UpdateDns(string url, string ipaddr)
        {
            Uri dest = new Uri(url.Replace("<ipaddr>", ipaddr));
            try
            {
                HttpWebRequest webRequest = WebRequest.CreateHttp(dest);
                if (!string.IsNullOrWhiteSpace(dest.UserInfo))
                {
                    int delimiter = dest.UserInfo.IndexOf(':');
                    if (delimiter == -1)
                    {
                        _logger.LogError("Invalid credentials in update URL");
                        return;
                    }
                    string username = dest.UserInfo.Remove(delimiter);
                    string password = dest.UserInfo.Substring(delimiter + 1);
                    webRequest.Credentials = new NetworkCredential(username, password);
                }
                HttpWebResponse webResponse = await webRequest.GetResponseAsync() as HttpWebResponse;
                _logger.LogInformation($"Successfully updated {dest.Host}: {webResponse.StatusCode}");
            }
            catch (WebException ex)
            {
                _logger.LogError(ex, $"Error while DNS update to {dest}");
            }
        }
    }
}
