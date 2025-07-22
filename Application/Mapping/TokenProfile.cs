// Application/Mapping/TokenProfile.cs
using AutoMapper;
using Domain.Entities; // كيان Token الخاص بك
using Application.DTOs; // DTOs التوكن

namespace Application.Mapping
{
    public class TokenProfile : Profile
    {
        public TokenProfile()
        {
            // من Token Entity إلى TokenDto
            CreateMap<Token, TokenDto>();
            // TokenId, Value, ValidFrom, ValidTo, BookingId, CreatedAt, UpdatedAt
            // سيتم ربطها تلقائيا لأن الأسماء متطابقة.

            // من CreateBookingTokenDto إلى Token Entity
            CreateMap<CreateTokenDto, Token>()
                .ForMember(dest => dest.TokenId, opt => opt.Ignore()) // يتم توليد ID في الخدمة
                .ForMember(dest => dest.Value, opt => opt.Ignore()) // يتم توليده في الخدمة
                .ForMember(dest => dest.ValidFrom, opt => opt.MapFrom(src => DateTime.UtcNow)) // تعيين ValidFrom عند الإنشاء
                .ForMember(dest => dest.ValidTo, opt => opt.MapFrom(src => DateTime.UtcNow.AddMinutes(src.ExpirationMinutes))) // حساب ValidTo
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // يتم تعيينها في الخدمة
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // يتم تعيينها في الخدمة
                                                                         // BookingId سيتم ربطه تلقائيا.

            // من ValidateBookingTokenDto إلى Token (عادة لا تستخدم لإنشاء Entity، بل للبحث)
            // هذا الماب يمكن أن يكون فارغاً أو يُستخدم لأغراض البحث فقط
            CreateMap<ValidateBookingTokenDto, Token>()
                .ForMember(dest => dest.TokenId, opt => opt.Ignore())
                .ForMember(dest => dest.ValidFrom, opt => opt.Ignore())
                .ForMember(dest => dest.ValidTo, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value)); // ربط TokenValue بـEntity.Value
        }
    }
}