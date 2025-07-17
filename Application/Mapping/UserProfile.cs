using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>()
              
                .ForMember(dest => dest.WalletId, opt => opt.MapFrom(src => src.WalletId));
            // Add other specific mappings if property names differ or logic is needed

            CreateMap<UpdateUserDto, User>()
                // Id is used for lookup, not mapped directly for update
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                // Other properties will map by name (UserName, Email, PhoneNumber, Address)
                .ForMember(dest => dest.WalletId, opt => opt.Ignore()) // WalletId isn't updated via User DTO normally
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Set on creation
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // Set in service logic
        }
    
}
}
