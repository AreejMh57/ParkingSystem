using Application.DTOs;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Dashboard.Controllers
{
    [Authorize] // تأمين المتحكم
    public class GarageController : Controller
    {
        private readonly IGarageService _garageService;
        private readonly ILogger<GarageController> _logger;

        public GarageController(IGarageService garageService, ILogger<GarageController> logger)
        {
            _garageService = garageService;
            _logger = logger;
        }

        // GET: /Garage
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Admin accessed the Garage management page.");

            // استخدام التابع GetAllGaragesAsync() الذي قدمته
            var garages = await _garageService.GetAllGaragesAsync();

            return View(garages);
        }

        // GET: /Garage/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Garage/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGarageDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _garageService.AddGarageAsync(dto);
                    _logger.LogInformation("New garage '{GarageName}' was created successfully.", dto.Name);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating new garage: {Message}", ex.Message);
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the garage.");
                }
            }
            return View(dto);
        }

        // GET: /Garage/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var garage = await _garageService.GetGarageByIdAsync(id);
            if (garage == null)
            {
                _logger.LogWarning("Attempted to edit garage with ID '{GarageId}', but it was not found.", id);
                return NotFound();
            }

            // استخدام AutoMapper لتحويل GarageDto إلى UpdateGarageDto
            // أو إنشاء view model مخصص لصفحة التعديل
            var updateDto = new UpdateGarageDto
            {
                GarageId = garage.GarageId,
                Name = garage.Name,
              //  Location = garage.Location,
                Area = garage.Area,
                Capacity = garage.Capacity,
                AvailableSpots = garage.AvailableSpots,
                PricePerHour = garage.PricePerHour,
               // IsActive = garage.IsActive
            };

            return View(updateDto);
        }

        // POST: /Garage/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateGarageDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _garageService.UpdateGarageAsync(dto);
                    _logger.LogInformation("Garage ID '{GarageId}' was updated successfully.", dto.GarageId);
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    _logger.LogWarning("Failed to update garage with ID '{GarageId}'. Garage not found.", dto.GarageId);
                    return NotFound();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating garage with ID '{GarageId}': {Message}", dto.GarageId, ex.Message);
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the garage.");
                }
            }
            return View(dto);
        }

        // POST: /Garage/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _garageService.DeleteGarageAsync(id);
                _logger.LogInformation("Garage with ID '{GarageId}' was deleted successfully.", id);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Attempted to delete garage with ID '{GarageId}', but it was not found.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting garage with ID '{GarageId}': {Message}", id, ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Garage/ToggleStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            try
            {
                await _garageService.ToggleGarageStatusAsync(id);
                _logger.LogInformation("Toggled status for garage with ID '{GarageId}'.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for garage with ID '{GarageId}': {Message}", id, ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}