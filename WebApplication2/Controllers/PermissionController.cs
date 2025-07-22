// Presentation/Controllers/PermissionController.cs
using Application.DTOs; // لـPermissionDto, CreatePermissionDto, UpdatePermissionDto
using Application.IServices; // لـIPermissionService
using Microsoft.AspNetCore.Authorization; // لـ[Authorize] attribute
using Microsoft.AspNetCore.Mvc; // لـControllerBase, IActionResult, إلخ
using System; // لـGuid
using System.Collections.Generic; // لـIEnumerable
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [ApiController] // يشير إلى أن هذا المتحكم يستجيب لطلبات API الويب
    [Route("api/admin/[controller]")] // مسار خاص بلوحة تحكم الإدارة (مثلاً: /api/admin/Permission)
    [Authorize(Roles = "Admin")] // حماية: فقط المستخدمين ذوو دور Admin يمكنهم الوصول لهذا المتحكم بالكامل
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Creates a new permission definition.
        /// Requires 'permission_create' policy.
        /// </summary>
        /// <param name="dto">Permission creation details.</param>
        /// <returns>The created PermissionDto on success.</returns>
        [HttpPost("add")]
        [Authorize(Policy = "permission_create")] // صلاحية محددة لإنشاء صلاحية
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var newPermission = await _permissionService.CreatePermissionAsync(dto);
                return StatusCode(201, newPermission); // HTTP 201 Created
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message }); // صلاحية بالاسم موجودة
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the permission.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all defined permissions in the system.
        /// Requires 'permission_browse' policy.
        /// </summary>
        /// <returns>A list of PermissionDto.</returns>
        [HttpGet("all")]
        [Authorize(Policy = "permission_browse")] // صلاحية محددة لتصفح الصلاحيات
        public async Task<IActionResult> GetAll()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        /// <summary>
        /// Retrieves a specific permission by ID.
        /// Requires 'permission_browse' policy.
        /// </summary>
        /// <param name="permissionId">The ID of the permission.</param>
        /// <returns>A PermissionDto for the specified permission.</returns>
        [HttpGet("{permissionId}")]
        [Authorize(Policy = "permission_browse")] // صلاحية محددة لتصفح الصلاحيات
        public async Task<IActionResult> GetPermissionById(Guid permissionId)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(permissionId);
            if (permission == null)
            {
                return NotFound(new { Message = $"Permission with ID {permissionId} not found." });
            }
            return Ok(permission);
        }

        /// <summary>
        /// Updates the description of an existing permission.
        /// Requires 'permission_update' policy.
        /// </summary>
        /// <param name="permissionId">The ID of the permission to update.</param>
        /// <param name="dto">DTO containing updated description.</param>
        /// <returns>The updated PermissionDto.</returns>
        [HttpPut("{permissionId}")]
        [Authorize(Policy = "permission_update")] // صلاحية محددة لتعديل الصلاحيات
        public async Task<IActionResult> UpdatePermission(Guid permissionId, [FromBody] UpdatePermissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var updatedPermission = await _permissionService.UpdatePermissionAsync(permissionId, dto);
                return Ok(updatedPermission);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the permission.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a permission definition from the system.
        /// Requires 'permission_delete' policy.
        /// </summary>
        /// <param name="permissionId">The ID of the permission to delete.</param>
        /// <returns>Success message.</returns>
        [HttpDelete("{permissionId}")]
        [Authorize(Policy = "permission_delete")] // صلاحية محددة لحذف الصلاحيات
        public async Task<IActionResult> DeletePermission(Guid permissionId)
        {
            try
            {
                var result = await _permissionService.DeletePermissionAsync(permissionId);
                if (result)
                {
                    return NoContent(); // HTTP 204 No Content for successful deletion
                }
                return BadRequest(new { Message = "Failed to delete permission." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex) // إذا كانت مرتبطة بأدوار
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the permission.", Details = ex.Message });
            }
        }
    }
}