using DynDnsDistributor.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DynDnsDistributor.Services
{
    public interface IConfigManager
    {
        ConfigFile CurrentConfig { get; }
        Task UpdateAccount(ConfigFile.Account account, IPAddress ipaddr, bool @override);
    }
}
