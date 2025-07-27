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

            // <--- إضافة ربط لـSensorStatusReportDto --->
            CreateMap<SensorStatusReportDto, Sensor>()
                .ForMember(dest => dest.SensorId, opt => opt.MapFrom(src => src.SensorId))
                .ForMember(dest => dest.IsOccupied, opt => opt.MapFrom(src => src.IsOccupied))
               // .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.EventTimestamp))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // ليس جزءاً من التقرير
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // يتم تحديثه يدوياً
                .ForMember(dest => dest.SensorType, opt => opt.Ignore()) // ليس جزءاً من التقرير
                .ForMember(dest => dest.AccountStatus, opt => opt.Ignore()) // ليس جزءاً من التقرير
                .ForMember(dest => dest.GarageId, opt => opt.Ignore()) // ليس جزءاً من التقرير
                .ForMember(dest => dest.Garage, opt => opt.Ignore()); // Navigation Property

        }
    }
}
