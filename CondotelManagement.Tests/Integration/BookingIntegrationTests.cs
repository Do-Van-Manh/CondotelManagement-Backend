using CondotelManagement.Data;
using CondotelManagement.DTOs;
using CondotelManagement.DTOs.Booking;
using CondotelManagement.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Tests.Integration
{
    public class BookingIntegrationTests : TestBase
    {
        public BookingIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateBooking_WithValidData_CreatesBooking()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            var request = new BookingDTO
            {
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(3))
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/booking", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            // Verify booking was created
            var booking = await DbContext.Bookings.FirstOrDefaultAsync(b => b.CondotelId == 1);
            booking.Should().NotBeNull();
            booking!.Status.Should().Be("Pending");
            booking.CustomerId.Should().Be(3);
        }

        [Fact]
        public async Task CreateBooking_WithPastDate_ReturnsBadRequest()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            var request = new BookingDTO
            {
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), // Past date
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1))
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/booking", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            
            var result = await DeserializeResponse<ServiceResultDTO>(response);
            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
        }

        [Fact]
        public async Task CreateBooking_WithOverlappingDates_ReturnsBadRequest()
        {
            // Arrange - Create existing booking
            var existingBooking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(4)),
                Status = "Confirmed",
                TotalPrice = 200000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(existingBooking);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            var request = new BookingDTO
            {
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(3)) // Overlaps with existing
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/booking", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CheckAvailability_WithAvailableDates_ReturnsTrue()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            var checkIn = DateOnly.FromDateTime(DateTime.Now.AddDays(10));
            var checkOut = DateOnly.FromDateTime(DateTime.Now.AddDays(12));

            // Act
            var response = await Client.GetAsync($"/api/booking/check-availability?condotelId=1&checkIn={checkIn:yyyy-MM-dd}&checkOut={checkOut:yyyy-MM-dd}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetMyBookings_ReturnsUserBookings()
        {
            // Arrange - Create some bookings
            var bookings = new List<Booking>
            {
                new Booking
                {
                    CondotelId = 1,
                    CustomerId = 3,
                    StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                    EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-3)),
                    Status = "Completed",
                    TotalPrice = 200000,
                    CreatedAt = DateTime.UtcNow
                },
                new Booking
                {
                    CondotelId = 1,
                    CustomerId = 3,
                    StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                    EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                    Status = "Confirmed",
                    TotalPrice = 200000,
                    CreatedAt = DateTime.UtcNow
                }
            };
            DbContext.Bookings.AddRange(bookings);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/booking/my");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<List<BookingDTO>>();
            result.Should().NotBeNull();
            result!.Count.Should().Be(2);
        }

        [Fact]
        public async Task CancelBooking_WithValidBooking_CancelsBooking()
        {
            // Arrange - Create a booking
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                Status = "Confirmed",
                TotalPrice = 200000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.DeleteAsync($"/api/booking/{booking.BookingId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
            
            var updatedBooking = await DbContext.Bookings.FindAsync(booking.BookingId);
            updatedBooking!.Status.Should().Be("Cancelled");
        }

        [Fact]
        public async Task CancelBooking_WithOtherUserBooking_ReturnsForbidden()
        {
            // Arrange - Create booking for different user
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 1, // Different customer
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                Status = "Confirmed",
                TotalPrice = 200000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.DeleteAsync($"/api/booking/{booking.BookingId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateBooking_WithPromotion_AppliesDiscount()
        {
            // Arrange - Create promotion
            var promotion = new Promotion
            {
                CondotelId = 1,
                Name = "Test Promotion",
                DiscountPercentage = 20,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                Status = "Active"
            };
            DbContext.Promotions.Add(promotion);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            var request = new BookingDTO
            {
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                PromotionId = promotion.PromotionId
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/booking", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
            
            var booking = await DbContext.Bookings
                .FirstOrDefaultAsync(b => b.CondotelId == 1 && b.PromotionId == promotion.PromotionId);
            booking.Should().NotBeNull();
            // Price should be 200000 * 0.8 = 160000 (20% discount)
            booking!.TotalPrice.Should().Be(160000);
        }
    }
}

