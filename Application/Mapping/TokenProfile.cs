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
            // 1. من Token Entity إلى TokenDto (للعرض)
            CreateMap<Token, TokenDto>()
                // TokenId, Value, ValidFrom, ValidTo, BookingId, CreatedAt, UpdatedAt, UserId, IsUsed
                // سيتم ربطها تلقائيا لأن الأسماء متطابقة في Entity و DTO.
                // هذا السطر يربط UserName في TokenDto من User.UserName في Entity
               // .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
                ;

            // 2. من CreateTokenDto إلى Token Entity (للإنشاء)
            CreateMap<CreateTokenDto, Token>()
                .ForMember(dest => dest.TokenId, opt => opt.Ignore()) // يتم توليد ID في الخدمة
                .ForMember(dest => dest.Value, opt => opt.Ignore()) // يتم توليده في الخدمة
                .ForMember(dest => dest.ValidFrom, opt => opt.MapFrom(src => DateTime.UtcNow)) // يتم تعيين ValidFrom في الخدمة
                .ForMember(dest => dest.ValidTo, opt => opt.MapFrom(src => DateTime.UtcNow.AddMinutes(src.ExpirationMinutes))) // يتم تعيين ValidTo في الخدمة
                .ForMember(dest => dest.IsUsed, opt => opt.Ignore()) // يتم تعيينه لـfalse في الخدمة
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // يتم تعيينها في الخدمة
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // يتم تعيينها في الخدمة
                                                                        // UserId و BookingId سيتم ربطهما تلقائيا
                ;

            // 3. من ValidateTokenDto إلى Token (لأغراض التحقق والبحث)
            // هذا الماب لا يُستخدم عادة لإنشاء Entity، بل للبحث أو لربط القيم
            CreateMap<ValidateBookingTokenDto, Token>()
                // جميع الخصائص التي لا تُعين من DTO أو يولدها Entity
                .ForMember(dest => dest.TokenId, opt => opt.Ignore())
                .ForMember(dest => dest.ValidFrom, opt => opt.Ignore())
                .ForMember(dest => dest.ValidTo, opt => opt.Ignore())
                .ForMember(dest => dest.IsUsed, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                // ربط TokenValue بـEntity.Value
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                // UserId و BookingId سيتم ربطهما تلقائيا
                ;
        }
    }
}