using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DynDnsDistributor.Configuration
{
    public class DynDnsOptions
    {
        [Required, Url] public string IpRetrieveUrl { get; set; }
        public int? IpPollingInterval { get; set; }
        [Required] public string UserAgent { get; set; }
        public IList<Account> Accounts { get; set; }

        public class Account
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public bool Local { get; set; } = true;
            public IList<string> UpdateUrls { get; set; }
        }
    }
}