using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DynDnsDistributor.Network;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace DynDnsDistributor.Controllers
{
    [Route("[controller]")]
    public class UpdateController : Controller
    {
        // GET update?myip=<ipaddr>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery(Name = "hostname")]string hostname, [FromQuery(Name = "myip")]string myip)
        {
            if (string.IsNullOrWhiteSpace(myip))
                return StatusCode(400, "Please specify your IP address.");

            if (string.IsNullOrWhiteSpace(hostname)) hostname = null;
            string username = null;
            string password = null;
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

            Config.ConfigFile.Account account = Config.Manager.CurrentConfig.Accounts
                .Where(a => a.Hostname == hostname &&
                a.Username == username && 
                a.Password == password).FirstOrDefault();

            if (account == null)
            {
                Response.Headers.Add(HeaderNames.WWWAuthenticate, "Basic realm=\"User Visible Realm\"");
                return StatusCode(401);
            }

            account.CurrentIpAddress = IPAddress.Parse(myip);

            await account.Update();
            return StatusCode(200);
        }
    }
}