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
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using CondotelManagement.Services.Interfaces.BookingService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using CondotelManagement.Services.Interfaces.Cloudinary;
using CondotelManagement.Services.CloudinaryService;

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
// 4️⃣ Swagger + CORS
// ============================
builder.Services.AddEndpointsApiExplorer();

// Giữ nguyên cấu hình Swagger để có nút Authorize
builder.Services.AddSwaggerGen(options =>
{
    // Thêm định nghĩa "Authorize" (Security Definition)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Nhập JWT Token: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    // Yêu cầu "Authorize" cho tất cả API
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
builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();


// CORS cho frontend React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3001") // port frontend
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============================
// 5️⃣ Dependency Injection (DI)
// ============================
// Dòng này sẽ gọi file config (và AddAuthentication) một cách chính xác
builder.Services.AddDependencyInjectionConfiguration(builder.Configuration);


// Đăng ký các service và repository của Admin
// GHI CHÚ: Bạn nên dời 2 dòng này vào file DependencyInjectionConfig.cs
// cho gọn, nhưng để đây vẫn chạy được.
//Đăng ký các service và repository của Booking
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
//Đăng ký các service và repository của Condotel
builder.Services.AddScoped<ICondotelRepository, CondotelRepository>();
builder.Services.AddScoped<ICondotelService, CondotelService>();


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

// Authentication + Authorization (Phải giữ 2 dòng này)
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Run App
app.Run();