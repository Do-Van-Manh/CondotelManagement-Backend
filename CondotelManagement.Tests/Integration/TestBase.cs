using CondotelManagement.Data;
using CondotelManagement.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace CondotelManagement.Tests.Integration
{
    // Make Program accessible for testing
    public partial class Program { }

    public class TestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        protected readonly WebApplicationFactory<Program> Factory;
        protected readonly HttpClient Client;
        protected readonly CondotelDbVer1Context DbContext;
        protected readonly IServiceScope ServiceScope;

        public TestBase(WebApplicationFactory<Program> factory)
        {
            Factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real database
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<CondotelDbVer1Context>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database
                    services.AddDbContext<CondotelDbVer1Context>(options =>
                    {
                        options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                    });

                    // Mock external services
                    services.AddScoped<CondotelManagement.Services.Interfaces.Shared.IEmailService, MockEmailService>();
                    services.AddScoped<CondotelManagement.Services.Interfaces.Cloudinary.ICloudinaryService, MockCloudinaryService>();
                });
            });

            Client = Factory.CreateClient();
            ServiceScope = Factory.Services.CreateScope();
            DbContext = ServiceScope.ServiceProvider.GetRequiredService<CondotelDbVer1Context>();
            
            // Seed test data
            SeedTestData();
        }

        protected virtual void SeedTestData()
        {
            // Seed Roles
            var roles = new List<Role>
            {
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Host" },
                new Role { RoleId = 3, RoleName = "Tenant" }
            };
            DbContext.Roles.AddRange(roles);

            // Seed Users
            var users = new List<User>
            {
                new User
                {
                    UserId = 1,
                    FullName = "Admin User",
                    Email = "admin@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    RoleId = 1,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = 2,
                    FullName = "Host User",
                    Email = "host@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Host123!"),
                    RoleId = 2,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = 3,
                    FullName = "Tenant User",
                    Email = "tenant@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tenant123!"),
                    RoleId = 3,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                }
            };
            DbContext.Users.AddRange(users);

            // Seed Host
            var host = new Host
            {
                HostId = 1,
                UserId = 2,
                CompanyName = "Test Host Company",
                Status = "Active"
            };
            DbContext.Hosts.Add(host);

            // Seed Location
            var location = new Location
            {
                LocationId = 1,
                Name = "Test Location",
                Description = "Test Description"
            };
            DbContext.Locations.Add(location);

            // Seed Resort
            var resort = new Resort
            {
                ResortId = 1,
                Name = "Test Resort",
                Description = "Test Resort Description",
                LocationId = 1
            };
            DbContext.Resorts.Add(resort);

            // Seed Condotel
            var condotel = new Condotel
            {
                CondotelId = 1,
                HostId = 1,
                ResortId = 1,
                Name = "Test Condotel",
                Description = "Test Condotel Description",
                PricePerNight = 100000,
                Beds = 2,
                Bathrooms = 1,
                Status = "Active"
            };
            DbContext.Condotels.Add(condotel);

            DbContext.SaveChanges();
        }

        protected string GenerateJwtToken(int userId, string email, string role)
        {
            var key = Encoding.UTF8.GetBytes("your-super-secret-key-that-is-long-enough-for-hmacsha256");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = "https://yourdomain.com",
                Audience = "https://yourdomain.com",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        protected void SetAuthHeader(string token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        protected async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            ServiceScope?.Dispose();
            Client?.Dispose();
        }
    }

    // Mock Email Service
    public class MockEmailService : CondotelManagement.Services.Interfaces.Shared.IEmailService
    {
        public List<string> SentEmails { get; } = new List<string>();
        public List<string> SentOtps { get; } = new List<string>();

        public Task SendVerificationOtpAsync(string email, string otp)
        {
            SentEmails.Add(email);
            SentOtps.Add(otp);
            return Task.CompletedTask;
        }

        public Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            SentEmails.Add(email);
            return Task.CompletedTask;
        }

        public Task SendPasswordResetOtpAsync(string email, string otp)
        {
            SentEmails.Add(email);
            SentOtps.Add(otp);
            return Task.CompletedTask;
        }

        public Task SendRefundConfirmationEmailAsync(string toEmail, string customerName, int bookingId, decimal refundAmount, string? bankCode = null, string? accountNumber = null)
        {
            SentEmails.Add(toEmail);
            return Task.CompletedTask;
        }

        public Task SendPayoutConfirmationEmailAsync(string toEmail, string hostName, int bookingId, string condotelName, decimal amount, DateTime paidAt, string? bankName = null, string? accountNumber = null, string? accountHolderName = null)
        {
            SentEmails.Add(toEmail);
            return Task.CompletedTask;
        }
    }

    // Mock Cloudinary Service
    public class MockCloudinaryService : CondotelManagement.Services.Interfaces.Cloudinary.ICloudinaryService
    {
        public Task<string> UploadImageAsync(Stream imageStream, string fileName)
        {
            return Task.FromResult($"https://mock-cloudinary.com/{fileName}");
        }

        public Task<bool> DeleteImageAsync(string publicId)
        {
            return Task.FromResult(true);
        }
    }
}

