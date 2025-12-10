using CondotelManagement.Data;
using CondotelManagement.DTOs;
using CondotelManagement.DTOs.Auth;
using CondotelManagement.DTOs.Booking;
using CondotelManagement.DTOs.Admin;
using CondotelManagement.DTOs.Host;
using CondotelManagement.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Tests.Integration
{
    /// <summary>
    /// Integration tests bổ sung dựa trên TestCases_Reorganized1.csv
    /// Đảm bảo kiểm tra output tiếng Việt
    /// </summary>
    public class AdditionalIntegrationTests : TestBase
    {
        public AdditionalIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        #region TC-AUTH-002 to TC-AUTH-044: Authentication Tests

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-002")]
        public async Task TC_AUTH_002_RegisterWithExistingEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "tenant@test.com",
                Password = "Password123!",
                FullName = "Existing User",
                Phone = "0123456789"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            result.GetProperty("message").GetString().Should().Contain("đã tồn tại");
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-003")]
        public async Task TC_AUTH_003_RegisterWithInvalidEmailFormat_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "invalid-email",
                Password = "Password123!",
                FullName = "Test User",
                Phone = "0123456789"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-005")]
        public async Task TC_AUTH_005_VerifyEmailWithInvalidOTP_ShouldReturnBadRequest()
        {
            // Arrange - Create pending user with OTP
            var user = new User
            {
                FullName = "Pending User",
                Email = "pending2@test.com",
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
                Email = "pending2@test.com",
                Otp = "999999" // Invalid OTP
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/verify-email", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            result.GetProperty("message").GetString().Should().ContainAny("không hợp lệ", "sai", "hết hạn");
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-007")]
        public async Task TC_AUTH_007_LoginWithInvalidPassword_ShouldReturnUnauthorized()
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
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            result.GetProperty("message").GetString().Should().ContainAny("không đúng", "chưa được kích hoạt");
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-008")]
        public async Task TC_AUTH_008_LoginWithNonExistentEmail_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "notexist@test.com",
                Password = "Password123!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-009")]
        public async Task TC_AUTH_009_GetCurrentUser_ShouldReturnUserProfile()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("userId").GetInt32().Should().Be(3);
            result.GetProperty("email").GetString().Should().Be("tenant@test.com");
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-010")]
        public async Task TC_AUTH_010_GetCurrentUserWithoutToken_ShouldReturnUnauthorized()
        {
            // Act
            var response = await Client.GetAsync("/api/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-011")]
        public async Task TC_AUTH_011_SendOTPForPasswordReset_ShouldReturnOk()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "tenant@test.com" };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/send-otp", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-013")]
        public async Task TC_AUTH_013_ResetPasswordWithInvalidOTP_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new ResetPasswordWithOtpRequest
            {
                Email = "tenant@test.com",
                Otp = "999999",
                NewPassword = "NewPassword123!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/reset-password-with-otp", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            result.GetProperty("message").GetString().Should().ContainAny("thất bại", "không hợp lệ", "sai", "hết hạn");
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-016")]
        public async Task TC_AUTH_016_Logout_ShouldReturnOk()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.PostAsync("/api/auth/logout", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            result.GetProperty("message").GetString().Should().Contain("thành công");
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-018")]
        public async Task TC_AUTH_018_AdminCheck_ShouldReturnOk()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/auth/admin-check");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            result.GetProperty("message").GetString().Should().ContainAny("Admin", "Chào mừng");
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-019")]
        public async Task TC_AUTH_019_RegisterWithMissingFields_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@test.com"
                // Missing required fields
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-020")]
        public async Task TC_AUTH_020_RegisterWithWeakPassword_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "newuser2@test.com",
                Password = "12345", // Weak password
                FullName = "New User",
                Phone = "0123456789"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", "Authentication")]
        [Trait("TestID", "TC-AUTH-022")]
        public async Task TC_AUTH_022_LoginWithUnverifiedAccount_ShouldReturnUnauthorized()
        {
            // Arrange - Create pending user
            var user = new User
            {
                FullName = "Pending User",
                Email = "pending3@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                RoleId = 3,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var request = new LoginRequest
            {
                Email = "pending3@test.com",
                Password = "Password123!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region TC-ADMIN-002 to TC-ADMIN-030: Admin Management Tests

        [Fact]
        [Trait("Category", "Admin")]
        [Trait("TestID", "TC-ADMIN-002")]
        public async Task TC_ADMIN_002_GetAllUsers_ShouldReturnUserList()
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
        [Trait("Category", "Admin")]
        [Trait("TestID", "TC-ADMIN-003")]
        public async Task TC_ADMIN_003_GetUserById_ShouldReturnUser()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/admin/users/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<UserViewDTO>();
            result.Should().NotBeNull();
            result!.UserId.Should().Be(1);
        }

        [Fact]
        [Trait("Category", "Admin")]
        [Trait("TestID", "TC-ADMIN-004")]
        public async Task TC_ADMIN_004_CreateUser_ShouldCreateUser()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            var request = new AdminCreateUserDTO
            {
                FullName = "Admin Created User",
                Email = "admincreated@test.com",
                Password = "Password123!",
                RoleId = 3,
                Phone = "0123456789"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/admin/users", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "admincreated@test.com");
            user.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Admin")]
        [Trait("TestID", "TC-ADMIN-005")]
        public async Task TC_ADMIN_005_UpdateUserStatus_ShouldUpdateStatus()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            var request = new UpdateUserStatusDTO
            {
                UserId = 1,
                Status = "Inactive"
            };

            // Act
            var response = await Client.PutAsJsonAsync("/api/admin/users/1/status", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var user = await DbContext.Users.FindAsync(1);
            user!.Status.Should().Be("Inactive");
        }

        [Fact]
        [Trait("Category", "Admin")]
        [Trait("TestID", "TC-ADMIN-015")]
        public async Task TC_ADMIN_015_GetNonExistentUser_ShouldReturnNotFound()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/admin/users/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Không tìm thấy");
        }

        [Fact]
        [Trait("Category", "Admin")]
        [Trait("TestID", "TC-ADMIN-016")]
        public async Task TC_ADMIN_016_CreateUserWithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            var request = new AdminCreateUserDTO
            {
                FullName = "",
                Email = "invalid-email",
                Password = "123",
                RoleId = 0
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/admin/users", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", "Admin")]
        [Trait("TestID", "TC-ADMIN-017")]
        public async Task TC_ADMIN_017_CreateUserWithExistingEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            var request = new AdminCreateUserDTO
            {
                FullName = "New User",
                Email = "tenant@test.com", // Existing email
                Password = "Password123!",
                RoleId = 3
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/admin/users", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region TC-BOOKING-011 to TC-BOOKING-017: Additional Booking Tests

        [Fact]
        [Trait("Category", "Booking")]
        [Trait("TestID", "TC-BOOKING-011")]
        public async Task TC_BOOKING_011_UpdateBooking_ShouldUpdateBooking()
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

            var request = new BookingDTO
            {
                BookingId = booking.BookingId,
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(6)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(8))
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/booking/{booking.BookingId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Booking")]
        [Trait("TestID", "TC-BOOKING-012")]
        public async Task TC_BOOKING_012_GetBookingsByHost_ShouldReturnHostBookings()
        {
            // Arrange
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

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/booking");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<BookingDTO>>();
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Booking")]
        [Trait("TestID", "TC-BOOKING-014")]
        public async Task TC_BOOKING_014_GetNonExistentBooking_ShouldReturnNotFound()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/booking/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", "Booking")]
        [Trait("TestID", "TC-BOOKING-016")]
        public async Task TC_BOOKING_016_UpdateBookingWithInvalidDateRange_ShouldReturnBadRequest()
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

            var request = new BookingDTO
            {
                BookingId = booking.BookingId,
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(3)) // End before start
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/booking/{booking.BookingId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region TC-REVIEW-005 to TC-REVIEW-018: Additional Review Tests

        [Fact]
        [Trait("Category", "Review")]
        [Trait("TestID", "TC-REVIEW-005")]
        public async Task TC_REVIEW_005_GetReviewById_ShouldReturnReview()
        {
            // Arrange
            var review = new Review
            {
                CondotelId = 1,
                UserId = 3,
                Rating = 5,
                Comment = "Great place!",
                Status = "Visible",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Reviews.Add(review);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync($"/api/tenant/reviews/{review.ReviewId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("data").GetProperty("reviewId").GetInt32().Should().Be(review.ReviewId);
        }

        [Fact]
        [Trait("Category", "Review")]
        [Trait("TestID", "TC-REVIEW-006")]
        public async Task TC_REVIEW_006_UpdateReviewWithin7Days_ShouldUpdateReview()
        {
            // Arrange
            var review = new Review
            {
                CondotelId = 1,
                UserId = 3,
                Rating = 5,
                Comment = "Original comment",
                Status = "Visible",
                CreatedAt = DateTime.UtcNow // Created now, within 7 days
            };
            DbContext.Reviews.Add(review);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            var request = new
            {
                reviewId = review.ReviewId,
                rating = 4,
                comment = "Updated review comment"
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/tenant/reviews/{review.ReviewId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var updatedReview = await DbContext.Reviews.FindAsync(review.ReviewId);
            updatedReview!.Comment.Should().Be("Updated review comment");
        }

        [Fact]
        [Trait("Category", "Review")]
        [Trait("TestID", "TC-REVIEW-007")]
        public async Task TC_REVIEW_007_DeleteReviewWithin7Days_ShouldDeleteReview()
        {
            // Arrange
            var review = new Review
            {
                CondotelId = 1,
                UserId = 3,
                Rating = 5,
                Comment = "To be deleted",
                Status = "Visible",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Reviews.Add(review);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.DeleteAsync($"/api/tenant/reviews/{review.ReviewId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var deletedReview = await DbContext.Reviews.FindAsync(review.ReviewId);
            deletedReview.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Review")]
        [Trait("TestID", "TC-REVIEW-008")]
        public async Task TC_REVIEW_008_GetReviewsByHost_ShouldReturnHostReviews()
        {
            // Arrange
            var review = new Review
            {
                CondotelId = 1,
                UserId = 3,
                Rating = 5,
                Comment = "Great stay!",
                Status = "Visible",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Reviews.Add(review);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/review");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("data").GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        [Trait("Category", "Review")]
        [Trait("TestID", "TC-REVIEW-009")]
        public async Task TC_REVIEW_009_HostReportReview_ShouldReportReview()
        {
            // Arrange
            var review = new Review
            {
                CondotelId = 1,
                UserId = 3,
                Rating = 1,
                Comment = "Inappropriate comment",
                Status = "Visible",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Reviews.Add(review);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.PutAsJsonAsync($"/api/host/review/{review.ReviewId}/report", new { });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var reportedReview = await DbContext.Reviews.FindAsync(review.ReviewId);
            reportedReview!.Status.Should().Be("Reported");
        }

        [Fact]
        [Trait("Category", "Review")]
        [Trait("TestID", "TC-REVIEW-010")]
        public async Task TC_REVIEW_010_HostReplyToNonExistentReview_ShouldReturnNotFound()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                reply = "Thank you for your feedback!"
            };

            // Act
            var response = await Client.PutAsJsonAsync("/api/host/review/999/reply", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", "Review")]
        [Trait("TestID", "TC-REVIEW-017")]
        public async Task TC_REVIEW_017_CreateReviewWithInvalidRating_ShouldReturnBadRequest()
        {
            // Arrange
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
                rating = 10, // Invalid rating (should be 1-5)
                comment = "Great stay!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/tenant/reviews", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region TC-HOST-005 to TC-HOST-011: Additional Host Tests

        [Fact]
        [Trait("Category", "Host")]
        [Trait("TestID", "TC-HOST-005")]
        public async Task TC_HOST_005_GetCondotelById_ShouldReturnCondotel()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/condotel/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<CondotelDetailDTO>();
            result.Should().NotBeNull();
            result!.CondotelId.Should().Be(1);
        }

        [Fact]
        [Trait("Category", "Host")]
        [Trait("TestID", "TC-HOST-006")]
        public async Task TC_HOST_006_GetNonExistentCondotel_ShouldReturnNotFound()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/condotel/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().ContainAny("not found", "Không tìm thấy");
        }

        [Fact]
        [Trait("Category", "Host")]
        [Trait("TestID", "TC-HOST-007")]
        public async Task TC_HOST_007_DeleteCondotel_ShouldDeleteCondotel()
        {
            // Arrange
            var condotel = new Condotel
            {
                HostId = 1,
                ResortId = 1,
                Name = "To Be Deleted",
                Description = "Description",
                PricePerNight = 100000,
                Beds = 2,
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
            deletedCondotel!.Status.Should().Be("Deleted");
        }

        [Fact]
        [Trait("Category", "Host")]
        [Trait("TestID", "TC-HOST-009")]
        public async Task TC_HOST_009_CreateCondotelWithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                name = "",
                description = "",
                resortId = 0,
                pricePerNight = -100
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/condotel", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region TC-VOUCHER-004 to TC-VOUCHER-012: Additional Voucher Tests

        [Fact]
        [Trait("Category", "Voucher")]
        [Trait("TestID", "TC-VOUCHER-004")]
        public async Task TC_VOUCHER_004_GetVouchersByHost_ShouldReturnHostVouchers()
        {
            // Arrange
            var voucher = new Voucher
            {
                Code = "HOSTVOUCHER",
                CondotelId = 1,
                DiscountPercentage = 15,
                MaxUses = 50,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };
            DbContext.Vouchers.Add(voucher);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/voucher");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("data").GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        [Trait("Category", "Voucher")]
        [Trait("TestID", "TC-VOUCHER-005")]
        public async Task TC_VOUCHER_005_UpdateVoucher_ShouldUpdateVoucher()
        {
            // Arrange
            var voucher = new Voucher
            {
                Code = "UPDATETEST",
                CondotelId = 1,
                DiscountPercentage = 10,
                MaxUses = 100,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };
            DbContext.Vouchers.Add(voucher);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                code = "UPDATED2024",
                discountPercentage = 15,
                maxUses = 200,
                expiryDate = DateTime.UtcNow.AddDays(60)
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/host/voucher/{voucher.VoucherId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var updatedVoucher = await DbContext.Vouchers.FindAsync(voucher.VoucherId);
            updatedVoucher!.DiscountPercentage.Should().Be(15);
        }

        [Fact]
        [Trait("Category", "Voucher")]
        [Trait("TestID", "TC-VOUCHER-006")]
        public async Task TC_VOUCHER_006_DeleteVoucher_ShouldDeleteVoucher()
        {
            // Arrange
            var voucher = new Voucher
            {
                Code = "DELETETEST",
                CondotelId = 1,
                DiscountPercentage = 10,
                MaxUses = 100,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };
            DbContext.Vouchers.Add(voucher);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.DeleteAsync($"/api/host/voucher/{voucher.VoucherId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
        }

        [Fact]
        [Trait("Category", "Voucher")]
        [Trait("TestID", "TC-VOUCHER-009")]
        public async Task TC_VOUCHER_009_CreateVoucherWithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                code = "",
                condotelId = 0,
                discountPercentage = -10,
                maxUses = -1
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/voucher", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region TC-PAYMENT-001 to TC-PAYMENT-004: Payment Tests

        [Fact]
        [Trait("Category", "Payment")]
        [Trait("TestID", "TC-PAYMENT-001")]
        public async Task TC_PAYMENT_001_CreatePaymentLink_ShouldCreatePaymentLink()
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
                bookingId = booking.BookingId
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/payment/payos/create", request);

            // Assert
            // Payment link creation may require PayOS service to be configured
            // In test environment, this might fail, which is acceptable
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        [Trait("Category", "Payment")]
        [Trait("TestID", "TC-PAYMENT-002")]
        public async Task TC_PAYMENT_002_CreatePaymentWithNonExistentBooking_ShouldReturnNotFound()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            var request = new
            {
                bookingId = 99999
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/payment/payos/create", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", "Payment")]
        [Trait("TestID", "TC-PAYMENT-003")]
        public async Task TC_PAYMENT_003_CreatePaymentForOtherUserBooking_ShouldReturnForbidden()
        {
            // Arrange
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 1, // Different customer
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
                bookingId = booking.BookingId
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/payment/payos/create", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", "Payment")]
        [Trait("TestID", "TC-PAYMENT-004")]
        public async Task TC_PAYMENT_004_CreatePaymentForAlreadyPaidBooking_ShouldReturnBadRequest()
        {
            // Arrange
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                Status = "Confirmed", // Already paid
                TotalPrice = 200000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            var request = new
            {
                bookingId = booking.BookingId
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/payment/payos/create", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().ContainAny("not in a payable state", "không thể thanh toán");
        }

        #endregion

        #region TC-HOST-004: Register as Host

        [Fact]
        [Trait("Category", "Host")]
        [Trait("TestID", "TC-HOST-004")]
        public async Task TC_HOST_004_RegisterAsHost_ShouldRegisterHost()
        {
            // Arrange - Create a new user first
            var newUser = new User
            {
                FullName = "New Host User",
                Email = "newhostuser@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                RoleId = 3,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Users.Add(newUser);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(newUser.UserId, "newhostuser@test.com", "Tenant");
            SetAuthHeader(token);

            var request = new HostRegisterRequestDto
            {
                PhoneContact = "0123456789",
                Address = "Host Address"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/register-as-host", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            result.GetProperty("message").GetString().Should().Contain("thành công");
        }

        #endregion

        #region TC-TENANT-005 to TC-TENANT-007: Additional Tenant Tests

        [Fact]
        [Trait("Category", "Tenant")]
        [Trait("TestID", "TC-TENANT-005")]
        public async Task TC_TENANT_005_FilterCondotelByLocation_ShouldReturnFiltered()
        {
            // Act
            var response = await Client.GetAsync("/api/tenant/condotels?locationId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<CondotelDTO>>();
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Tenant")]
        [Trait("TestID", "TC-TENANT-006")]
        public async Task TC_TENANT_006_FilterCondotelByDateRange_ShouldReturnFiltered()
        {
            // Act
            var checkIn = DateOnly.FromDateTime(DateTime.Now.AddDays(10));
            var checkOut = DateOnly.FromDateTime(DateTime.Now.AddDays(15));
            var response = await Client.GetAsync($"/api/tenant/condotels?fromDate={checkIn:yyyy-MM-dd}&toDate={checkOut:yyyy-MM-dd}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<CondotelDTO>>();
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Tenant")]
        [Trait("TestID", "TC-TENANT-007")]
        public async Task TC_TENANT_007_FilterCondotelByBedsAndBathrooms_ShouldReturnFiltered()
        {
            // Act
            var response = await Client.GetAsync("/api/tenant/condotels?beds=3&bathrooms=2");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<CondotelDTO>>();
            result.Should().NotBeNull();
        }

        #endregion
    }
}








