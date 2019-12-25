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
using ManagementWebHost.EventMoudles;

namespace ManagementWebHost.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICacheService cacheService;
        private readonly IEventBus eventBus;

        public HomeController(ICacheService cacheService, IEventBus eventBus)
        {
            this.cacheService = cacheService;
            this.eventBus = eventBus;
        }

        public async Task<IActionResult> Index()
        {
            await eventBus.PublishAsync<object, CommonEventArgs>(1, new CommonEventArgs(string.Empty));
            await eventBus.PublishAsync<int, CommonEventArgs>(2, new CommonEventArgs(string.Empty));
            await eventBus.PublishAsync<object, UserEventArgs>(1, new UserEventArgs());
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
