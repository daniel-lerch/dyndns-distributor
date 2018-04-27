using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynDnsDistributor.Config
{
    public class ConfigFile
    {
        public IList<Account> Accounts { get; set; }

        public class Account
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public IList<string> UpdateUrls { get; set; }
        }
    }
}