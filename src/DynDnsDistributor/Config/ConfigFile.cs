using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DynDnsDistributor.Config
{
    public class ConfigFile
    {
        public string IpRetrieveUrl { get; set; }
        public int? IpPollingInterval { get; set; }
        public IList<string> LocalAccounts { get; set; }
        public IList<Account> Accounts { get; set; }

        public class Account
        {
            public string Hostname { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public IList<string> UpdateUrls { get; set; }

            [JsonIgnore]
            public IPAddress CurrentIpAddress { get; set; }
        }
    }
}