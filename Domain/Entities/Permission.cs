using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class Permission
    {

        public Guid PermissionId { get; set; }
        public string Name { get; set; }
        public List<IdentityRole> Roles { get; set; }
        public string Description { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; }
    



    }
}
