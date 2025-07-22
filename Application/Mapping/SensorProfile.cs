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
    public class SensorProfile : Profile
    {
        public SensorProfile()
        {
            // من Sensor Entity إلى SensorDto
            CreateMap<Sensor, SensorDto>();
               

     
            CreateMap<CreateSensorDto, Sensor>()
                .ForMember(dest => dest.SensorId, opt => opt.Ignore()) 
                .ForMember(dest => dest.AccountStatus, opt => opt.Ignore()) 
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) 
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); 
                                                                         

            // من UpdateSensorDto إلى Sensor Entity
            CreateMap<UpdateSensorDto, Sensor>()
                .ForMember(dest => dest.SensorId, opt => opt.Ignore()) 
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) 
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}
