// Application/Mapping/AuthProfile.cs
using AutoMapper;
using Application.DTOs; // لـRegisterDto, LoginDto, UserDto, LoginResponseDto
using Domain.Entities; // لـUser Entity و Wallet Entity
using Microsoft.EntityFrameworkCore; // لـInclude() في المستقبل إذا كان الـmapper يستخدم queryable

namespace Application.Mapping
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            // 1. ربط RegisterDto بـUser Entity
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)) // UserName من Email (يتطلب إعدادات Program.cs)
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // UserManager هو من يشفر
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Identity يولد الـId
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore()) // تعيين في الخدمة
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // تعيين في الخدمة
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // تعيين في الخدمة

                .ForMember(dest => dest.Wallet, opt => opt.Ignore()) // Navigation property (لا تُعين مباشرة من DTO)
                .ForMember(dest => dest.Bookings, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.Notifications, opt => opt.Ignore()); // Navigation property

            // 2. ربط User Entity بـUserDto
            CreateMap<User, UserDto>()
                // Id, UserName, Email, PhoneNumber, CreatedAt, UpdatedAt سيتم ربطها تلقائياً بالاسم
                // <--- ربط WalletId من خلال Navigation Property --->
                // هذا يتطلب أن تكون خاصية Wallet محملة (eager loaded) عند جلب المستخدم
              //  .ForMember(dest => dest.WalletId, opt => opt.MapFrom(src => src.Wallet != null ? src.Wallet.WalletId : (Guid?)null))
            // تأكد أن UserDto لديه خاصية WalletId من نوع Guid?

             .ForMember(dest => dest.Token, opt => opt.Ignore());
        }
    }  
    }
