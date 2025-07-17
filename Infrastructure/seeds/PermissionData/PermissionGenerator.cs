using System;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Infrastructure.seeds.PermissionData
{
   
    public static class PermissionGenerator
    {
        public static List<Permission> GenerateAll()
        {
            var permissions = new List<Permission>();

            foreach (var module in ProjectModules.Modules)
            {
                foreach (var action in PermissionActions.Actions)
                {
                    var name = $"{module}_{action}";

                    permissions.Add(new Permission
                    {
                        PermissionId = Guid.NewGuid(),
                        Name = name,
                        Description = $"Allows user to {action.ToUpper()} on {module.ToUpper()}"
                    });
                }
            }

            return permissions;
        }
    }

}
