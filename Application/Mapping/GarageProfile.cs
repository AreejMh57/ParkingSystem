// Application/Mapping/GarageProfile.cs
using AutoMapper;
using Domain.Entities; // لكيان Garage
using Application.DTOs; // لـGarageDto, CreateGarageDto, UpdateGarageDto

namespace Application.Mapping
{
    public class GarageProfile : Profile
    {
        public GarageProfile()
        {
            // 1. من Garage Entity إلى GarageDto (للعرض)
            CreateMap<Garage, GarageDto>()
                // GarageId, Name, Location, Area, Capacity, AvailableSpots, PricePerHour, IsActive, CreatedAt, UpdatedAt
                // هذه الخصائص سيتم ربطها تلقائياً بالاسم إذا كانت متطابقة في Entity و DTO.
                // إذا كان لديك Latitude/Longitude في DTO/Entity ولم تُظهرها، أضف ForMember لها
                // .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
                // .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude))
                .ForMember(dest => dest.Distance, opt => opt.Ignore()); // Distance سيتم حسابه في الخدمة، لذا تجاهله هنا
            ;

            ;

            // 2. من CreateGarageDto إلى Garage Entity (للإنشاء)
            CreateMap<CreateGarageDto, Garage>()
                .ForMember(dest => dest.GarageId, opt => opt.Ignore()) // يتم توليد ID في الخدمة
                .ForMember(dest => dest.AvailableSpots, opt => opt.MapFrom(src => src.Capacity)) // عند الإنشاء، الأماكن المتاحة = السعة
                .ForMember(dest => dest.IsActive, opt => opt.Ignore()) // يتم تعيينها في الخدمة (true)
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // يتم تعيينها في الخدمة
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // يتم تعيينها في الخدمة
                                                                        // Name, Location, Area, Capacity, PricePerHour سيتم ربطها تلقائياً
                ;

            // 3. من UpdateGarageDto إلى Garage Entity (للتعديل)
            CreateMap<UpdateGarageDto, Garage>()
                .ForMember(dest => dest.GarageId, opt => opt.Ignore()) // ID يُستخدم للبحث، لا يُحدث من الـDTO
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // يُعين عند الإنشاء
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // يُحدث في منطق الخدمة
                                                                        // Name, Location, Area, Capacity, AvailableSpots, PricePerHour, IsActive سيتم ربطها تلقائياً
                ;
        }
    }
}