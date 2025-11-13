using System.Text;
using System.Text.Json.Serialization; // 1. BẠN CẦN THÊM USING NÀY
using CondotelManagement.Configurations;
using CondotelManagement.Data;
using CondotelManagement.Models;
using CondotelManagement.Repositories;
using CondotelManagement.Repositories.Implementations.Admin;
using CondotelManagement.Repositories.Implementations.Auth;
using CondotelManagement.Repositories.Interfaces.Admin;
using CondotelManagement.Repositories.Interfaces.Auth;
using CondotelManagement.Services;
using CondotelManagement.Services.CloudinaryService;
using CondotelManagement.Services.Implementations.Admin;
using CondotelManagement.Services.Implementations.Auth;
using CondotelManagement.Services.Interfaces.Admin;
using CondotelManagement.Services.Interfaces.Auth;
using CondotelManagement.Services.Interfaces.BookingService;
using CondotelManagement.Services.Interfaces.Cloudinary;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 2. XÓA 2 DÒNG THỪA NÀY (Vì chúng đã có trong DependencyInjectionConfig.cs)
// builder.Services.AddScoped<IAuthRepository, AuthRepository>();
// builder.Services.AddScoped<IAuthService, AuthService>();


// ============================
// 1️⃣ Database Configuration
// ============================
builder.Services.AddDbContext<CondotelDbVer1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// ============================
// 2️⃣ Controller + SỬA LỖI JSON CRASH
// ============================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enum -> string thay vì số
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        // 3. THÊM DÒNG NÀY ĐỂ FIX LỖI CRASH (StackOverflow)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });


// ============================
// 4️⃣ Swagger + CORS
// ============================
builder.Services.AddEndpointsApiExplorer();

// Giữ nguyên cấu hình Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Nhập JWT Token: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
// dang ki cloudinary
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();


// CORS cho frontend React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3001", "http://localhost:3000") // port frontend
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============================
// 5️⃣ Dependency Injection (DI)
// ============================
// Dòng này sẽ gọi file config (và AddAuthentication) một cách chính xác
// (Nó đã chứa IAuthService, IAuthRepository, IProfileService, ...)
builder.Services.AddDependencyInjectionConfiguration(builder.Configuration);


// ============================
// 6️⃣ Build & Middleware
// ============================
var app = builder.Build();

// (Static files, bạn có thể bỏ comment nếu cần)
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

// Authentication + Authorization (Phải giữ 2 dòng này)
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Run App
app.Run();