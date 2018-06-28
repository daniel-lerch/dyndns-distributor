using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynDnsDistributor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DynDnsDistributor.Controllers
{
    [Route("[controller]")]
    public class AppInitController : Controller
    {
        private readonly IConfigManager _configManager;
        private readonly ILogger<AppInitController> _logger;

        public AppInitController(IConfigManager configManager, ILogger<AppInitController> logger)
        {
            _configManager = configManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            if (_configManager.ValidConfigFile)
                return StatusCode(200, "Running");
            else
                return StatusCode(500, "Invalid config file");
        }
    }
}
