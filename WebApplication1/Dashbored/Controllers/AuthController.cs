// Presentation/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Application.IServices; // لـIAuthService
using Application.DTOs; // لـLoginDto, RegisterDto, UserDto
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // لـ[AllowAnonymous] و [Authorize]
using Microsoft.AspNetCore.Authentication; // لـSignOutAsync

namespace Dashbored.Controllers
{
    // [Route("[controller]")] // يمكنك استخدام هذا إذا كنت تفضل Web API-style routing
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // --- صفحة تسجيل الدخول (GET) ---
        [HttpGet]
        [AllowAnonymous] // للسماح بالوصول لهذه الصفحة بدون مصادقة
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(); // سيبحث عن Views/Auth/Login.cshtml
        }

        // --- معالجة طلب تسجيل الدخول (POST) ---
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] // حماية ضد هجمات CSRF
        public async Task<IActionResult> Login(loginDto model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                // إذا كانت البيانات المدخلة غير صحيحة (مثلاً، حقول فارغة بسبب [Required])
                return View(model);
            }

            // استدعاء خدمة المصادقة
            var userDto = await _authService.SignInAsync(model);

            if (userDto == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt.");
                return View(model);
            }

            // إذا نجح تسجيل الدخول، يتم الآن تخزين معلومات المستخدم والتوكن
            // في MVC، عادة ما يتم استخدام Cookies للمصادقة بعد الحصول على التوكن.
            // يمكنك تعيين الـJWT Token كـCookie آمن (HttpOnly) هنا إذا كنت تعتمد على الكوكيز للمصادقة
            // أو إعادته إلى الواجهة الأمامية إذا كنت تستخدم JS لـclient-side token storage.
            // مثال لتعيين كوكي:
            // Response.Cookies.Append("AuthToken", userDto.value, new CookieOptions
            // {
            //     HttpOnly = true,
            //     Secure = true, // يجب أن يكون true في بيئة الإنتاج مع HTTPS
            //     Expires = DateTime.UtcNow.AddHours(1) // مدة صلاحية الكوكي
            // });

            // في سيناريو MVC النموذجي، بعد SignInAsync، ستقوم ASP.NET Identity
            // بإعداد الـAuthentication Cookie تلقائيًا.
            // بما أن AuthService يستخدم SignInManager، فإن هذا الجزء يتم معالجته
            // بواسطة Identity نفسه. كل ما تحتاجه هو توجيه المستخدم.

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                // التوجيه إلى لوحة التحكم أو الصفحة الرئيسية بعد تسجيل الدخول الناجح
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // --- صفحة التسجيل (GET) ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(); // سيبحث عن Views/Auth/Register.cshtml
        }

        // --- معالجة طلب التسجيل (POST) ---
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Password and confirmation password do not match.");
                return View(model);
            }

            var result = await _authService.CreateAccountAsync(model);

            if (result.Succeeded)
            {
                // بعد التسجيل الناجح، قم بتوجيه المستخدم لتسجيل الدخول
                // أو قم بتسجيل دخوله تلقائياً إذا كان هذا هو منطق عملك
                return RedirectToAction("Login", "Auth"); // توجيه لصفحة تسجيل الدخول
            }

            // إذا فشل التسجيل، أضف الأخطاء إلى ModelState
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // --- تسجيل الخروج (POST) ---
        [HttpPost]
        [Authorize] // يتطلب المستخدم أن يكون مصادقاً لتسجيل الخروج
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            // بعد تسجيل الخروج، أزل الكوكي الذي يحتوي على التوكن إذا كنت تستخدمه يدوياً
            // Response.Cookies.Delete("AuthToken");
            return RedirectToAction("Index", "Home"); // أو صفحة تسجيل الدخول
        }

        // --- رسالة الوصول المرفوض (إذا حاول مستخدم غير مصادق الوصول إلى صفحة محمية) ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View(); // سيبحث عن Views/Auth/AccessDenied.cshtml
        }
    }
}