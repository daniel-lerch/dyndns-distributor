using DynDnsDistributor.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DynDnsDistributor.Services
{
    public class DnsUpdateService
    {
        private readonly IOptionsMonitor<DynDnsOptions> optionsMonitor;
        private readonly ILogger<DnsUpdateService> logger;

        public DnsUpdateService(IOptionsMonitor<DynDnsOptions> optionsMonitor, ILogger<DnsUpdateService> logger)
        {
            this.optionsMonitor = optionsMonitor;
            this.logger = logger;
        }

        public async Task<bool> UpdateAsync(string url, string ipaddr)
        {
            Uri dest = new Uri(url.Replace("<ipaddr>", ipaddr));
            try
            {
                HttpWebRequest webRequest = WebRequest.CreateHttp(dest);
                if (!string.IsNullOrWhiteSpace(dest.UserInfo))
                {
                    string encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(dest.UserInfo));
                    webRequest.Headers.Add(HttpRequestHeader.Authorization, $"Basic {encoded}");
                    webRequest.Headers.Add(HttpRequestHeader.UserAgent, optionsMonitor.CurrentValue.UserAgent);
                }
                HttpWebResponse webResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                using (Stream responseStream = webResponse.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string responseContent = await reader.ReadToEndAsync();
                    bool result = responseContent.StartsWith("good", StringComparison.OrdinalIgnoreCase) ||
                        responseContent.StartsWith("nochg", StringComparison.OrdinalIgnoreCase);
                    if (result)
                    {
                        logger.LogInformation($"Successfully updated {dest.Host}: {webResponse.StatusCode}" +
                            Environment.NewLine + responseContent);
                        return true;
                    }
                    else
                    {
                        logger.LogWarning($"Updating {dest.Host} returned {webResponse.StatusCode}" +
                            Environment.NewLine + responseContent);
                        return false;
                    }
                }
            }
            catch (WebException ex)
            {
                logger.LogError(ex, $"Error while DNS update to {dest}");
                return false;
            }
        }
    }
}
