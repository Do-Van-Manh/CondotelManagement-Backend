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
    /// Integration tests cho TẤT CẢ các modules còn lại
    /// </summary>
    public class AllModulesIntegrationTests : TestBase
    {
        public AllModulesIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        #region Reward Points Tests

        [Fact]
        [Trait("Category", "RewardPoints")]
        [Trait("TestID", "TC-REWARD-001")]
        public async Task TC_REWARD_001_GetMyPoints_ShouldReturnPoints()
        {
            // Arrange - Create reward points
            var rewardPoint = new RewardPoint
            {
                CustomerId = 3,
                TotalPoints = 5000,
                LastUpdated = DateTime.UtcNow
            };
            DbContext.RewardPoints.Add(rewardPoint);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/tenant/rewards/points");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("success").GetBoolean().Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "RewardPoints")]
        [Trait("TestID", "TC-REWARD-002")]
        public async Task TC_REWARD_002_CalculateDiscount_ShouldReturnDiscount()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/tenant/rewards/calculate-discount?points=5000");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("discountAmount").GetDecimal().Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Category", "RewardPoints")]
        [Trait("TestID", "TC-REWARD-003")]
        public async Task TC_REWARD_003_GetPointsHistory_ShouldReturnHistory()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/tenant/rewards/history?page=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Chat Tests

        [Fact]
        [Trait("Category", "Chat")]
        [Trait("TestID", "TC-CHAT-001")]
        public async Task TC_CHAT_001_GetConversations_ShouldReturnConversations()
        {
            // Arrange - Create conversation
            var conversation = new ChatConversation
            {
                Name = "Test Conversation",
                ConversationType = "Direct",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.ChatConversations.Add(conversation);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/chat/conversations/3");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Chat")]
        [Trait("TestID", "TC-CHAT-002")]
        public async Task TC_CHAT_002_GetMessages_ShouldReturnMessages()
        {
            // Arrange - Create conversation and message
            var conversation = new ChatConversation
            {
                Name = "Test",
                ConversationType = "Direct",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.ChatConversations.Add(conversation);
            await DbContext.SaveChangesAsync();

            var message = new ChatMessage
            {
                ConversationId = conversation.ConversationId,
                SenderId = 3,
                Content = "Test message",
                SentAt = DateTime.UtcNow
            };
            DbContext.ChatMessages.Add(message);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/api/chat/messages/{conversation.ConversationId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Chat")]
        [Trait("TestID", "TC-CHAT-003")]
        public async Task TC_CHAT_003_SendDirectMessage_ShouldCreateMessage()
        {
            // Arrange
            var request = new
            {
                senderId = 3,
                receiverId = 2,
                content = "Hello, this is a test message"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/chat/messages/send-direct", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Blog Tests

        [Fact]
        [Trait("Category", "Blog")]
        [Trait("TestID", "TC-BLOG-001")]
        public async Task TC_BLOG_001_GetPublishedPosts_ShouldReturnPosts()
        {
            // Arrange - Create blog post
            var category = new BlogCategory
            {
                Name = "Test Category",
                Slug = "test-category"
            };
            DbContext.BlogCategories.Add(category);
            await DbContext.SaveChangesAsync();

            var post = new BlogPost
            {
                CategoryId = category.CategoryId,
                AuthorUserId = 1,
                Title = "Test Post",
                Slug = "test-post",
                Content = "Test content",
                Status = "Published",
                PublishedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.BlogPosts.Add(post);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/blog/posts");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<object>>();
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Blog")]
        [Trait("TestID", "TC-BLOG-002")]
        public async Task TC_BLOG_002_GetPostBySlug_ShouldReturnPost()
        {
            // Arrange
            var category = new BlogCategory
            {
                Name = "Test Category 2",
                Slug = "test-category-2"
            };
            DbContext.BlogCategories.Add(category);
            await DbContext.SaveChangesAsync();

            var post = new BlogPost
            {
                CategoryId = category.CategoryId,
                AuthorUserId = 1,
                Title = "Test Post 2",
                Slug = "test-post-2",
                Content = "Test content",
                Status = "Published",
                PublishedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.BlogPosts.Add(post);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/blog/posts/test-post-2");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Blog")]
        [Trait("TestID", "TC-BLOG-003")]
        public async Task TC_BLOG_003_GetCategories_ShouldReturnCategories()
        {
            // Arrange
            var category = new BlogCategory
            {
                Name = "Test Category 3",
                Slug = "test-category-3"
            };
            DbContext.BlogCategories.Add(category);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/blog/categories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Promotion Tests

        [Fact]
        [Trait("Category", "Promotion")]
        [Trait("TestID", "TC-PROMO-001")]
        public async Task TC_PROMO_001_GetAllPromotions_ShouldReturnPromotions()
        {
            // Arrange
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

            // Act
            var response = await Client.GetAsync("/api/promotions");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Promotion")]
        [Trait("TestID", "TC-PROMO-002")]
        public async Task TC_PROMO_002_GetPromotionsByCondotel_ShouldReturnFiltered()
        {
            // Arrange
            var promotion = new Promotion
            {
                CondotelId = 1,
                Name = "Test Promotion 2",
                DiscountPercentage = 15,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                Status = "Active"
            };
            DbContext.Promotions.Add(promotion);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/promotions/condotel/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Promotion")]
        [Trait("TestID", "TC-PROMO-003")]
        public async Task TC_PROMO_003_HostCreatePromotion_ShouldCreatePromotion()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                condotelId = 1,
                name = "New Promotion",
                discountPercentage = 25,
                startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                status = "Active"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/promotion", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        [Trait("Category", "Promotion")]
        [Trait("TestID", "TC-PROMO-004")]
        public async Task TC_PROMO_004_HostUpdatePromotion_ShouldUpdatePromotion()
        {
            // Arrange
            var promotion = new Promotion
            {
                CondotelId = 1,
                Name = "Update Test",
                DiscountPercentage = 10,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                Status = "Active"
            };
            DbContext.Promotions.Add(promotion);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                condotelId = 1,
                name = "Updated Promotion",
                discountPercentage = 30,
                startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                status = "Active"
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/host/promotion/{promotion.PromotionId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Promotion")]
        [Trait("TestID", "TC-PROMO-005")]
        public async Task TC_PROMO_005_HostDeletePromotion_ShouldDeletePromotion()
        {
            // Arrange
            var promotion = new Promotion
            {
                CondotelId = 1,
                Name = "Delete Test",
                DiscountPercentage = 10,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                Status = "Active"
            };
            DbContext.Promotions.Add(promotion);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.DeleteAsync($"/api/host/promotion/{promotion.PromotionId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Service Package Tests

        [Fact]
        [Trait("Category", "ServicePackage")]
        [Trait("TestID", "TC-SERVICEPKG-001")]
        public async Task TC_SERVICEPKG_001_HostGetServicePackages_ShouldReturnPackages()
        {
            // Arrange
            var servicePackage = new ServicePackage
            {
                HostID = 1,
                Name = "Test Service",
                Description = "Test Description",
                Price = 100000,
                Status = "Active"
            };
            DbContext.ServicePackages.Add(servicePackage);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/service-packages");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "ServicePackage")]
        [Trait("TestID", "TC-SERVICEPKG-002")]
        public async Task TC_SERVICEPKG_002_HostCreateServicePackage_ShouldCreatePackage()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                hostId = 1,
                name = "New Service Package",
                description = "New Description",
                price = 150000,
                status = "Active"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/service-packages", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "ServicePackage")]
        [Trait("TestID", "TC-SERVICEPKG-003")]
        public async Task TC_SERVICEPKG_003_HostUpdateServicePackage_ShouldUpdatePackage()
        {
            // Arrange
            var servicePackage = new ServicePackage
            {
                HostID = 1,
                Name = "Update Test",
                Description = "Test",
                Price = 100000,
                Status = "Active"
            };
            DbContext.ServicePackages.Add(servicePackage);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                hostId = 1,
                name = "Updated Service",
                description = "Updated",
                price = 200000,
                status = "Active"
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/host/service-packages/{servicePackage.ServiceId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "ServicePackage")]
        [Trait("TestID", "TC-SERVICEPKG-004")]
        public async Task TC_SERVICEPKG_004_HostDeleteServicePackage_ShouldDeletePackage()
        {
            // Arrange
            var servicePackage = new ServicePackage
            {
                HostID = 1,
                Name = "Delete Test",
                Description = "Test",
                Price = 100000,
                Status = "Active"
            };
            DbContext.ServicePackages.Add(servicePackage);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.DeleteAsync($"/api/host/service-packages/{servicePackage.ServiceId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Location/Resort Tests

        [Fact]
        [Trait("Category", "Location")]
        [Trait("TestID", "TC-LOCATION-001")]
        public async Task TC_LOCATION_001_GetAllLocations_ShouldReturnLocations()
        {
            // Act
            var response = await Client.GetAsync("/api/host/location");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Location")]
        [Trait("TestID", "TC-LOCATION-002")]
        public async Task TC_LOCATION_002_HostCreateLocation_ShouldCreateLocation()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                name = "New Location",
                description = "New Location Description"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/location", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        [Trait("Category", "Resort")]
        [Trait("TestID", "TC-RESORT-001")]
        public async Task TC_RESORT_001_HostGetResorts_ShouldReturnResorts()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/resort");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Resort")]
        [Trait("TestID", "TC-RESORT-002")]
        public async Task TC_RESORT_002_HostCreateResort_ShouldCreateResort()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                locationId = 1,
                name = "New Resort",
                description = "New Resort Description"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/resort", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        #endregion

        #region Profile Tests

        [Fact]
        [Trait("Category", "Profile")]
        [Trait("TestID", "TC-PROFILE-001")]
        public async Task TC_PROFILE_001_GetMyProfile_ShouldReturnProfile()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/profile");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Profile")]
        [Trait("TestID", "TC-PROFILE-002")]
        public async Task TC_PROFILE_002_UpdateProfile_ShouldUpdateProfile()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            var request = new
            {
                fullName = "Updated Name",
                phone = "0987654321",
                address = "Updated Address"
            };

            // Act
            var response = await Client.PutAsJsonAsync("/api/profile", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Admin User Management Tests

        [Fact]
        [Trait("Category", "Admin")]
        [Trait("TestID", "TC-ADMIN-002")]
        public async Task TC_ADMIN_002_GetAllUsers_ShouldReturnUsers()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/admin/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
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
            var response = await Client.GetAsync("/api/admin/users/3");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Admin")]
        [Trait("TestID", "TC-ADMIN-004")]
        public async Task TC_ADMIN_004_CreateUser_ShouldCreateUser()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            var request = new
            {
                fullName = "Admin Created User",
                email = "admincreated@test.com",
                password = "Password123!",
                roleId = 3,
                phone = "0123456789"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/admin/users", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        [Trait("Category", "Admin")]
        [Trait("TestID", "TC-ADMIN-005")]
        public async Task TC_ADMIN_005_UpdateUserStatus_ShouldUpdateStatus()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            var request = new
            {
                userId = 3,
                status = "Inactive"
            };

            // Act
            var response = await Client.PutAsJsonAsync("/api/admin/users/3/status", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var user = await DbContext.Users.FindAsync(3);
            user!.Status.Should().Be("Inactive");
        }

        #endregion

        #region Utility Tests

        [Fact]
        [Trait("Category", "Utility")]
        [Trait("TestID", "TC-UTILITY-001")]
        public async Task TC_UTILITY_001_HostGetUtilities_ShouldReturnUtilities()
        {
            // Arrange
            var utility = new Utility
            {
                HostId = 1,
                Name = "Test Utility",
                Category = "Test Category",
                Description = "Test Description"
            };
            DbContext.Utilities.Add(utility);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/utility");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Utility")]
        [Trait("TestID", "TC-UTILITY-002")]
        public async Task TC_UTILITY_002_HostCreateUtility_ShouldCreateUtility()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                hostId = 1,
                name = "New Utility",
                category = "New Category",
                description = "New Description"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/utility", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        #endregion

        #region Host Package Tests

        [Fact]
        [Trait("Category", "HostPackage")]
        [Trait("TestID", "TC-HOSTPKG-001")]
        public async Task TC_HOSTPKG_001_GetAvailablePackages_ShouldReturnPackages()
        {
            // Arrange
            var package = new Package
            {
                Name = "Test Package",
                Description = "Test Description",
                Price = 1000000,
                Duration = "1 month",
                Status = "Active"
            };
            DbContext.Packages.Add(package);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/package");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion
    }
}














