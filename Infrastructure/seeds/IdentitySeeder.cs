// Infrastructure/Seeds/IdentitySeeder.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Entities; // To use your custom User and Wallet entities
using Microsoft.AspNetCore.Identity; // To use UserManager, RoleManager, and IdentityRole
using Infrastructure.Contexts; // To use AppDbContext
using Microsoft.EntityFrameworkCore; // For Database.BeginTransactionAsync()

namespace Infrastructure.Seeds
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminUserAsync(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context) // AppDbContext is passed here
        {
            // 1. Create the base roles
            string[] roleNames = { "Admin", "Customer", "ParkingManager" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Create the admin user (admin@parking.com)
            var adminEmail = "admin@parking.com";
            var adminPassword = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // <--- Start of the Transaction here --->
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // <--- Step 1: Create and save the user first --->
                        adminUser = new User
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            EmailConfirmed = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                            // User.WalletId doesn't exist here, so it won't cause an error
                            // User.Wallet Navigation Property is not assigned yet here
                        };

                        // This line will succeed now because there is no WalletId in the User Entity that can be NULL
                        var createResult = await userManager.CreateAsync(adminUser, adminPassword);

                        if (createResult.Succeeded)
                        {
                            await userManager.AddToRoleAsync(adminUser, "Admin");

                            // <--- Step 2: Now, after the user is created and adminUser.Id is generated, we create and link the wallet --->
                            var adminWallet = new Wallet
                            {
                                WalletId = Guid.NewGuid(),
                                Balance = 0.00M,
                                UserId = adminUser.Id, // <--- Here the UserId is assigned to the wallet
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                LastUpdated = DateTime.UtcNow
                            };

                            context.Add(adminWallet); // <--- Now this line will succeed because the UserId for the wallet is assigned
                            await context.SaveChangesAsync(); // Save the wallet within the transaction

                            // (Optional) Link the Navigation Property in User after creating the wallet
                            // (Requires fetching the user again or ensuring the object is being tracked)
                            // var userToUpdate = await userManager.FindByIdAsync(adminUser.Id);
                            // if (userToUpdate != null) {
                            //    userToUpdate.Wallet = adminWallet;
                            //    await userManager.UpdateAsync(userToUpdate);
                            // }

                            // <--- If you reach here, everything succeeded, commit the transaction --->
                            await transaction.CommitAsync();
                        }
                        else
                        {
                            // If user creation fails, roll back the transaction
                            await transaction.RollbackAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        // In case of any unexpected error within the transaction
                        await transaction.RollbackAsync();
                        throw; // Rethrow the exception to see it in the output
                    }
                }
            }
        }
    }
}