using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.IServices;
using Domain.Entities;
using Domain.IRepositories;
using AutoMapper; // Add this using directive for AutoMapper


namespace Infrastructure.Services
{
    public class GarageService : IGarageService
    {
        private readonly IRepository<Garage> _garageRepo;
        private readonly IMapper _mapper; // Inject IMapper

        public GarageService(IRepository<Garage> garageRepo, IMapper mapper)
        { 
                _garageRepo = garageRepo;
                _mapper = mapper;

        }

       public async Task<GarageDto> AddGarageAsync(CreateGarageDto createGarageDto)
        {

            var garage = new Garage
            {
                GarageId = Guid.NewGuid(),
                Name = createGarageDto.Name,
                Location = createGarageDto.Location,
                Area = createGarageDto.Area,
                Capacity = createGarageDto.Capacity,
                AvailableSpots = createGarageDto.Capacity,
                PricePerHour = createGarageDto.PricePerHour,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _garageRepo.AddAsync(garage);
            await _garageRepo.SaveChangesAsync();
            // Map the newly created garage entity to GarageDto and return it
           
            // Return the DTO
            return _mapper.Map<GarageDto>(garage);

        }
        public async Task<GarageDto> UpdateGarageAsync(UpdateGarageDto  updateGarageDto)
        {
            var garage = await _garageRepo.GetByIdAsync(updateGarageDto.GarageId);

            if (garage == null)
            {
                // In a service, it's better to throw an exception if the entity isn't found
                // rather than returning null or a default string. The controller will catch it.
                throw new KeyNotFoundException("Garage not found.");
            }

            garage.Name = updateGarageDto.Name;
            garage.Location = updateGarageDto.Location;
            garage.Area = updateGarageDto.Area;
            garage.Capacity = updateGarageDto.Capacity;
            garage.AvailableSpots = updateGarageDto.AvailableSpots;
            garage.PricePerHour = updateGarageDto.PricePerHour;
            garage.UpdatedAt = DateTime.UtcNow;

            _garageRepo.Update(garage);
            await _garageRepo.SaveChangesAsync();
            // Map the updated garage entity to GarageDto and return it
            return _mapper.Map<GarageDto>(garage); // Return the DTO

        }
        public async Task<IEnumerable<GarageDto>> SearchGaragesAsync(string? city = null, int? minAvailableSpots = null, bool? isActive = null)
        {
            var filters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(city))
            {
                filters.Add("Location", city);
            }
            if (isActive.HasValue)
            {
                filters.Add("IsActive", isActive.Value);
            }

            var garages = await _garageRepo.FilterByAsync(filters);

            if (minAvailableSpots.HasValue)
            {
                garages = garages.Where(g => g.AvailableSpots >= minAvailableSpots.Value);
            }

            return garages.Select(g => _mapper.Map<GarageDto>(g)); // Use mapper here
        }

        public async Task<IEnumerable<GarageDto>> GetAllGaragesAsync()
        {
            var garages = await _garageRepo.GetAllAsync();
            return garages.Select(g => _mapper.Map<GarageDto>(g)); // Use mapper here too
        }





        public async Task ToggleGarageStatusAsync(Guid garageId)
        {
            var garage = await _garageRepo.GetByIdAsync(garageId);
            if (garage == null)
                return;

            garage.IsActive = !garage.IsActive;
            garage.UpdatedAt = DateTime.UtcNow;

            _garageRepo.Update(garage);
            await _garageRepo.SaveChangesAsync();
        }
        public async Task<GarageDto> GetGarageByIdAsync(Guid garageId)
        {
            var garage = await _garageRepo.GetByIdAsync(garageId);
            if (garage == null)
            {
                return null; // Return null if not found, or throw KeyNotFoundException
            }
            return _mapper.Map<GarageDto>(garage); // Use mapper
        }
        public async Task DeleteGarageAsync(Guid garageId)
        {
            var garage = await _garageRepo.GetByIdAsync(garageId);
            if (garage == null)
            {
                throw new KeyNotFoundException("Garage not found.");
            }

            _garageRepo.Delete(garage);
            await _garageRepo.SaveChangesAsync();
        }
    }
}
