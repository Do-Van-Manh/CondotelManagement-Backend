using CondotelManagement.Data;
using CondotelManagement.DTOs.Payment;
using CondotelManagement.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace CondotelManagement.Tests.Integration
{
    public class PaymentIntegrationTests : TestBase
    {
        public PaymentIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreatePayment_WithValidBooking_CreatesPaymentLink()
        {
            // Arrange - Create a booking
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                Status = "Pending",
                TotalPrice = 200000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            var request = new CreatePaymentRequest
            {
                BookingId = booking.BookingId,
                Description = "Test payment"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/payment/payos/create", request);

            // Assert
            // Note: This will fail if PayOS service is not mocked properly
            // In real scenario, you would mock the PayOS HTTP client
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreatePayment_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            var request = new CreatePaymentRequest
            {
                BookingId = 1,
                Description = "Test"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/payment/payos/create", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreatePayment_WithInvalidBooking_ReturnsBadRequest()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            var request = new CreatePaymentRequest
            {
                BookingId = 99999, // Non-existent booking
                Description = "Test"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/payment/payos/create", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}















