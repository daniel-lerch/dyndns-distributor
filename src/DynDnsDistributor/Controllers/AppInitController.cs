using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DynDnsDistributor.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DynDnsDistributor.Controllers
{
    [Route("[controller]")]
    public class AppInitController : Controller
    {
        private readonly IOptionsMonitor<DynDnsOptions> optionsMonitor;

        public AppInitController(IOptionsMonitor<DynDnsOptions> optionsMonitor)
        {
            this.optionsMonitor = optionsMonitor;
        }

        [HttpGet]
        public IActionResult Get()
        {
            DynDnsOptions options = optionsMonitor.CurrentValue;
            List<ValidationResult> results = new List<ValidationResult>();
            if (Validator.TryValidateObject(options, new ValidationContext(options), results))
            {
                int updateUrls = options.Accounts.SelectMany(acc => acc.UpdateUrls).Count();
                return StatusCode(200, $"Running ({options.Accounts.Count} accounts with {updateUrls} update URLs)");
            }
            else
                return StatusCode(500, results);
        }
    }
}
