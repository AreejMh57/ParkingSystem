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
    public class PermissionProfile : Profile
    {
        public PermissionProfile()
        {
            // From Permission Entity to PermissionDto
            CreateMap<Permission, PermissionDto>();
            // Id, Name, Description will map automatically if names match.

            // From CreatePermissionDto to Permission Entity
            CreateMap<CreatePermissionDto, Permission>()
                .ForMember(dest => dest.PermissionId, opt => opt.Ignore()); // ID is generated in service
                                                                  // Name, Description will map automatically.

            // From UpdatePermissionDto to Permission Entity
            CreateMap< UpdatePermissionDto, Permission>()
                .ForMember(dest => dest.PermissionId, opt => opt.Ignore()) // ID is used for lookup, not mapped from DTO
                .ForMember(dest => dest.Name, opt => opt.Ignore()); // Name is generally not updatable via this DTO
                                                                    // Description will map automatically.
        }
    }

}
