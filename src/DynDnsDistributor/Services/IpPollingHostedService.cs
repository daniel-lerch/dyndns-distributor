using DynDnsDistributor.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DynDnsDistributor.Services
{
    public class IpPollingHostedService : IHostedService
    {
        private readonly IOptionsMonitor<DynDnsOptions> optionsMonitor;
        private readonly DnsUpdateService updateService;
        private readonly ILogger<IpPollingHostedService> logger;
        private readonly Timer timer;
        private readonly IDisposable eventHander;
        private string? lastIpAddress;

        public IpPollingHostedService(IOptionsMonitor<DynDnsOptions> optionsMonitor, DnsUpdateService updateService, ILogger<IpPollingHostedService> logger)
        {
            this.optionsMonitor = optionsMonitor;
            this.updateService = updateService;
            this.logger = logger;
            eventHander = optionsMonitor.OnChange(async (options, name) =>
            {
                logger.LogInformation("Configuration has been changed and reloaded. Updating targets...");
                await UpdateLocalAccounts();
            });
            timer = new Timer(async state => await UpdateLocalAccounts());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            int intervall = optionsMonitor.CurrentValue.IpPollingInterval ?? -1;
            timer.Change(0, intervall);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Dispose();
            eventHander.Dispose();
            return Task.CompletedTask;
        }

        private async Task UpdateLocalAccounts()
        {
            string? address = await GetPublicAddress();
            if (address == null || address == lastIpAddress)
                return;

            lastIpAddress = address;

            foreach (DynDnsOptions.Account account in optionsMonitor.CurrentValue.Accounts)
            {
                if (account.Local)
                {
                    foreach (string url in account.UpdateUrls)
                        await updateService.UpdateAsync(url, address);
                }
            }
        }

        private Task<string?> GetPublicAddress()
        {
            WebClient client = new WebClient();
            try
            {
                return client.DownloadStringTaskAsync(optionsMonitor.CurrentValue.IpRetrieveUrl);
            }
            catch (WebException ex)
            {
                logger.LogError(ex, "Failed to fetch public IP address from {0}", optionsMonitor.CurrentValue.IpRetrieveUrl);
                return Task.FromResult<string?>(null);
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}
