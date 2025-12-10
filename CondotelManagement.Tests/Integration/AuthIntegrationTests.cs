using CondotelManagement.Data;
using CondotelManagement.DTOs.Auth;
using CondotelManagement.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Tests.Integration
{
    public class AuthIntegrationTests : TestBase
    {
        public AuthIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsTokenAndUser()
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
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
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
        public async Task Register_WithNewEmail_CreatesPendingUser()
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
            
            // Verify user was created with Pending status
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "newuser@test.com");
            user.Should().NotBeNull();
            user!.Status.Should().Be("Pending");
            user.PasswordResetToken.Should().NotBeNull(); // OTP was set
        }

        [Fact]
        public async Task Register_WithExistingActiveEmail_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "tenant@test.com", // Already exists and Active
                Password = "NewPassword123!",
                FullName = "Duplicate User",
                Phone = "0123456789"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task VerifyEmail_WithValidOtp_ActivatesUser()
        {
            // Arrange - Create a pending user with OTP
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
        public async Task VerifyEmail_WithInvalidOtp_ReturnsBadRequest()
        {
            // Arrange
            var user = new User
            {
                FullName = "Pending User 2",
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
                Otp = "999999" // Wrong OTP
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/verify-email", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetMe_WithValidToken_ReturnsUserProfile()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var userProfile = await DeserializeResponse<UserProfileDto>(response);
            userProfile.Should().NotBeNull();
            userProfile!.Email.Should().Be("tenant@test.com");
            userProfile.RoleName.Should().Be("Tenant");
        }

        [Fact]
        public async Task GetMe_WithoutToken_ReturnsUnauthorized()
        {
            // Act
            var response = await Client.GetAsync("/api/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ForgotPassword_WithValidEmail_SendsOtp()
        {
            // Arrange
            var request = new ForgotPasswordRequest
            {
                Email = "tenant@test.com"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/send-otp", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Verify OTP was set
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "tenant@test.com");
            user.Should().NotBeNull();
            user!.PasswordResetToken.Should().NotBeNull();
        }

        [Fact]
        public async Task ResetPasswordWithOtp_WithValidOtp_UpdatesPassword()
        {
            // Arrange - Set OTP for user
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "tenant@test.com");
            user!.PasswordResetToken = "123456";
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(10);
            await DbContext.SaveChangesAsync();

            var oldPasswordHash = user.PasswordHash;

            var request = new ResetPasswordWithOtpRequest
            {
                Email = "tenant@test.com",
                Otp = "123456",
                NewPassword = "NewPassword123!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/reset-password-with-otp", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var updatedUser = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "tenant@test.com");
            updatedUser!.PasswordHash.Should().NotBe(oldPasswordHash);
            updatedUser.PasswordResetToken.Should().BeNull();
        }
    }
}

