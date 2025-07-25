
using Domain.Entities;
using Domain.IRepositories;
using Infrastructure;
using Infrastructure.Seeds;
//using Infrastructure.Seeds.PermissionData;
using Infrastructure.Contexts;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Authentication;
using Application.External;
using Infrastructure.ExternalClients;
using Application.IServices;
using Infrastructure.Services;

using AutoMapper;
using System;
using Microsoft.OpenApi.Models;
using System.Text;
using Infrastructure.seeds.PermissionData;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));



builder.Services.AddHttpClient<ILoggingClient, LoggingClient>(client =>
{
    client.BaseAddress = new Uri("https://your-log-api.com/");
});
// ASP.NET Core Identity setup (includes UserManager, SignInManager, RoleManager)
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    // ��� ���� ��������� ������� ������ PermissionGenerator
    // ���� �� PermissionGenerator.GenerateAll() ���� ������� ����� ����� (����� WALLET_CREATE)
    var allPermissions = PermissionGenerator.GenerateAll();

    // ����� �� ������ ������ (Policy) �� ���� �������
    foreach (var permission in allPermissions)
    {
        options.AddPolicy(permission.Name, policy =>
        {
            policy.RequireClaim("Permission", permission.Name);
        });
    }
});


builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<Infrastructure.Authentication.IJwtTokenGenerator, Infrastructure.Authentication.JwtTokenGenerator>();

//builder.Services.AddScoped<JwtTokenGenerator>();


builder.Services.AddScoped<IAuthService, AuthService>();

// Add services to the container.
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IPaymentTransactionService, PaymentTransactionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISensorService, SensorService>();


builder.Services.AddScoped<IGarageService, GarageService>();
builder.Services.AddScoped<INotificationService,NotificationService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("https://localhost:7169") 
                            .AllowAnyHeader()
                            .AllowAnyMethod());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi                                                                                                                                                          
builder.Services.AddOpenApi();


var app = builder.Build();
app.UseHttpsRedirection();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();  
    app.UseSwaggerUI(); // ������ ����� �������� ��������� (Swagger UI)
}
app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



// Run Seeder after initializing the application
using (var scope = app.Services.CreateScope())

{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<User>>();


  await IdentitySeeder.SeedRolesAndAdminUserAsync(userManager, roleManager, context);

    await PermissionSeeder.SeedPermissionsAndAssignToRolesAsync(context, roleManager);
}





app.Run();
