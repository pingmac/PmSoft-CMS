using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ManagementWebHost.Models;
using PmSoft.Caching;
using PmSoft.Events;

namespace ManagementWebHost.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICacheService cacheService;

        public HomeController(ILogger<HomeController> logger, ICacheService cacheService)
        {
            _logger = logger;
            this.cacheService = cacheService;
        }

        public async Task<IActionResult> Index()
        {
            await RedisEventBus<object, CommonEventArgs>.Instance().PublishAsync(1, new CommonEventArgs(string.Empty));
            return Json(new { });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
