
using Domain.Entities;
using Domain.IRepositories;
using Infrastructure;
using Infrastructure.Seeds;
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


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));



builder.Services.AddHttpClient<ILoggingClient, LoggingClient>(client =>
{
    client.BaseAddress = new Uri("https://your-log-api.com/");
});
// ASP.NET Core Identity setup (includes UserManager, SignInManager, RoleManager)
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<Infrastructure.Authentication.IJwtTokenGenerator, Infrastructure.Authentication.JwtTokenGenerator>();

builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAuthService, AuthService>();
// Add services to the container.
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IPaymentTransactionService, PaymentTransactionService>();
builder.Services.AddScoped<IUserService, UserService>();


builder.Services.AddScoped<IGarageService, GarageService>();

builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi                                                                                                                                                          
//builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();  
    app.UseSwaggerUI(); // · ›⁄Ì· Ê«ÃÂ… «·„” Œœ„ «· ›«⁄·Ì… (Swagger UI)
}
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



// Run Seeder after initializing the application
using (var scope = app.Services.CreateScope())

{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await PermissionSeeder.SeedPermissionsAndAssignToRolesAsync(context, roleManager);
}




app.Run();
