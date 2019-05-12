using DynDnsDistributor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DynDnsDistributor.Services
{
    public interface IConfigManager
    {
        DynDnsOptions CurrentConfig { get; }
        bool ValidConfigFile { get; }
        Task UpdateAccount(DynDnsOptions.Account account, IPAddress ipaddr, bool @override);
    }
}
