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
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, NotificationDto>();
              

            CreateMap<CreateNotificationDto, Notification>()
                .ForMember(dest => dest.NotificationId, opt => opt.Ignore()) // Generated in service
                .ForMember(dest => dest.IsRead, opt => opt.Ignore()) // Set to false by default in service
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()); // Set in service
                                                                         // If Notification entity has UpdatedAt, also ignore it here.
        }
    }
}
