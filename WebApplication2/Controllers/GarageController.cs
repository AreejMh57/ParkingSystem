using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.IServices;
using Application.DTOs; // Make sure your DTOs are in this namespace or adjust accordingly

// <--- إضافة هذا الـusing --->
using Microsoft.AspNetCore.Authorization;

namespace YourProjectName.Controllers // Replace YourProjectName with your actual project's namespace for controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // <--- يمكن وضع حماية عامة هنا للمتحكم إذا كانت جميع دواله تتطلب دور Admin --->
    // [Authorize(Roles = "Admin")] // مثال: إذا كان كل شيء في هذا المتحكم خاص بالـAdmin
    public class GarageController : ControllerBase
    {
        private readonly IGarageService _garageService;

        public GarageController(IGarageService garageService)
        {
            _garageService = garageService;
        }

        /// <summary>
        /// Adds a new garage.
        /// </summary>
        /// <param name="createGarageDto">The garage creation data.</param>
        /// <returns>The newly created garage.</returns>
        [HttpPost("add")] // أضفت مسار Add هنا لتمييزه
        [ProducesResponseType(typeof(GarageDto), 201)]
        [ProducesResponseType(400)]
        [Authorize(Policy = "garage_create")] // <--- تم إضافة حماية: تتطلب صلاحية 'garage_create'
        public async Task<IActionResult> AddGarage([FromBody] CreateGarageDto createGarageDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var garage = await _garageService.AddGarageAsync(createGarageDto);
                return CreatedAtAction(nameof(GetGarageById), new { garageId = garage.GarageId }, garage);
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logger)
                return StatusCode(500, "An error occurred while adding the garage.");
            }
        }

        /// <summary>
        /// Updates an existing garage.
        /// </summary>
        /// <param name="updateGarageDto">The garage update data.</param>
        /// <returns>The updated garage.</returns>
        [HttpPut("update")] // أضفت مسار Update هنا لتمييزه
        [ProducesResponseType(typeof(GarageDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [Authorize(Policy = "garage_update")] // <--- تم إضافة حماية: تتطلب صلاحية 'garage_update'
        public async Task<IActionResult> UpdateGarage([FromBody] UpdateGarageDto updateGarageDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var garage = await _garageService.UpdateGarageAsync(updateGarageDto);
                return Ok(garage);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the garage.");
            }
        }

        /// <summary>
        /// Searches for garages based on criteria.
        /// </summary>
        /// <param name="city">Filter by city (location).</param>
        /// <param name="minAvailableSpots">Filter by minimum available spots.</param>
        /// <param name="isActive">Filter by active status.</param>
        /// <returns>A list of matching garages.</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<GarageDto>), 200)]
        [Authorize(Policy = "garage_browse")] // <--- تم إضافة حماية: تتطلب صلاحية 'garage_browse'
        public async Task<IActionResult> SearchGarages([FromQuery] string? city = null, [FromQuery] int? minAvailableSpots = null, [FromQuery] bool? isActive = null)
        {
            try
            {
                var garages = await _garageService.SearchGaragesAsync(city, minAvailableSpots, isActive);
                return Ok(garages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while searching for garages.");
            }
        }

        /// <summary>
        /// Gets all garages.
        /// </summary>
        /// <returns>A list of all garages.</returns>
        [HttpGet("all")] // أضفت مسار All هنا لتمييزه
        [ProducesResponseType(typeof(IEnumerable<GarageDto>), 200)]
        [Authorize(Policy = "garage_browse")] // <--- تم إضافة حماية: تتطلب صلاحية 'garage_browse'
        public async Task<IActionResult> GetAllGarages()
        {
            try
            {
                var garages = await _garageService.GetAllGaragesAsync();
                return Ok(garages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving all garages.");
            }
        }

        /// <summary>
        /// Toggles the active status of a garage.
        /// </summary>
        /// <param name="garageId">The ID of the garage to toggle status.</param>
        [HttpPut("{garageId}/toggle-status")]
        [ProducesResponseType(204)] // No Content for successful update
        [ProducesResponseType(404)]
        [Authorize(Policy = "garage_toggle")] // <--- تم إضافة حماية: تتطلب صلاحية 'garage_toggle'
        public async Task<IActionResult> ToggleGarageStatus(Guid garageId)
        {
            try
            {
                await _garageService.ToggleGarageStatusAsync(garageId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while toggling garage status.");
            }
        }

        /// <summary>
        /// Gets a garage by its ID.
        /// </summary>
        /// <param name="garageId">The ID of the garage.</param>
        /// <returns>The garage with the specified ID.</returns>
        [HttpGet("{garageId}")]
        [ProducesResponseType(typeof(GarageDto), 200)]
        [ProducesResponseType(404)]
        [Authorize(Policy = "garage_browse")] // <--- تم إضافة حماية: تتطلب صلاحية 'garage_browse'
        public async Task<IActionResult> GetGarageById(Guid garageId)
        {
            try
            {
                var garage = await _garageService.GetGarageByIdAsync(garageId);
                if (garage == null)
                {
                    return NotFound($"Garage with ID {garageId} not found.");
                }
                return Ok(garage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the garage.");
            }
        }

        /// <summary>
        /// Deletes a garage by its ID.
        /// </summary>
        /// <param name="garageId">The ID of the garage to delete.</param>
        [HttpDelete("{garageId}")]
        [ProducesResponseType(204)] // No Content for successful deletion
        [ProducesResponseType(404)]
        [Authorize(Policy = "garage_delete")] // <--- تم إضافة حماية: تتطلب صلاحية 'garage_delete'
        public async Task<IActionResult> DeleteGarage(Guid garageId)
        {
            try
            {
                await _garageService.DeleteGarageAsync(garageId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the garage.");
            }
        }
    }
}