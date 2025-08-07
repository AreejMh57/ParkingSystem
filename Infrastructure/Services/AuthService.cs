// Infrastructure/Services/AuthService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // For IConfiguration
using Application.DTOs; // For RegisterDto, UserDto, LoginDto, LoginResponseDto
using Application.IServices; // For IAuthService
using Domain.Entities; // For User Entity
using Microsoft.AspNetCore.Identity; // For UserManager, SignInManager, IdentityResult, RoleManager
using Infrastructure.Authentication; // For IJwtTokenGenerator
using Infrastructure.Contexts;
using AutoMapper; // For IMapper
using Microsoft.EntityFrameworkCore; // For .Include() (very important for fetching Wallet)
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly RoleManager<IdentityRole> _roleManager; // To inject RoleManager
        private readonly IMapper _mapper; // To inject IMapper
        private readonly IConfiguration _configuration; // To inject IConfiguration
        private readonly AppDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly IWalletService _IWalletService;

        // Corrected Constructor: Inject all required dependencies
        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtTokenGenerator jwtTokenGenerator, // Use the interface
            RoleManager<IdentityRole> roleManager, // Inject RoleManager
            IMapper mapper, // Inject IMapper
            IConfiguration configuration,
            AppDbContext context,
            ILogger<AuthService> logger,
            IWalletService walletService) // Inject IConfiguration
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _roleManager = roleManager;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _IWalletService = walletService;
        }

        // User registration function: CreateAccountAsync (with Transaction and Wallet creation)
        public async Task<string?> CreateAccountAsync(RegisterDto dto)
        {
            // <--- Start of the Transaction to ensure atomicity --->
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Create the User Entity object
                    var user = _mapper.Map<User>(dto);
                    user.EmailConfirmed = true;
                    user.CreatedAt = DateTime.UtcNow;
                    user.UpdatedAt = DateTime.UtcNow;

                    // Attempt to create the user in Identity (will generate User.Id)
                    var createResult = await _userManager.CreateAsync(user, dto.Password);

                    if (!createResult.Succeeded)
                    {
                        // If user creation fails, roll back the transaction
                        await transaction.RollbackAsync();
                        _logger.LogWarning("User creation failed for {Email}. Errors: {Errors}", dto.Email, string.Join("; ", createResult.Errors.Select(e => e.Description)));
                        return null; // Or throw a custom exception
                    }

                    // 2. Add the user to the role
                    if (!await _roleManager.RoleExistsAsync(dto.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(dto.Role));
                    }
                    await _userManager.AddToRoleAsync(user, dto.Role);

                    // 4. Create a new wallet for the user
                    var newWallet = new Wallet
                    {
                        WalletId = Guid.NewGuid(), // A new ID will be generated for the wallet
                        Balance = 0.00M, // <--- Initial balance is zero as requested
                        UserId = user.Id, // <--- Assign the UserId to the wallet from the created user
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    };

                    // Link the wallet to the user (in the Navigation Property) - optional, but good practice
                    user.Wallet = newWallet;

                    // Save the wallet to the database (within the same Transaction)
                    _context.Add(newWallet);
                    await _context.SaveChangesAsync(); // <--- Save the wallet here

                    // 5. Generate a token for the user
                    var roles = await _userManager.GetRolesAsync(user);
                    string singleRole = roles.FirstOrDefault();
                    var token = _jwtTokenGenerator.GenerateToken(user, singleRole);

                    // <--- Commit the Transaction after all steps have succeeded --->
                    await transaction.CommitAsync();
                    _logger.LogInformation("User {UserId} ({Email}) registered successfully with wallet {WalletId} and token generated.", user.Id, dto.Email, newWallet.WalletId);

                    // Return a string containing the required information
                    return token;
                }
                catch (Exception ex)
                {
                    // In case of any unexpected error in any step, roll back the transaction
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An unexpected error occurred during user registration and wallet creation for {Email}. Transaction rolled back.", dto.Email);
                    throw; // Rethrow the exception to be handled in the Controller layer
                }
            } // End of using transaction
        }

        /*
        // User registration function: CreateAccountAsync
        public async Task<IdentityResult> CreateAccountAsync(RegisterDto dto)
        {
            // <--- Use AutoMapper to convert RegisterDto to User Entity --->
            var user = _mapper.Map<User>(dto);

            // Set properties not assigned from the DTO or generated by Identity
            // user.Id = Guid.NewGuid().ToString(); // ❌ Let UserManager generate it automatically
            user.EmailConfirmed = true;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            // User.Wallet Navigation Property remains null here, and is linked in the Seeder

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                // Check if the role exists and add the user to it
                if (!await _roleManager.RoleExistsAsync(dto.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(dto.Role));
                }
                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            return result;
        }*/

        // Login function: SignInAsync
        public async Task<UserDto> SignInAsync(loginDto dto) // Return type is UserDto
        {
            // Fetch the user from the database
            // <--- This line is very important to fetch the Wallet with the user --->
            var user = await _userManager.Users
                                         .Include(u => u.Wallet)
                                         .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null) return null; // If user is not found

            // Check the password and sign in
            var result = await _signInManager.PasswordSignInAsync(user.UserName, dto.Password, dto.RememberMe, lockoutOnFailure: false);
            if (!result.Succeeded) return null; // If sign-in failed

            // Fetch the role (since the token will only include one role)
            var roles = await _userManager.GetRolesAsync(user);
            string singleRole = roles.FirstOrDefault();

            // Generate the token (matches the signature of GenerateToken(User user, string role))
            var token = _jwtTokenGenerator.GenerateToken(user, singleRole);

            // <--- Use AutoMapper to create UserDto from User Entity --->
            var userDto = _mapper.Map<UserDto>(user);
            userDto.value = token; // Set the token in the UserDto

            return userDto; // Return the UserDto
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}