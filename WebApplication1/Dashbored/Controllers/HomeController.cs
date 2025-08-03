// Dashboard/Controllers/HomeController.cs
using Microsoft.AspNetCore.Authorization; // لإضافة حماية الوصول
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // أضف هذه المكتبة

namespace YourProject.Dashboard.Controllers // استبدل YourProject باسم مشروعك
{
    // [Authorize] // يمكنك إضافة هذا السطر لحماية المتحكم بالكامل، بحيث لا يمكن الوصول إليه إلا للمستخدمين المسجلين
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Home page accessed.");
            return View();
        }

        // يمكنك إضافة إجراءات أخرى للوحة التحكم هنا
        public IActionResult Privacy()
        {
            _logger.LogInformation("Privacy page accessed.");
            return View();
        }
    }
}