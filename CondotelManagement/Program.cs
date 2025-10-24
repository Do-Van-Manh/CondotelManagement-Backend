using System.Text;
using System.Text.Json.Serialization;
using CondotelManagement.Configurations;
using CondotelManagement.Data;
using CondotelManagement.Models;
using CondotelManagement.Repositories;
using CondotelManagement.Repositories.Implementations.Admin;
using CondotelManagement.Repositories.Interfaces.Admin;
using CondotelManagement.Services;
using CondotelManagement.Services.Implementations.Admin;
using CondotelManagement.Services.Interfaces.Admin;
using CondotelManagement.Services.Interfaces.BookingService;

//using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ============================
// 1️⃣ Database Configuration
// ============================
builder.Services.AddDbContext<CondotelDbVer1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// ============================
// 2️⃣ Controller + JSON Enum Convert
// ============================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enum -> string thay vì số
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ============================
// 3️⃣ JWT Authentication Configuration
// ============================
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//        };
//    });

// ============================
// 4️⃣ Swagger + CORS
// ============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS cho frontend React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // port frontend
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============================
// 5️⃣ Dependency Injection (DI)
// ============================
builder.Services.AddDependencyInjectionConfiguration(builder.Configuration);


// Đăng ký các service và repository của Admin
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
//Đăng ký các service và repository của Booking
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();


// (sau này bạn có thể thêm các service khác tại đây)
// builder.Services.AddScoped<IHostService, HostService>();
// builder.Services.AddScoped<ITenantService, TenantService>();

// ============================
// 6️⃣ Build & Middleware
// ============================
var app = builder.Build();

// Static files (nếu có upload hình ảnh)
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(
//        Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
//    RequestPath = "/uploads"
//});

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// CORS + HTTPS
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

// Authentication + Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Run App
app.Run();
