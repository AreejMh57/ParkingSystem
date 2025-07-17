using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreateRolePermissionDto
    {
        public Guid RoleId { get; set; }

        public Guid PermissionId { get; set; }
    }
}
