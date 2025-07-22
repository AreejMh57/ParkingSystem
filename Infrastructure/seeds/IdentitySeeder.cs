// Infrastructure/Seeds/IdentitySeeder.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Entities; // لاستخدام كيان المستخدم الخاص بك (User) و Wallet
using Microsoft.AspNetCore.Identity; // لاستخدام UserManager و RoleManager و IdentityRole
using Infrastructure.Contexts; // لاستخدام AppDbContext
using Microsoft.EntityFrameworkCore; // لـDatabase.BeginTransactionAsync()

namespace Infrastructure.Seeds
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminUserAsync(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context) // AppDbContext يتم تمريره هنا
        {
            // 1. إنشاء الأدوار الأساسية
            string[] roleNames = { "Admin", "Customer", "ParkingManager" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. إنشاء المستخدم الإداري (admin@parking.com)
            var adminEmail = "admin@parking.com";
            var adminPassword = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // <--- بداية الـTransaction هنا --->
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // <--- الخطوة 1: إنشاء المستخدم وحفظه أولاً --->
                        adminUser = new User
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            EmailConfirmed = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                            // User.WalletId غير موجود هنا، لذا لن يسبب خطأ
                            // User.Wallet Navigation Property لا تُعين بعد هنا
                        };

                        // هذا السطر سينجح الآن لأنه لا يوجد WalletId في User Entity الذي يمكن أن يكون NULL
                        var createResult = await userManager.CreateAsync(adminUser, adminPassword);

                        if (createResult.Succeeded)
                        {
                            await userManager.AddToRoleAsync(adminUser, "Admin");

                            // <--- الخطوة 2: الآن، بعد إنشاء المستخدم وتولد adminUser.Id، ننشئ المحفظة ونربطها --->
                            var adminWallet = new Wallet
                            {
                                WalletId = Guid.NewGuid(),
                                Balance = 0.00M,
                                UserId = adminUser.Id, // <--- هنا يتم تعيين UserId للمحفظة
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                LastUpdated = DateTime.UtcNow
                            };

                            context.Add(adminWallet); // <--- الآن هذا السطر سينجح لأن UserId للمحفظة تم تعيينه
                            await context.SaveChangesAsync(); // حفظ المحفظة ضمن الـTransaction

                            // (اختياري) ربط Navigation Property في User بعد إنشاء المحفظة
                            // (يتطلب جلب المستخدم مرة أخرى أو التأكد من تتبع الكائن)
                            // var userToUpdate = await userManager.FindByIdAsync(adminUser.Id);
                            // if (userToUpdate != null) {
                            //    userToUpdate.Wallet = adminWallet;
                            //    await userManager.UpdateAsync(userToUpdate);
                            // }

                            // <--- إذا وصلت هنا، كل شيء نجح، قم بتأكيد الـTransaction --->
                            await transaction.CommitAsync();
                        }
                        else
                        {
                            // إذا فشل إنشاء المستخدم، تراجع عن الـTransaction
                            await transaction.RollbackAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        // في حال حدوث أي خطأ غير متوقع ضمن الـTransaction
                        await transaction.RollbackAsync();
                        throw; // أعد رمي الاستثناء لتراه في الـOutput
                    }
                }
            }
        }
    }
}