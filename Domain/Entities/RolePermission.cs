using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    
    public class RolePermission 
    {

        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }

        public Permission Permission { get; set; }

        public IdentityRole Role { get; set; }




    }
}
