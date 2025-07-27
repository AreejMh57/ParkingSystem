// Infrastructure/Services/TokenService.cs
using Application.DTOs; // لـToken DTOs
using Application.IServices; // لـITokenService
using Domain.Entities; // كيان Token الخاص بك
using Domain.IRepositories; // IRepository
using Microsoft.AspNetCore.Identity; // UserManager, IdentityResult
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore; // لـ.Include() إذا لزم

namespace Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IRepository<Token> _tokenRepo;
        private readonly UserManager<User> _userManager; // للتحقق من UserId
        private readonly IRepository<Booking> _bookingRepo; // للتحقق من BookingId إذا كان موجوداً
        private readonly ILogService _logService;
        private readonly IMapper _mapper;

        // حقن جميع التبعيات
        public TokenService(
            IRepository<Token> tokenRepo,
            UserManager<User> userManager,
            IRepository<Booking> bookingRepo,
            ILogService logService,
            IMapper mapper)
        {
            _tokenRepo = tokenRepo;
            _userManager = userManager;
            _bookingRepo = bookingRepo;
            _logService = logService;
            _mapper = mapper;
        }

        public async Task<TokenDto> CreateCustomTokenAsync(CreateTokenDto dto)
        {
            // 1. التحقق مما إذا كان المستخدم موجوداً
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                await _logService.LogWarningAsync($"Token creation failed: User {dto.UserId} not found.");
                throw new KeyNotFoundException($"User with ID {dto.UserId} not found.");
            }

            // 2. التحقق من وجود BookingId إذا تم توفيره
            if (dto.BookingId==Guid.Empty)
            {
                var booking = await _bookingRepo.GetByIdAsync(dto.BookingId);
                if (booking == null)
                {
                    await _logService.LogWarningAsync($"Token creation failed: Booking {dto.BookingId } not found.");
                    throw new KeyNotFoundException($"Booking with ID {dto.BookingId } not found.");
                }
            }

            // 3. توليد قيمة توكن فريدة
            string tokenValue = Guid.NewGuid().ToString("N"); // N format لإزالة الواصلات

            var token = new Token
            {
                TokenId = Guid.NewGuid(),
                UserId = dto.UserId, // استخدام UserId من الـDTO
                Value = tokenValue,
                ValidFrom = DateTime.UtcNow, // يبدأ الصلاحية من الآن
                ValidTo = DateTime.UtcNow.AddMinutes(dto.ExpirationMinutes), // ينتهي بعد ExpirationMinutes
                BookingId = dto.BookingId, // استخدام BookingId من الـDTO (سيكون null إذا لم يتم توفيره)
                IsUsed = false, // غير مستخدم افتراضياً عند الإنشاء
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow // تحديث تاريخ الإنشاء والتحديث
            };

            await _tokenRepo.AddAsync(token);
            await _tokenRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Token {token.TokenId} created for user {dto.UserId} (BookingId: {dto.BookingId}). Expires at: {token.ValidTo}");

            // 4. تحويل الكيان إلى DTO وإرجاعه
            return _mapper.Map<TokenDto>(token);
        }

        public async Task<IdentityResult> ValidateCustomTokenAsync(ValidateBookingTokenDto dto)
        {
            // 1. البحث عن التوكن بواسطة القيمة ومعرف المستخدم
            var tokens = await _tokenRepo.FilterByAsync(new Dictionary<string, object>
            {
                { "UserId", dto.UserId },
                { "Value", dto.Value }
            });

            var token = tokens.FirstOrDefault();

            if (token == null)
            {
                await _logService.LogWarningAsync($"Token validation failed for user {dto.UserId}: Token not found.");
                return IdentityResult.Failed(new IdentityError { Description = "Invalid token." });
            }

            // 2. التحقق من انتهاء صلاحية التوكن
            if (token.ValidTo < DateTime.UtcNow)
            {
                await _logService.LogWarningAsync($"Token validation failed for user {dto.UserId}: Token {token.TokenId} expired.");
                return IdentityResult.Failed(new IdentityError { Description = "Token has expired." });
            }

            // 3. التحقق مما إذا كان التوكن مستخدماً بالفعل
            if (token.IsUsed)
            {
                await _logService.LogWarningAsync($"Token validation failed for user {dto.UserId}: Token {token.TokenId} already used.");
                return IdentityResult.Failed(new IdentityError { Description = "Token has already been used." });
            }

            // <--- تحديث IsUsed هنا بعد التحقق الناجح --->
            token.IsUsed = true;
            token.UpdatedAt = DateTime.UtcNow;
            _tokenRepo.Update(token);
            await _tokenRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Token {token.TokenId} marked as used after successful validation.");


            // إذا مرت جميع التحققات
            await _logService.LogInfoAsync($"Token {token.TokenId} validated successfully for user {dto.UserId}.");
            return IdentityResult.Success;
        }

        public async Task<IEnumerable<TokenDto>> GetActiveTokensByUserIdAndBookingIdAsync(string userId, Guid? bookingId = null)
        {
            var filters = new Dictionary<string, object>
            {
                { "UserId", userId },
                { "IsUsed", false } // فقط التوكنات غير المستخدمة
            };
            // إضافة فلتر الحجز إذا تم توفيره
            if (bookingId.HasValue)
            {
                filters.Add("BookingId", bookingId.Value);
            }

            var tokens = await _tokenRepo.FilterByAsync(filters);
            // فلترة التوكنات غير المنتهية الصلاحية
            tokens = tokens.Where(t => t.ValidTo > DateTime.UtcNow);

            // يمكن تضمين المستخدم هنا إذا أردت (يتطلب .Include(t => t.User) في Repository)
            return tokens.Select(t => _mapper.Map<TokenDto>(t));
        }

        public async Task<int> CleanupExpiredTokensAsync()
        {
            // هذه الدالة ستُستخدم لتنظيف التوكنات منتهية الصلاحية والمستخدمة
            var tokensToClean = await _tokenRepo.FilterByAsync(new Dictionary<string, object>
            {
                { "IsUsed", true } // التوكنات المستخدمة
            });

            // فلترة التوكنات منتهية الصلاحية
            var expiredTokens = await _tokenRepo.GetAllAsync();
            expiredTokens = expiredTokens.Where(t => t.ValidTo < DateTime.UtcNow).ToList();

            var allTokensToClean = tokensToClean.Union(expiredTokens).Distinct().ToList();

            foreach (var token in allTokensToClean)
            {
                _tokenRepo.Delete(token);
            }
            var deletedCount = allTokensToClean.Count();
            if (deletedCount > 0)
            {
                await _tokenRepo.SaveChangesAsync();
                await _logService.LogInfoAsync($"Cleaned up {deletedCount} expired or used tokens.");
            }
            return deletedCount;
        }
    }
}