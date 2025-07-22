using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.IServices
{
    public interface IPermissionService
    {

        // Creates a new permission definition. (Usually via Admin Panel)

        Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto dto);

  
        // Retrieves all defined permissions in the system.
      
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();

        // Retrieves a single permission definition by its ID.
   
        Task<PermissionDto?> GetPermissionByIdAsync(Guid permissionId);

     
        /// Updates the description of an existing permission.
        /// </summary>>
        Task<PermissionDto> UpdatePermissionAsync(Guid permissionId, UpdatePermissionDto dto);

  
        /// Deletes a permission definition from the system. (Admin-level operation)
        
  
        Task<bool> DeletePermissionAsync(Guid permissionId);
    }
}
