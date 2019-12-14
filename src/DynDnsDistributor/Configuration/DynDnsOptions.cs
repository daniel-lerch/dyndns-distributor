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
        [Required, Url] public string IpRetrieveUrl { get; set; } = null!;
        public int? IpPollingInterval { get; set; }
        [Required] public string UserAgent { get; set; } = null!;
        public IList<Account> Accounts { get; set; } = new List<Account>();

        public class Account
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
            public bool Local { get; set; } = true;
            public IList<string> UpdateUrls { get; set; } = new List<string>();
        }
    }
}