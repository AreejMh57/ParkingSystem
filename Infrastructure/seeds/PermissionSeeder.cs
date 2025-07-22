// Infrastructure/Seeds/PermissionSeeder.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Contexts;
using Infrastructure.seeds.PermissionData; // لـPermissionGenerator, ProjectModules, PermissionActions
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // لـToListAsync()

namespace Infrastructure.Seeds
{
    public static class PermissionSeeder
    {
        public static async Task SeedPermissionsAndAssignToRolesAsync(
            AppDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            var permissions = PermissionGenerator.GenerateAll();

            // حفظ الصلاحيات في DB
            // نحصل على الأسماء الموجودة بالفعل لمنع التكرار
            var existingPermissionNames = await context.Permissions.Select(p => p.Name).ToHashSetAsync();
            var newPermissions = permissions
                .Where(p => !existingPermissionNames.Contains(p.Name))
                .ToList();

            if (newPermissions.Any()) // أضف هذا التحقق لتجنب AddRange إذا كانت فارغة
            {
                context.Permissions.AddRange(newPermissions);
                await context.SaveChangesAsync();
            }

            // <--- تم حذف جزء إنشاء الأدوار هنا (يتم في IdentitySeeder) --->

            // جلب الصلاحيات من قاعدة البيانات (بعد حفظها) لضمان الحصول على IDs
            var dbPermissions = await context.Permissions.ToListAsync();

            // Admin: جميع الصلاحيات باستثناء booking_delete
            var adminPermissions = dbPermissions
                .Where(p => p.Name != "BOOKING_DELETE") // تأكد أن الاسم كبير
                .ToList();
            await AssignPermissionsToRole(roleManager, "Admin", adminPermissions);

            // Customer: فقط browse + booking_create + wallet_create
            var customerPermissions = dbPermissions
                .Where(p =>
                    p.Name.EndsWith("_BROWSE") || // تأكد من مطابقة الأحرف الكبيرة
                    p.Name == "BOOKING_CREATE" ||
                    p.Name == "WALLET_CREATE")
                .ToList();
            await AssignPermissionsToRole(roleManager, "Customer", customerPermissions);

            // ParkingManager: جميع الصلاحيات باستثناء booking_delete
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
                // إذا لم يتم العثور على الدور، سجل تحذيراً ولا تكمل
                // Console.WriteLine($"Warning: Role '{roleName}' not found when assigning permissions.");
                return;
            }
            var existingClaims = await roleManager.GetClaimsAsync(role);

            foreach (var permission in permissions)
            {
                // أضف الـClaim فقط إذا لم يكن موجوداً بالفعل
                if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission.Name))
                {
                    await roleManager.AddClaimAsync(role, new Claim("Permission", permission.Name));
                }
            }
        }
    }
}