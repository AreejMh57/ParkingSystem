
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

using System.Text;
using Infrastructure.seeds.PermissionData;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.WebHost.UseUrls("https://localhost:7169", "http://localhost:5152");

//builder.WebHost.UseUrls("http://0.0.0.0:5000", "https://0.0.0.0:5001");

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
    // Ì„ﬂ‰ﬂ  ⁄œÌ· ≈⁄œ«œ«  ﬂ·„… «·„—Ê— Â‰« Õ”» „ ÿ·»« ﬂ
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    // ... ≈⁄œ«œ«  √Œ—Ï
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login"; // «·„”«— ≈·Ï ’›Õ…  ”ÃÌ· «·œŒÊ· (≈–« ·„ Ìﬂ‰ «·„” Œœ„ „’«œﬁ«)
    options.AccessDeniedPath = "/Auth/AccessDenied"; // «·„”«— ≈·Ï ’›Õ… «·Ê’Ê· «·„—›Ê÷ (≈–« ﬂ«‰ „’«œﬁ« Ê·ﬂ‰ ·« Ì„·ﬂ ’·«ÕÌ…)
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // „œ… ’·«ÕÌ… «·ﬂÊﬂÌ
    options.SlidingExpiration = true; // · ÃœÌœ ’·«ÕÌ… «·ﬂÊﬂÌ ⁄‰œ ﬂ· ÿ·» ≈–« „— ‰’› Êﬁ  «·’·«ÕÌ…
});


builder.Services.AddAuthorization(options =>
{
    // Ã·» Ã„Ì⁄ «·’·«ÕÌ«  «·„Ê·œ… »Ê«”ÿ… PermissionGenerator
    //  √ﬂœ √‰ PermissionGenerator.GenerateAll() ÌÊ·œ «·√”„«¡ »√Õ—› ﬂ»Ì—… („À·« WALLET_CREATE)
    var allPermissions = PermissionGenerator.GenerateAll();

    // ≈÷«›… ﬂ· ’·«ÕÌ… ﬂ”Ì«”… (Policy) ›Ì ‰Ÿ«„ «· —ŒÌ’
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
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

//builder.Services.AddCors(options =>
//{
//  options.AddPolicy("AllowFlutter", policy =>
//{
//  policy.WithOrigins(
//      "http://localhost",       // ·· ÿÊÌ— «·„Õ·Ì
//       "http://192.168.96.230"// „À«·: IP ÃÂ«“ Flutter
//   "http://<Public_IP>"     // ≈–« ﬂ«‰ «·Ê’Ê· ⁄»— «·≈‰ —‰ 
// )

//AllowAnyHeader()
//.AllowAnyMethod();
//  });
//});
//builder.Services.AddCors(options =>
//{
// options.AddPolicy("AllowSpecificOrigin",
//   builder => builder.WithOrigins("https://localhost:7169") 
//                     .AllowAnyHeader()
//                    .AllowAnyMethod());
//});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi                                                                                                                                                          
//builder.Services.AddOpenApi();


var app = builder.Build();
app.UseCors("AllowFlutter");
app.UseHttpsRedirection();
// Configure the HTTP request pipeline.


///if (app.Environment.IsDevelopment())
//{
    //app.UseSwagger();
  //  app.UseSwaggerUI(); // · ›⁄Ì· Ê«ÃÂ… «·„” Œœ„ «· ›«⁄·Ì… (Swagger UI)
//}
app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();

//app.MapControllers();
//builder.Services.AddControllersWithViews();
//app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);


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
