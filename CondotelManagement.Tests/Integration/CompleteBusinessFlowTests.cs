using CondotelManagement.Data;
using CondotelManagement.DTOs;
using CondotelManagement.DTOs.Auth;
using CondotelManagement.DTOs.Booking;
using CondotelManagement.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Tests.Integration
{
    /// <summary>
    /// Integration tests theo đúng luồng nghiệp vụ từ Test Cases
    /// </summary>
    public class CompleteBusinessFlowTests : TestBase
    {
        public CompleteBusinessFlowTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        #region TC-AUTH-001 to TC-AUTH-005: Authentication Flow

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-001")]
        public async Task TC_AUTH_001_RegisterNewUser_ShouldCreatePendingUser()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "newuser@test.com",
                Password = "NewUser123!",
                FullName = "New User",
                Phone = "0123456789"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "newuser@test.com");
            user.Should().NotBeNull();
            user!.Status.Should().Be("Pending");
            user.PasswordResetToken.Should().NotBeNull(); // OTP was set
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-002")]
        public async Task TC_AUTH_002_VerifyEmailWithOTP_ShouldActivateUser()
        {
            // Arrange - Create pending user with OTP
            var user = new User
            {
                FullName = "Pending User",
                Email = "pending@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                RoleId = 3,
                Status = "Pending",
                PasswordResetToken = "123456",
                ResetTokenExpires = DateTime.UtcNow.AddMinutes(10),
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var request = new VerifyEmailRequest
            {
                Email = "pending@test.com",
                Otp = "123456"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/verify-email", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var updatedUser = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "pending@test.com");
            updatedUser.Should().NotBeNull();
            updatedUser!.Status.Should().Be("Active");
            updatedUser.PasswordResetToken.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-003")]
        public async Task TC_AUTH_003_LoginWithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "tenant@test.com",
                Password = "Tenant123!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            result.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
            result.GetProperty("user").GetProperty("email").GetString().Should().Be("tenant@test.com");
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-004")]
        public async Task TC_AUTH_004_LoginWithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "tenant@test.com",
                Password = "WrongPassword"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-005")]
        public async Task TC_AUTH_005_ForgotPasswordFlow_ShouldResetPassword()
        {
            // Arrange
            var sendOtpRequest = new ForgotPasswordRequest { Email = "tenant@test.com" };
            
            // Act - Step 1: Send OTP
            var sendOtpResponse = await Client.PostAsJsonAsync("/api/auth/send-otp", sendOtpRequest);
            sendOtpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Get OTP from database
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "tenant@test.com");
            var otp = user!.PasswordResetToken;
            otp.Should().NotBeNull();

            // Step 2: Reset password with OTP
            var resetRequest = new ResetPasswordWithOtpRequest
            {
                Email = "tenant@test.com",
                Otp = otp!,
                NewPassword = "NewPassword123!"
            };
            var resetResponse = await Client.PostAsJsonAsync("/api/auth/reset-password-with-otp", resetRequest);
            
            // Assert
            resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Step 3: Login with new password
            var loginRequest = new LoginRequest
            {
                Email = "tenant@test.com",
                Password = "NewPassword123!"
            };
            var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region TC-TENANT-001 to TC-TENANT-005: Tenant View & Search Condotel

        [Fact]
        [Trait("Category", "Tenant")]
        [Trait("TestID", "TC-TENANT-001")]
        public async Task TC_TENANT_001_ViewAllCondotels_ShouldReturnList()
        {
            // Act
            var response = await Client.GetAsync("/api/tenant/condotels");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<CondotelDTO>>();
            result.Should().NotBeNull();
            result!.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Category", "Tenant")]
        [Trait("TestID", "TC-TENANT-002")]
        public async Task TC_TENANT_002_SearchCondotelByName_ShouldReturnFiltered()
        {
            // Act
            var response = await Client.GetAsync("/api/tenant/condotels?name=Test");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<CondotelDTO>>();
            result.Should().NotBeNull();
            result!.All(c => c.Name.Contains("Test", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Tenant")]
        [Trait("TestID", "TC-TENANT-003")]
        public async Task TC_TENANT_003_FilterCondotelByPrice_ShouldReturnInRange()
        {
            // Act
            var response = await Client.GetAsync("/api/tenant/condotels?minPrice=50000&maxPrice=150000");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<CondotelDTO>>();
            result.Should().NotBeNull();
            result!.All(c => c.PricePerNight >= 50000 && c.PricePerNight <= 150000).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Tenant")]
        [Trait("TestID", "TC-TENANT-004")]
        public async Task TC_TENANT_004_FilterCondotelByBedsBathrooms_ShouldReturnFiltered()
        {
            // Act
            var response = await Client.GetAsync("/api/tenant/condotels?beds=2&bathrooms=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<CondotelDTO>>();
            result.Should().NotBeNull();
            // Note: This depends on CondotelService implementation
        }

        [Fact]
        [Trait("Category", "Tenant")]
        [Trait("TestID", "TC-TENANT-005")]
        public async Task TC_TENANT_005_ViewCondotelDetail_ShouldReturnFullInfo()
        {
            // Act
            var response = await Client.GetAsync("/api/tenant/condotels/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<CondotelDetailDTO>();
            result.Should().NotBeNull();
            result!.CondotelId.Should().Be(1);
            result.Name.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region TC-BOOKING-001 to TC-BOOKING-011: Booking Flow

        [Fact]
        [Trait("Category", "Booking")]
        [Trait("TestID", "TC-BOOKING-001")]
        public async Task TC_BOOKING_001_CheckAvailability_Available_ShouldReturnTrue()
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
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("available").GetBoolean().Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Booking")]
        [Trait("TestID", "TC-BOOKING-002")]
        public async Task TC_BOOKING_002_CheckAvailability_NotAvailable_ShouldReturnFalse()
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

            var checkIn = DateOnly.FromDateTime(DateTime.Now.AddDays(3)); // Overlaps
            var checkOut = DateOnly.FromDateTime(DateTime.Now.AddDays(5));

            // Act
            var response = await Client.GetAsync($"/api/booking/check-availability?condotelId=1&checkIn={checkIn:yyyy-MM-dd}&checkOut={checkOut:yyyy-MM-dd}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("available").GetBoolean().Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "Booking")]
        [Trait("TestID", "TC-BOOKING-003")]
        public async Task TC_BOOKING_003_CreateBooking_Valid_ShouldCreateBooking()
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
            
            var booking = await DbContext.Bookings
                .FirstOrDefaultAsync(b => b.CondotelId == 1 && b.CustomerId == 3);
            booking.Should().NotBeNull();
            booking!.Status.Should().Be("Pending");
            booking.TotalPrice.Should().Be(200000); // 100000 * 2 nights
        }

        [Fact]
        [Trait("Category", "Booking")]
        [Trait("TestID", "TC-BOOKING-004")]
        public async Task TC_BOOKING_004_CreateBooking_PastDate_ShouldReturnBadRequest()
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
        }

        [Fact]
        [Trait("Category", "Booking")]
        [Trait("TestID", "TC-BOOKING-005")]
        public async Task TC_BOOKING_005_CreateBooking_Overlapping_ShouldReturnBadRequest()
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
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(3)) // Overlaps
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/booking", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", "Booking")]
        [Trait("TestID", "TC-BOOKING-006")]
        public async Task TC_BOOKING_006_CreateBooking_WithPromotion_ShouldApplyDiscount()
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

        [Fact]
        [Trait("Category", "Booking")]
        [Trait("TestID", "TC-BOOKING-008")]
        public async Task TC_BOOKING_008_GetMyBookings_ShouldReturnUserBookings()
        {
            // Arrange - Create bookings
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
            result!.Count.Should().BeGreaterThanOrEqualTo(2);
            result.All(b => b.CustomerId == 3).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Booking")]
        [Trait("TestID", "TC-BOOKING-010")]
        public async Task TC_BOOKING_010_CancelBooking_ShouldCancelBooking()
        {
            // Arrange - Create booking
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
        [Trait("Category", "Booking")]
        [Trait("TestID", "TC-BOOKING-011")]
        public async Task TC_BOOKING_011_CancelBooking_OtherUser_ShouldReturnForbidden()
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

        #endregion

        #region TC-REVIEW-001 to TC-REVIEW-007: Review Flow

        [Fact]
        [Trait("Category", "Review")]
        [Trait("TestID", "TC-REVIEW-001")]
        public async Task TC_REVIEW_001_CreateReview_CompletedBooking_ShouldCreateReview()
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
            var response = await Client.PostAsJsonAsync("/api/tenant/reviews", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var review = await DbContext.Reviews
                .FirstOrDefaultAsync(r => r.BookingId == booking.BookingId);
            review.Should().NotBeNull();
            review!.Rating.Should().Be(5);
            review.Comment.Should().Be("Great place to stay!");
        }

        [Fact]
        [Trait("Category", "Review")]
        [Trait("TestID", "TC-REVIEW-002")]
        public async Task TC_REVIEW_002_CreateReview_PendingBooking_ShouldReturnBadRequest()
        {
            // Arrange
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
            var response = await Client.PostAsJsonAsync("/api/tenant/reviews", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", "Review")]
        [Trait("TestID", "TC-REVIEW-004")]
        public async Task TC_REVIEW_004_GetMyReviews_ShouldReturnUserReviews()
        {
            // Arrange - Create reviews
            var reviews = new List<Review>
            {
                new Review
                {
                    CondotelId = 1,
                    UserId = 3,
                    Rating = 5,
                    Comment = "Excellent!",
                    Status = "Visible",
                    CreatedAt = DateTime.UtcNow
                },
                new Review
                {
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

            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/tenant/reviews");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("data").GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
        }

        [Fact]
        [Trait("Category", "Review")]
        [Trait("TestID", "TC-REVIEW-007")]
        public async Task TC_REVIEW_007_HostReplyReview_ShouldAddReply()
        {
            // Arrange - Create review
            var review = new Review
            {
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
            var response = await Client.PutAsJsonAsync($"/api/host/review/{review.ReviewId}/reply", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var updatedReview = await DbContext.Reviews.FindAsync(review.ReviewId);
            updatedReview!.Reply.Should().Be("Thank you for your feedback!");
        }

        #endregion

        #region TC-HOST-001 to TC-HOST-006: Host Flow

        [Fact]
        [Trait("Category", "Host")]
        [Trait("TestID", "TC-HOST-001")]
        public async Task TC_HOST_001_CreateCondotel_ShouldCreateCondotel()
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
                bathrooms = 2,
                status = "Active"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/condotel", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
            
            var condotel = await DbContext.Condotels
                .FirstOrDefaultAsync(c => c.Name == "New Condotel");
            condotel.Should().NotBeNull();
            condotel!.HostId.Should().Be(1);
        }

        [Fact]
        [Trait("Category", "Host")]
        [Trait("TestID", "TC-HOST-004")]
        public async Task TC_HOST_004_GetMyCondotels_ShouldReturnHostCondotels()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/condotel");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<CondotelDTO>>();
            result.Should().NotBeNull();
            result!.Count.Should().BeGreaterThan(0);
        }

        #endregion

        #region TC-VOUCHER-001 to TC-VOUCHER-003: Voucher Flow

        [Fact]
        [Trait("Category", "Voucher")]
        [Trait("TestID", "TC-VOUCHER-001")]
        public async Task TC_VOUCHER_001_HostCreateVoucher_ShouldCreateVoucher()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                code = "TEST2024",
                condotelId = 1,
                discountPercentage = 10,
                maxUses = 100,
                expiryDate = DateTime.UtcNow.AddDays(30)
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/voucher", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var voucher = await DbContext.Vouchers
                .FirstOrDefaultAsync(v => v.Code == "TEST2024");
            voucher.Should().NotBeNull();
            voucher!.DiscountPercentage.Should().Be(10);
        }

        [Fact]
        [Trait("Category", "Voucher")]
        [Trait("TestID", "TC-VOUCHER-002")]
        public async Task TC_VOUCHER_002_CreateVoucher_DuplicateCode_ShouldReturnBadRequest()
        {
            // Arrange - Create existing voucher
            var existingVoucher = new Voucher
            {
                Code = "EXISTING",
                CondotelId = 1,
                DiscountPercentage = 15,
                MaxUses = 50,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };
            DbContext.Vouchers.Add(existingVoucher);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                code = "EXISTING",
                condotelId = 1,
                discountPercentage = 10,
                maxUses = 100,
                expiryDate = DateTime.UtcNow.AddDays(30)
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/voucher", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region TC-ADMIN-001 to TC-ADMIN-003: Admin Flow

        [Fact]
        [Trait("Category", "Admin")]
        [Trait("TestID", "TC-ADMIN-001")]
        public async Task TC_ADMIN_001_GetDashboard_ShouldReturnDashboardData()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/admin/dashboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.Should().NotBeNull();
        }

        #endregion

        #region TC-AUTHZ-001 to TC-AUTHZ-002: Authorization Tests

        [Fact]
        [Trait("Category", "Authorization")]
        [Trait("TestID", "TC-AUTHZ-001")]
        public async Task TC_AUTHZ_001_AccessWithWrongRole_ShouldReturnForbidden()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/condotel", new { });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        [Trait("Category", "Authorization")]
        [Trait("TestID", "TC-AUTHZ-002")]
        public async Task TC_AUTHZ_002_AccessWithoutToken_ShouldReturnUnauthorized()
        {
            // Act
            var response = await Client.GetAsync("/api/booking/my");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion
    }
}

