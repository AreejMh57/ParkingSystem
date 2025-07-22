// Infrastructure/Services/TokenService.cs
using Application.DTOs;
using Application.IServices;
using Domain.Entities; 
using Domain.IRepositories; // IRepository
using Microsoft.AspNetCore.Identity; // IdentityResult
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IRepository<Token> _tokenRepo;
        private readonly IRepository<Booking> _bookingRepo; // للتحقق من BookingId
        private readonly ILogService _logService;
        private readonly IMapper _mapper;

        // حقن جميع التبعيات
        public TokenService(
            IRepository<Token> tokenRepo,
            IRepository<Booking> bookingRepo,
            ILogService logService,
            IMapper mapper)
        {
            _tokenRepo = tokenRepo;
            _bookingRepo = bookingRepo;
            _logService = logService;
            _mapper = mapper;
        }

        public async Task<TokenDto> CreateBookingTokenAsync(CreateTokenDto dto)
        {
            // 1. التحقق مما إذا كان BookingId موجوداً
            var booking = await _bookingRepo.GetByIdAsync(dto.BookingId);
            if (booking == null)
            {
                await _logService.LogWarningAsync($"Token creation failed: Booking {dto.BookingId} not found.");
                throw new KeyNotFoundException($"Booking with ID {dto.BookingId} not found.");
            }

            // 2. توليد قيمة توكن فريدة
            string tokenValue = Guid.NewGuid().ToString("N"); // N format removes hyphens

            var token = new Token
            {
                TokenId = Guid.NewGuid(),
                Value = tokenValue,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddMinutes(dto.ExpirationMinutes),
                BookingId = dto.BookingId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _tokenRepo.AddAsync(token);
            await _tokenRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Token {token.TokenId} created for booking {dto.BookingId}. Expires at: {token.ValidTo}");

            // 3. تحويل الكيان إلى DTO وإرجاعه
            return _mapper.Map<TokenDto>(token);
        }

        public async Task<IdentityResult> ValidateBookingTokenAsync(ValidateBookingTokenDto dto)
        {
            // 1. البحث عن التوكن بواسطة القيمة ومعرف الحجز
            var tokens = await _tokenRepo.FilterByAsync(new Dictionary<string, object>
            {
                { "BookingId", dto.BookingId },
                { "Value", dto.Value }
            });

            var token = tokens.FirstOrDefault();

            if (token == null)
            {
                await _logService.LogWarningAsync($"Token validation failed for booking {dto.BookingId}: Token not found.");
                return IdentityResult.Failed(new IdentityError { Description = "Invalid token." });
            }

            // 2. التحقق من انتهاء صلاحية التوكن
            if (token.ValidTo < DateTime.UtcNow)
            {
                await _logService.LogWarningAsync($"Token validation failed for booking {dto.BookingId}: Token {token.TokenId} expired.");
                return IdentityResult.Failed(new IdentityError { Description = "Token has expired." });
            }

            // <--- لا يمكن التحقق من 'IsUsed' هنا لأن الخاصية غير موجودة في الكيان --->
            // <--- لا يمكن ربط التوكن بـ'UserId' أو 'Purpose' لأنهما غير موجودين أيضاً --->

            // إذا مرت جميع التحققات
            await _logService.LogInfoAsync($"Token {token.TokenId} validated successfully for booking {dto.BookingId}.");
            return IdentityResult.Success;
        }

        public async Task<IEnumerable<TokenDto>> GetBookingTokensAsync(Guid bookingId)
        {
            // 1. جلب التوكنات للحجز المحدد
            var tokens = await _tokenRepo.FilterByAsync(new Dictionary<string, object>
            {
                { "BookingId", bookingId }
            });
            // اختياري: فلترة التوكنات غير المنتهية الصلاحية
            tokens = tokens.Where(t => t.ValidTo > DateTime.UtcNow);

            // 2. تحويل الكيانات إلى DTOs
            return tokens.Select(t => _mapper.Map<TokenDto>(t));
        }

        public async Task<int> CleanupExpiredTokensAsync()
        {
            // هذه الدالة ستُستخدم لتنظيف التوكنات منتهية الصلاحية
            var expiredTokens = await _tokenRepo.FilterByAsync(new Dictionary<string, object>
            {
                { "ValidTo", DateTime.UtcNow } // يمكن استخدام هذا للفلترة على التاريخ
            });

            // هذا الفلتر قد يكون أكثر دقة إذا كان FilterByAsync لا يدعم مقارنة "<"
            expiredTokens = expiredTokens.Where(t => t.ValidTo < DateTime.UtcNow).ToList();

            foreach (var token in expiredTokens)
            {
                _tokenRepo.Delete(token);
            }
            var deletedCount = expiredTokens.Count();
            if (deletedCount > 0)
            {
                await _tokenRepo.SaveChangesAsync();
                await _logService.LogInfoAsync($"Cleaned up {deletedCount} expired tokens.");
            }
            return deletedCount;
        }
    }
}
