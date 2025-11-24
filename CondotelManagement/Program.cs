using System.Text;
using System.Text.Json.Serialization;
using CondotelManagement.Configurations;
using CondotelManagement.Data;
using CondotelManagement.Hub;
using CondotelManagement.Models;
using CondotelManagement.Services.CloudinaryService;
using CondotelManagement.Services.Interfaces.Cloudinary;
using Microsoft.AspNetCore.Http.Connections;
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
        // Nếu dòng này không có, BE mặc định mong đợi camelCase.
        //options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        // 3. THÊM DÒNG NÀY ĐỂ FIX LỖI CRASH (StackOverflow)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 🚨 BẮT ĐẦU KHỐI FIX LỖI 400 VALIDATION
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    // Tắt hành vi tự động xử lý lỗi validation của ASP.NET Core (khiến lỗi bị generic)
    options.SuppressModelStateInvalidFilter = true;

    // Định nghĩa hàm xử lý lỗi validation tùy chỉnh
    options.InvalidModelStateResponseFactory = context =>
    {
        // Trả về một đối tượng ProblemDetails chứa chi tiết lỗi
        var problemDetails = new Microsoft.AspNetCore.Mvc.ValidationProblemDetails(context.ModelState)
        {
            // Tùy chỉnh trạng thái phản hồi
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Detail = "Please check the 'errors' property for details."
        };

        // Quan trọng: Gán lỗi Model State vào thuộc tính 'errors' của ProblemDetails
        // Frontend sẽ đọc thuộc tính này
        problemDetails.Extensions["errors"] = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(problemDetails)
        {
            ContentTypes = { "application/problem+json", "application/json" }
        };
    };
});
// 🚨 KẾT THÚC KHỐI FIX LỖI 400 VALIDATION

// ============================
// 4️⃣ Swagger + CORS
// ============================
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
app.MapHub<ChatHub>("/hubs/chat", options =>
{
    options.Transports = HttpTransportType.WebSockets;
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
