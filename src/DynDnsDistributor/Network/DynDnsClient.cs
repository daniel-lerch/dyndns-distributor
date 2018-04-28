using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DynDnsDistributor.Network
{
    public static class DynDnsClient
    {
        public static Task Update(this Config.ConfigFile.Account account, IPAddress ipaddr)
        {
            account.CurrentIpAddress = ipaddr;
            return account.Update();
        }

        public static Task Update(this Config.ConfigFile.Account account)
        {
            if (account.CurrentIpAddress == null) return Task.CompletedTask;

            return Task.WhenAll(account.UpdateUrls
                .Select(x => UpdateDns(x, account.CurrentIpAddress.ToString())));
        }

        public static async Task UpdateDns(string url, string ipaddr)
        {
            try
            {
                Uri dest = new Uri(url.Replace("<ipaddr>", ipaddr));
                HttpWebRequest webRequest = WebRequest.CreateHttp(dest);
                if (!string.IsNullOrWhiteSpace(dest.UserInfo))
                {
                    int delimiter = dest.UserInfo.IndexOf(':');
                    if (delimiter == -1)
                    {
                        Debug.WriteLine("Invalid credentials in update URL");
                        return;
                    }
                    string username = dest.UserInfo.Remove(delimiter);
                    string password = dest.UserInfo.Substring(delimiter + 1);
                    webRequest.Credentials = new NetworkCredential(username, password);
                }
                HttpWebResponse webResponse = await webRequest.GetResponseAsync() as HttpWebResponse;
                Debug.WriteLine($"Successfully updated {dest.Host}: {webResponse.StatusCode}");
            }
            catch (WebException ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
