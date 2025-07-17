using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.IServices;


namespace Application.IServices
{
    public interface IGarageService
    {
        Task<GarageDto> AddGarageAsync(CreateGarageDto CreateGarageDto);
        Task<GarageDto> UpdateGarageAsync(UpdateGarageDto updateGarageDtO);
        Task<IEnumerable<GarageDto>> GetAllGaragesAsync();
        Task<IEnumerable<GarageDto>> SearchGaragesAsync(string? city = null, int? minAvailableSpots = null, bool? isActive = null);
        Task ToggleGarageStatusAsync(Guid garageId);
        Task<GarageDto> GetGarageByIdAsync(Guid garageId);
        Task DeleteGarageAsync(Guid garageId);
    }
}
