using CondotelManagement.Data;
using CondotelManagement.DTOs;
using CondotelManagement.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Tests.Integration
{
    public class ReviewIntegrationTests : TestBase
    {
        public ReviewIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateReview_WithCompletedBooking_CreatesReview()
        {
            // Arrange - Create completed booking
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-8)),
                Status = "Completed",
                TotalPrice = 200000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            var request = new
            {
                bookingId = booking.BookingId,
                condotelId = 1,
                rating = 5,
                comment = "Great place to stay!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/tenant/review", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var review = await DbContext.Reviews
                .FirstOrDefaultAsync(r => r.BookingId == booking.BookingId);
            review.Should().NotBeNull();
            review!.Rating.Should().Be(5);
            review.Comment.Should().Be("Great place to stay!");
        }

        [Fact]
        public async Task CreateReview_WithPendingBooking_ReturnsBadRequest()
        {
            // Arrange - Create pending booking
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

            var request = new
            {
                bookingId = booking.BookingId,
                condotelId = 1,
                rating = 5,
                comment = "Test review"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/tenant/review", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetReviews_ForCondotel_ReturnsReviews()
        {
            // Arrange - Create reviews
            var reviews = new List<Review>
            {
                new Review
                {
                    BookingId = null,
                    CondotelId = 1,
                    UserId = 3,
                    Rating = 5,
                    Comment = "Excellent!",
                    Status = "Visible",
                    CreatedAt = DateTime.UtcNow
                },
                new Review
                {
                    BookingId = null,
                    CondotelId = 1,
                    UserId = 3,
                    Rating = 4,
                    Comment = "Very good",
                    Status = "Visible",
                    CreatedAt = DateTime.UtcNow
                }
            };
            DbContext.Reviews.AddRange(reviews);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/tenant/condotel/1/reviews");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<List<ReviewDTO>>();
            result.Should().NotBeNull();
            result!.Count.Should().BeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task HostReply_ToReview_UpdatesReview()
        {
            // Arrange - Create review
            var review = new Review
            {
                BookingId = null,
                CondotelId = 1,
                UserId = 3,
                Rating = 4,
                Comment = "Good place",
                Status = "Visible",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Reviews.Add(review);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                reviewId = review.ReviewId,
                reply = "Thank you for your feedback!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/review/reply", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var updatedReview = await DbContext.Reviews.FindAsync(review.ReviewId);
            updatedReview!.Reply.Should().Be("Thank you for your feedback!");
        }
    }
}

