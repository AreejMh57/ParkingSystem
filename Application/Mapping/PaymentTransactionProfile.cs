using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Application.DTOs;
using Domain.Entities; 

namespace Application.Mapping
{
    public class PaymentTransactionProfile : Profile
    {
        public PaymentTransactionProfile()
        {
            // From Entity to DTO
            CreateMap<PaymentTransaction, PaymentTransactionDto>()
                // Map properties with different names
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TransactionType)) // Map Entity.TransactionType to DTO.Type
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.PaymentStatus)) // Map Entity.PaymentStatus to DTO.Status
                                                                                              // All other properties (PaymentTransactionId, WalletId, BookingId, UserId, Amount, TransactionReference, TransactionDate, CreatedAt, UpdatedAt)
                                                                                              // will be mapped automatically due to matching names and types.
                ;

            // From Create DTO to Entity
            CreateMap<CreatePaymentTransactionDto, PaymentTransaction>()
                .ForMember(dest => dest.TransactionId, opt => opt.Ignore()) // Generated in service
                .ForMember(dest => dest.TransactionDate, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // Set in service
                                                                        // Map properties with different names
                .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => src.Type)) // Map DTO.Type to Entity.TransactionType
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.Status)); // Map DTO.Status to Entity.PaymentStatus
        }
    }

}


