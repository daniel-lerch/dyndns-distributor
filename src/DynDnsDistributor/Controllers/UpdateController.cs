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
        // GET update?myip=<ipaddr>
        [HttpGet]
        public IActionResult Get([FromQuery(Name = "myip")]string myip)
        {
            if (string.IsNullOrWhiteSpace(myip))
                return StatusCode(400);

            //Request.Headers["Authorization"];

            return StatusCode(501);
        }
    }
}
