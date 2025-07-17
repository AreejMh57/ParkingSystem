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
public class BookingProfile : Profile
{
    public BookingProfile()
    {
            CreateMap<Booking, BookingDto>();
            CreateMap<CreateBookingDto, Booking>()
            .ForMember(dest => dest.BookingId, opt => opt.Ignore()) // Generated in Service
            .ForMember(dest => dest.TotalPrice, opt => opt.Ignore()) // Calculated in Service
            .ForMember(dest => dest.BookingStatus, opt => opt.Ignore()) // Set in Service
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Set in Service
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // Set in Service
            .ForMember(dest => dest.User, opt => opt.Ignore()) // Navigation property, not mapped from DTO
            .ForMember(dest => dest.Garage, opt => opt.Ignore()); // Navigation property, not mapped from DTO
    }
}
}
