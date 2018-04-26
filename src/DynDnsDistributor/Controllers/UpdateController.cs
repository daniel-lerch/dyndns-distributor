using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DynDnsDistributor.Controllers
{
    [Route("[controller]")]
    public class UpdateController : Controller
    {
        // GET update?hostname=<hostname>&myip=<ipaddr>
        [HttpGet]
        public IActionResult Get([FromQuery(Name = "hostname")]string hostname, [FromQuery(Name = "myip")]string myip)
        {
            return StatusCode(501);
        }
    }
}
