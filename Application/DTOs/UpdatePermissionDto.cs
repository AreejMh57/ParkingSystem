using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
   public class UpdatePermissionDto
    {

        public Guid PermissionId { get; set; }
        public string Name { get; set; }
        
        public string Description { get; set; }
    }
}
