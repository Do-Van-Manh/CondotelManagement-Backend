using System.Text;
using System.Text.Json.Serialization;
using CondotelManagement.Configurations;
using CondotelManagement.Data;
using CondotelManagement.Models;
using CondotelManagement.Services.CloudinaryService;
using CondotelManagement.Services.Interfaces.Cloudinary;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ====================== DB ======================
builder.Services.AddDbContext<CondotelDbVer1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// ====================== Controllers + JSON Fix ======================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// ====================== Swagger ======================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Bearer {token}",
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

// Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// CORS cho React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Dependency Injection (gồm Auth, Admin, Booking,...)
builder.Services.AddDependencyInjectionConfiguration(builder.Configuration);

// ====================== Build ======================
var app = builder.Build();

// Always enable Swagger (both Dev & Production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Condotel API v1");
    c.RoutePrefix = "swagger";  // ⚠ FIX 404 trên VPS
});

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
