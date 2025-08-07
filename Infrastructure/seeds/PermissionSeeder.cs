// Infrastructure/Seeds/PermissionSeeder.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Contexts;
using Infrastructure.seeds.PermissionData; // For PermissionGenerator, ProjectModules, PermissionActions
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // For ToListAsync()

namespace Infrastructure.Seeds
{
    public static class PermissionSeeder
    {
        public static async Task SeedPermissionsAndAssignToRolesAsync(
            AppDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            var permissions = PermissionGenerator.GenerateAll();

            // Save permissions to the DB
            // We get the names that already exist to prevent duplicates
            var existingPermissionNames = await context.Permissions.Select(p => p.Name).ToHashSetAsync();
            var newPermissions = permissions
                .Where(p => !existingPermissionNames.Contains(p.Name))
                .ToList();

            if (newPermissions.Any()) // Add this check to avoid AddRange if it's empty
            {
                context.Permissions.AddRange(newPermissions);
                await context.SaveChangesAsync();
            }

            // <--- The part for creating roles has been removed here (it's done in IdentitySeeder) --->

            // Fetch permissions from the database (after saving them) to ensure we have the IDs
            var dbPermissions = await context.Permissions.ToListAsync();

            // Admin: All permissions except booking_delete
            var adminPermissions = dbPermissions
                .Where(p => p.Name != "BOOKING_DELETE") // Make sure the name is uppercase
                .ToList();
            await AssignPermissionsToRole(roleManager, "Admin", adminPermissions);

            // Customer: Only browse + booking_create + wallet_create
            var customerPermissions = dbPermissions
                .Where(p =>
                    p.Name.EndsWith("_BROWSE") || // Make sure to match the uppercase letters
                    p.Name == "BOOKING_CREATE" ||
                    p.Name == "WALLET_CREATE")
                .ToList();
            await AssignPermissionsToRole(roleManager, "Customer", customerPermissions);

            // ParkingManager: All permissions except booking_delete
            var parkingManagerPermissions = dbPermissions
                .Where(p => p.Name != "BOOKING_DELETE")
                .ToList();
            await AssignPermissionsToRole(roleManager, "ParkingManager", parkingManagerPermissions);
        }

        private static async Task AssignPermissionsToRole(
            RoleManager<IdentityRole> roleManager,
            string roleName,
            List<Permission> permissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                // If the role is not found, log a warning and don't continue
                // Console.WriteLine($"Warning: Role '{roleName}' not found when assigning permissions.");
                return;
            }
            var existingClaims = await roleManager.GetClaimsAsync(role);

            foreach (var permission in permissions)
            {
                // Add the Claim only if it doesn't already exist
                if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission.Name))
                {
                    await roleManager.AddClaimAsync(role, new Claim("Permission", permission.Name));
                }
            }
        }
    }
}