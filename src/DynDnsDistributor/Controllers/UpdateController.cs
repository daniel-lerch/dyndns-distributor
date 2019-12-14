using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DynDnsDistributor.Configuration;
using DynDnsDistributor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace DynDnsDistributor.Controllers
{
    [Route("[controller]")]
    public class UpdateController : Controller
    {
        private readonly IOptionsMonitor<DynDnsOptions> optionsMonitor;
        private readonly DnsUpdateService updateService;

        public UpdateController(IOptionsMonitor<DynDnsOptions> optionsMonitor, DnsUpdateService updateService)
        {
            this.optionsMonitor = optionsMonitor;
            this.updateService = updateService;
        }

        // GET update?myip=<ipaddr>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery(Name = "myip")]string myip)
        {
            if (string.IsNullOrWhiteSpace(myip))
                return StatusCode(400, "Please specify your IP address.");
            if (!IPAddress.TryParse(myip, out _))
                return StatusCode(400, "Invalid IP address format.");

            string? username = null;
            string? password = null;
            string authHeader = Request.Headers["Authorization"];
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(400, "Only basic authentication is supported.");

                try
                {
                    byte[] buffer = Convert.FromBase64String(authHeader.Substring(6));
                    authHeader = Encoding.ASCII.GetString(buffer);
                }
                catch
                {
                    return StatusCode(400, "You did not send a valid authorization header.");
                }

                int split = authHeader.IndexOf(':');
                if (split == -1)
                    return StatusCode(400, "You did not send a valid authorization header.");
                username = authHeader.Remove(split);
                password = authHeader.Substring(split + 1);
            }

            DynDnsOptions.Account account = optionsMonitor.CurrentValue.Accounts
                .Where(a => a.Username == username && a.Password == password).FirstOrDefault();

            if (account == null)
            {
                Response.Headers.Add(HeaderNames.WWWAuthenticate, "Basic realm=\"User Visible Realm\"");
                return StatusCode(401);
            }

            foreach (string url in account.UpdateUrls)
            {
                await updateService.UpdateAsync(url, myip);
            }
            return StatusCode(200);
        }
    }
}