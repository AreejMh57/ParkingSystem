using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Contexts;
using Infrastructure.seeds.PermissionData;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Seeds
{
    public static class PermissionSeeder
    {
        public static async Task SeedPermissionsAndAssignToRolesAsync(
            AppDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            var permissions = PermissionGenerator.GenerateAll();

            // save permission in DB
            var existingPermissionNames = context.Permissions.Select(p => p.Name).ToHashSet();
            var newPermissions = permissions
                .Where(p => !existingPermissionNames.Contains(p.Name))
                .ToList();

            context.Permissions.AddRange(newPermissions);
            await context.SaveChangesAsync();

            // defintion Roles
            var roles = new[] { "Admin", "Customer", "ParkingManager" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // Admin: all permission without booking_delete
            var adminPermissions = permissions
                .Where(p => p.Name != "booking_delete")
                .ToList();
            await AssignPermissionsToRole(roleManager, "Admin", adminPermissions);

            //  Customer: only browse + booking_create + wallet_create
            var customerPermissions = permissions
                .Where(p =>
                    p.Name.EndsWith("_browse") ||
                    p.Name == "booking_create" ||
                    p.Name == "wallet_create")
                .ToList();
            await AssignPermissionsToRole(roleManager, "Customer", customerPermissions);

            //  ParkingManager: all permission without booking_delete
            var parkingManagerPermissions = permissions
                .Where(p => p.Name != "booking_delete")
                .ToList();
            await AssignPermissionsToRole(roleManager, "ParkingManager", parkingManagerPermissions);
        }

        private static async Task AssignPermissionsToRole(
            RoleManager<IdentityRole> roleManager,
            string roleName,
            List<Permission> permissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            var existingClaims = await roleManager.GetClaimsAsync(role);

            foreach (var permission in permissions)
            {
                if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission.Name))
                {
                    await roleManager.AddClaimAsync(role, new Claim("Permission", permission.Name));
                }
            }
        }
    }
}
