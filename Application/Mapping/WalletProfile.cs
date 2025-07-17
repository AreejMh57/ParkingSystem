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
    public class WalletProfile : Profile
    {
        public WalletProfile()
        {
            CreateMap<Wallet, WalletDto>();
            // If you use CreateWalletDto to create a Wallet entity, add this:
            CreateMap<CreateWalletDto, Wallet>()
                .ForMember(dest => dest.WalletId, opt => opt.Ignore()) // WalletId is generated
                .ForMember(dest => dest.LastUpdated, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // Set in service
            // If you use UpdateWalletDto, add this:
            // CreateMap<UpdateWalletDto, Wallet>(); 
        }
    }
}
