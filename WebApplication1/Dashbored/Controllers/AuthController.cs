// Dashboard/Controllers/AuthController.cs
using Application.DTOs; // تأكد من أن هذا المسار صحيح لـ RegisterDto و LoginDto
using Application.IServices; // تأكد من أن هذا المسار صحيح لـ IAuthService
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // أضف هذه المكتبة لتسجيل الأخطاء

namespace Dashboard.Controllers // استبدل YourProject باسم مشروعك
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger; // لحقن مسجل الأخطاء

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // GET: لعرض صفحة التسجيل
        public IActionResult Register()
        {
            return View();
        }

        // POST: لمعالجة بيانات التسجيل المرسلة
        [HttpPost]
        [ValidateAntiForgeryToken] // حماية ضد هجمات CSRF
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var token = await _authService.CreateAccountAsync(model);
                    if (!string.IsNullOrEmpty(token))
                    {
                        // تم التسجيل بنجاح، يمكنك الآن إعادة توجيه المستخدم
                        // مثلاً إلى صفحة الدخول أو مباشرة إلى لوحة التحكم
                        _logger.LogInformation("User {Email} registered successfully.", model.Email);
                        // بعد التسجيل الناجح، قم بتسجيل الدخول تلقائيًا أو اطلب منهم تسجيل الدخول
                        return RedirectToAction("Login", "Auth"); // أعد التوجيه إلى صفحة الدخول
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                        _logger.LogWarning("Registration failed for {Email}. AuthService returned null token.", model.Email);
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Error during user registration for {Email}.", model.Email);
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred during registration.");
                }
            }
            // إذا لم يكن النموذج صالحاً أو فشل التسجيل، أعد عرض النموذج مع الأخطاء
            return View(model);
        }

        // GET: لعرض صفحة الدخول
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: لمعالجة بيانات الدخول المرسلة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(loginDto model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                try
                {
                    var userDto = await _authService.SignInAsync(model);
                    if (userDto != null)
                    {
                        _logger.LogInformation("User {Email} logged in successfully.", model.Email);
                        // تم الدخول بنجاح، أعد التوجيه إلى لوحة التحكم
                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home"); // توجيه إلى صفحة Dashboard الرئيسية
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        _logger.LogWarning("Login failed for {Email}. Invalid credentials.", model.Email);
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Error during user login for {Email}.", model.Email);
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred during login.");
                }
            }
            // إذا لم يكن النموذج صالحاً أو فشل الدخول، أعد عرض النموذج مع الأخطاء
            return View(model);
        }

        // POST: لتسجيل الخروج
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            _logger.LogInformation("User logged out successfully.");
            return RedirectToAction("Login", "Auth"); // أعد التوجيه إلى صفحة الدخول بعد تسجيل الخروج
        }
    }
}