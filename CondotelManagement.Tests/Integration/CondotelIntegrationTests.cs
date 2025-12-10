using CondotelManagement.Data;
using CondotelManagement.DTOs;
using CondotelManagement.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Tests.Integration
{
    public class CondotelIntegrationTests : TestBase
    {
        public CondotelIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetCondotels_ReturnsListOfCondotels()
        {
            // Act
            var response = await Client.GetAsync("/api/tenant/condotel");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<List<CondotelDTO>>();
            result.Should().NotBeNull();
            result!.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetCondotelById_WithValidId_ReturnsCondotel()
        {
            // Act
            var response = await Client.GetAsync("/api/tenant/condotel/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<CondotelDTO>();
            result.Should().NotBeNull();
            result!.CondotelId.Should().Be(1);
            result.Name.Should().Be("Test Condotel");
        }

        [Fact]
        public async Task CreateCondotel_AsHost_CreatesCondotel()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                name = "New Condotel",
                description = "New Condotel Description",
                resortId = 1,
                pricePerNight = 150000,
                beds = 3,
                bathrooms = 2
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/condotel", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var condotel = await DbContext.Condotels
                .FirstOrDefaultAsync(c => c.Name == "New Condotel");
            condotel.Should().NotBeNull();
            condotel!.HostId.Should().Be(1); // Host ID from seed data
        }

        [Fact]
        public async Task UpdateCondotel_AsHost_UpdatesCondotel()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                condotelId = 1,
                name = "Updated Condotel Name",
                description = "Updated Description",
                pricePerNight = 120000
            };

            // Act
            var response = await Client.PutAsJsonAsync("/api/host/condotel/1", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var condotel = await DbContext.Condotels.FindAsync(1);
            condotel!.Name.Should().Be("Updated Condotel Name");
            condotel.PricePerNight.Should().Be(120000);
        }

        [Fact]
        public async Task DeleteCondotel_AsHost_DeletesCondotel()
        {
            // Arrange - Create a condotel to delete
            var condotel = new Condotel
            {
                HostId = 1,
                ResortId = 1,
                Name = "To Delete",
                Description = "Will be deleted",
                PricePerNight = 100000,
                Beds = 1,
                Bathrooms = 1,
                Status = "Active"
            };
            DbContext.Condotels.Add(condotel);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.DeleteAsync($"/api/host/condotel/{condotel.CondotelId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var deletedCondotel = await DbContext.Condotels.FindAsync(condotel.CondotelId);
            deletedCondotel.Should().BeNull();
        }

        [Fact]
        public async Task CreateCondotel_AsTenant_ReturnsForbidden()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            var request = new
            {
                name = "Unauthorized Condotel",
                description = "Should fail",
                resortId = 1,
                pricePerNight = 100000
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/condotel", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}

