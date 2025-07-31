// Infrastructure/Services/AuthService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // لـIConfiguration
using Application.DTOs; // لـRegisterDto, UserDto, LoginDto, LoginResponseDto
using Application.IServices; // لـIAuthService
using Domain.Entities; // لـUser Entity
using Microsoft.AspNetCore.Identity; // لـUserManager, SignInManager, IdentityResult, RoleManager
using Infrastructure.Authentication; // لـIJwtTokenGenerator
using Infrastructure.Contexts;
using AutoMapper; // لـIMapper
using Microsoft.EntityFrameworkCore; // لـ.Include() (مهم جداً لجلب Wallet)
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly RoleManager<IdentityRole> _roleManager; // لحقن RoleManager
        private readonly IMapper _mapper; // لحقن IMapper
        private readonly IConfiguration _configuration; // لحقن IConfiguration
        private readonly AppDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly IWalletService _IWalletService;

        // Constructor المصحح: حقن جميع التبعيات المطلوبة
        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtTokenGenerator jwtTokenGenerator, // استخدام الواجهة
            RoleManager<IdentityRole> roleManager, // حقن RoleManager
            IMapper mapper, // حقن IMapper
            IConfiguration configuration,
            AppDbContext context,
            ILogger<AuthService> logger ,
            IWalletService walletService) // حقن IConfiguration
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _roleManager = roleManager;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
            _context= context;
            _IWalletService = walletService;
        }


        // دالة تسجيل المستخدم: CreateAccountAsync (مع Transaction وإنشاء المحفظة)
        public async Task<string?> CreateAccountAsync(RegisterDto dto)
        {
            // <--- بداية الـTransaction لضمان الذرية --->
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. إنشاء كائن المستخدم (User Entity)
                    var user = _mapper.Map<User>(dto);
                    user.EmailConfirmed = true;
                    user.CreatedAt = DateTime.UtcNow;
                    user.UpdatedAt = DateTime.UtcNow;

                    // محاولة إنشاء المستخدم في Identity (سيولد User.Id)
                    var createResult = await _userManager.CreateAsync(user, dto.Password);

                    if (!createResult.Succeeded)
                    {
                        // إذا فشل إنشاء المستخدم، تراجع عن المعاملة
                        await transaction.RollbackAsync();
                        _logger.LogWarning("User creation failed for {Email}. Errors: {Errors}", dto.Email, string.Join("; ", createResult.Errors.Select(e => e.Description)));
                        return null; // أو رمي استثناء مخصص
                    }

                    // 2. إضافة المستخدم للدور
                    if (!await _roleManager.RoleExistsAsync(dto.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(dto.Role));
                    }
                    await _userManager.AddToRoleAsync(user, dto.Role);

                    // 4. إنشاء محفظة جديدة للمستخدم
                    var newWallet = new Wallet
                    {
                        WalletId = Guid.NewGuid(), // سيتم توليد ID للمحفظة
                        Balance = 0.00M, // <--- رصيد ابتدائي صفر كما طلب
                        UserId = user.Id, // <--- تعيين UserId للمحفظة من المستخدم الذي تم إنشاؤه
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    };

                    // ربط المحفظة بالمستخدم (في الـNavigation Property) - اختياري، لكن جيد
                    user.Wallet = newWallet;

                    // حفظ المحفظة في قاعدة البيانات (ضمن نفس Transaction)
                    _context.Add(newWallet);
                    await _context.SaveChangesAsync(); // <--- حفظ المحفظة هنا

                    // 5. توليد التوكن للمستخدم
                    var roles = await _userManager.GetRolesAsync(user);
                    string singleRole = roles.FirstOrDefault();
                    var token = _jwtTokenGenerator.GenerateToken(user, singleRole);

                    // <--- تأكيد (Commit) الـTransaction بعد نجاح جميع الخطوات --->
                    await transaction.CommitAsync();
                    _logger.LogInformation("User {UserId} ({Email}) registered successfully with wallet {WalletId} and token generated.", user.Id, dto.Email, newWallet.WalletId);

                    // إرجاع سلسلة نصية تحتوي على المعلومات المطلوبة
                    return token;
                }
                catch (Exception ex)
                {
                    // في حال حدوث أي خطأ غير متوقع في أي خطوة، تراجع عن المعاملة
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An unexpected error occurred during user registration and wallet creation for {Email}. Transaction rolled back.", dto.Email);
                    throw; // أعد رمي الاستثناء ليتم التعامل معه في طبقة الـController
                }
            } // نهاية using transaction
        }
        /*
        // دالة تسجيل المستخدم: CreateAccountAsync
        public async Task<IdentityResult> CreateAccountAsync(RegisterDto dto)
        {
            // <--- استخدام AutoMapper لتحويل RegisterDto إلى User Entity --->
            var user = _mapper.Map<User>(dto);

            // تعيين الخصائص التي لا تُعين من DTO أو يولدها Identity
            // user.Id = Guid.NewGuid().ToString(); // ❌ دعه لـUserManager ليولده تلقائياً
            user.EmailConfirmed = true;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            // User.Wallet Navigation Property تبقى null هنا، ويتم ربطها في Seeder

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                // التحقق من وجود الدور وإضافة المستخدم إليه
                if (!await _roleManager.RoleExistsAsync(dto.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(dto.Role));
                }
                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            return result;
        }*/

        // دالة تسجيل الدخول: SignInAsync
        public async Task<UserDto> SignInAsync(loginDto dto) // نوع الإرجاع هو UserDto
        {
            // جلب المستخدم من قاعدة البيانات
            // <--- هذا السطر ضروري جداً لجلب المحفظة (Wallet) مع المستخدم --->
            var user = await _userManager.Users
                                         .Include(u => u.Wallet)
                                         .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null) return null; // إذا لم يتم العثور على المستخدم

            // التحقق من كلمة المرور وتسجيل الدخول
            var result = await _signInManager.PasswordSignInAsync(user.UserName, dto.Password, dto.RememberMe, lockoutOnFailure: false);
            if (!result.Succeeded) return null; // إذا فشل تسجيل الدخول

            // جلب الدور (بما أن التوكن سيتضمن دور واحد فقط)
            var roles = await _userManager.GetRolesAsync(user);
            string singleRole = roles.FirstOrDefault();

            // توليد التوكن (يتوافق مع توقيع GenerateToken(User user, string role))
            var token = _jwtTokenGenerator.GenerateToken(user, singleRole);

            // <--- استخدام AutoMapper لإنشاء UserDto من User Entity --->
            var userDto = _mapper.Map<UserDto>(user);
            userDto.value = token; // تعيين التوكن في الـUserDto

            return userDto; // إرجاع UserDto
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

    }
}