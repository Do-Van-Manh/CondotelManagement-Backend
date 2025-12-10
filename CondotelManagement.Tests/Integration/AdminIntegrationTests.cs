using CondotelManagement.Data;
using CondotelManagement.DTOs.Admin;
using CondotelManagement.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Tests.Integration
{
    public class AdminIntegrationTests : TestBase
    {
        public AdminIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetDashboard_AsAdmin_ReturnsDashboardData()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/admin/dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<DashboardOverviewDto>();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUsers_AsAdmin_ReturnsUserList()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/admin/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<List<UserViewDTO>>();
            result.Should().NotBeNull();
            result!.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateUser_AsAdmin_CreatesUser()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            var request = new AdminCreateUserDTO
            {
                FullName = "New Admin User",
                Email = "newadmin@test.com",
                Password = "Admin123!",
                RoleId = 1,
                Phone = "0123456789"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/admin/users", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
            
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "newadmin@test.com");
            user.Should().NotBeNull();
            user!.FullName.Should().Be("New Admin User");
        }

        [Fact]
        public async Task UpdateUserStatus_AsAdmin_UpdatesStatus()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            var request = new UpdateUserStatusDTO
            {
                UserId = 3,
                Status = "Inactive"
            };

            // Act
            var response = await Client.PutAsJsonAsync("/api/admin/users/3/status", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var user = await DbContext.Users.FindAsync(3);
            user!.Status.Should().Be("Inactive");
        }

        [Fact]
        public async Task GetDashboard_AsNonAdmin_ReturnsForbidden()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/admin/dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}















