using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // 1. إضافة هذا السطر

namespace Dashboard.Controllers
{
   // [Authorize]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // 2. تعريف حقل الـ Logger

        // 3. حقن ILogger في الباني
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // 4. استخدام الـ Logger لتسجيل معلومات عند الوصول إلى الصفحة
            _logger.LogInformation("User accessed the Home dashboard page.");

            return View();
        }

        [AllowAnonymous] 
        public IActionResult Privacy()
        {
            _logger.LogInformation("Privacy page was accessed.");
            return View();
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            _logger.LogWarning("User attempted to access a restricted resource and was redirected to Access Denied page.");
            return View();
        }
    }
}