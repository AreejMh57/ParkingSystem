using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.IServices;
using AutoMapper;
using Domain.Entities;
using Domain.IRepositories;

namespace Infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IRepository<Permission> _permissionRepo;
        private readonly ILogService _logService;
        private readonly IMapper _mapper;
        // You might also need IRepository<RolePermission> to check before deletion
        private readonly IRepository<RolePermission> _rolePermissionRepo;

        public PermissionService(
            IRepository<Permission> permissionRepo,
            ILogService logService,
            IMapper mapper,
            IRepository<RolePermission> rolePermissionRepo) // Inject IRepository<RolePermission>
        {
            _permissionRepo = permissionRepo;
            _logService = logService;
            _mapper = mapper;
            _rolePermissionRepo = rolePermissionRepo;
        }

        public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto dto)
        {
            // 1. Check if a permission with the same name already exists
            var existingPermissions = await _permissionRepo.FilterByAsync(new Dictionary<string, object> { { "Name", dto.Name } });
            if (existingPermissions.Any())
            {
                await _logService.LogWarningAsync($"Permission creation failed: Permission with name '{dto.Name}' already exists.");
                throw new InvalidOperationException($"Permission with name '{dto.Name}' already exists.");
            }

            // 2. Create the permission
            var permission = new Permission
            {
                PermissionId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description
            };

            await _permissionRepo.AddAsync(permission);
            await _permissionRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Permission '{permission.Name}' (ID: {permission.PermissionId}) created successfully.");

            return _mapper.Map<PermissionDto>(permission);
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _permissionRepo.GetAllAsync();
            return permissions.Select(p => _mapper.Map<PermissionDto>(p));
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(Guid permissionId)
        {
            var permission = await _permissionRepo.GetByIdAsync(permissionId);
            if (permission == null)
            {
                return null; // Or throw KeyNotFoundException if you prefer consistent exceptions
            }
            return _mapper.Map<PermissionDto>(permission);
        }

        public async Task<PermissionDto> UpdatePermissionAsync(Guid permissionId, UpdatePermissionDto dto)
        {
            var permission = await _permissionRepo.GetByIdAsync(permissionId);
            if (permission == null)
            {
                await _logService.LogWarningAsync($"Permission update failed: Permission with ID {permissionId} not found.");
                throw new KeyNotFoundException($"Permission with ID {permissionId} not found.");
            }

            // Update description if provided (Name should ideally not be updatable via this method as it's the unique key)
            permission.Description = dto.Description ?? permission.Description;
            // If you need to enforce that description is provided:
            // if (!string.IsNullOrEmpty(dto.Description)) { permission.Description = dto.Description; }

            _permissionRepo.Update(permission);
            await _permissionRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Permission '{permission.Name}' (ID: {permissionId}) updated successfully.");

            return _mapper.Map<PermissionDto>(permission);
        }

        public async Task<bool> DeletePermissionAsync(Guid permissionId)
        {
            var permission = await _permissionRepo.GetByIdAsync(permissionId);
            if (permission == null)
            {
                await _logService.LogWarningAsync($"Permission deletion failed: Permission with ID {permissionId} not found.");
                throw new KeyNotFoundException($"Permission with ID {permissionId} not found.");
            }

            // 1. Check if the permission is still assigned to any role
            var existingRolePermissions = await _rolePermissionRepo.FilterByAsync(new Dictionary<string, object> { { "PermissionId", permissionId } });
            if (existingRolePermissions.Any())
            {
                await _logService.LogWarningAsync($"Permission deletion failed: Permission '{permission.Name}' (ID: {permissionId}) is still assigned to roles.");
                throw new InvalidOperationException($"Permission '{permission.Name}' is still assigned to roles and cannot be deleted. Please unassign it first.");
            }

            // 2. Delete the permission
            _permissionRepo.Delete(permission);
            await _permissionRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Permission '{permission.Name}' (ID: {permissionId}) deleted successfully.");

            return true;
        }
    }
}
                                                   
        
