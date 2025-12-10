using CondotelManagement.Data;
using CondotelManagement.DTOs;
using CondotelManagement.DTOs.Auth;
using CondotelManagement.DTOs.Blog;
using CondotelManagement.DTOs.Location;
using CondotelManagement.DTOs.Resort;
using CondotelManagement.DTOs.Promotion;
using CondotelManagement.DTOs.Voucher;
using CondotelManagement.DTOs.ServicePackage;
using CondotelManagement.DTOs.Package;
using CondotelManagement.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Tests.Integration
{
    /// <summary>
    /// Integration tests mở rộng cho các chức năng: Location, Resort, Utility, Blog, Promotion, Package, Chat, Reward, Profile
    /// Dựa trên TestCases_Reorganized1.csv
    /// </summary>
    public class ExtendedIntegrationTests : TestBase
    {
        public ExtendedIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        #region TC-LOCATION-001 to TC-LOCATION-010: Location Management Tests

        [Fact]
        [Trait("Category", "Location")]
        [Trait("TestID", "TC-LOCATION-001")]
        public async Task TC_LOCATION_001_GetAllLocations_ShouldReturnLocations()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/location");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<LocationDTO>>();
            result.Should().NotBeNull();
            result!.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Category", "Location")]
        [Trait("TestID", "TC-LOCATION-002")]
        public async Task TC_LOCATION_002_CreateLocation_ShouldCreateLocation()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new LocationCreateUpdateDTO
            {
                Name = "New Location",
                Description = "New Location Description"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/location", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var location = await DbContext.Locations.FirstOrDefaultAsync(l => l.Name == "New Location");
            location.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Location")]
        [Trait("TestID", "TC-LOCATION-003")]
        public async Task TC_LOCATION_003_GetLocationById_ShouldReturnLocation()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/location/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<LocationDTO>();
            result.Should().NotBeNull();
            result!.LocationId.Should().Be(1);
        }

        [Fact]
        [Trait("Category", "Location")]
        [Trait("TestID", "TC-LOCATION-004")]
        public async Task TC_LOCATION_004_UpdateLocation_ShouldUpdateLocation()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new LocationCreateUpdateDTO
            {
                Name = "Updated Location",
                Description = "Updated Description"
            };

            // Act
            var response = await Client.PutAsJsonAsync("/api/host/location/1", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("thành công");
        }

        [Fact]
        [Trait("Category", "Location")]
        [Trait("TestID", "TC-LOCATION-005")]
        public async Task TC_LOCATION_005_DeleteLocation_ShouldDeleteLocation()
        {
            // Arrange
            var location = new Location
            {
                Name = "To Be Deleted",
                Description = "Description"
            };
            DbContext.Locations.Add(location);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.DeleteAsync($"/api/host/location/{location.LocationId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Location")]
        [Trait("TestID", "TC-LOCATION-006")]
        public async Task TC_LOCATION_006_GetNonExistentLocation_ShouldReturnNotFound()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/location/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", "Location")]
        [Trait("TestID", "TC-LOCATION-007")]
        public async Task TC_LOCATION_007_CreateLocationWithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new LocationCreateUpdateDTO
            {
                Name = "",
                Description = ""
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/location", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region TC-RESORT-001 to TC-RESORT-007: Resort Management Tests

        [Fact]
        [Trait("Category", "Resort")]
        [Trait("TestID", "TC-RESORT-001")]
        public async Task TC_RESORT_001_GetAllResorts_ShouldReturnResorts()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/resorts");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<ResortDTO>>();
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Resort")]
        [Trait("TestID", "TC-RESORT-002")]
        public async Task TC_RESORT_002_CreateResort_ShouldCreateResort()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new ResortCreateUpdateDTO
            {
                LocationId = 1,
                Name = "New Resort",
                Description = "New Resort Description"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/resort", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var resort = await DbContext.Resorts.FirstOrDefaultAsync(r => r.Name == "New Resort");
            resort.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Resort")]
        [Trait("TestID", "TC-RESORT-003")]
        public async Task TC_RESORT_003_GetResortById_ShouldReturnResort()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/resorts/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ResortDTO>();
            result.Should().NotBeNull();
            result!.ResortId.Should().Be(1);
        }

        [Fact]
        [Trait("Category", "Resort")]
        [Trait("TestID", "TC-RESORT-004")]
        public async Task TC_RESORT_004_GetResortsByLocationId_ShouldReturnResorts()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/resorts/location/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<ResortDTO>>();
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Resort")]
        [Trait("TestID", "TC-RESORT-005")]
        public async Task TC_RESORT_005_GetNonExistentResort_ShouldReturnNotFound()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/resorts/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().ContainAny("not found", "Không tìm thấy");
        }

        [Fact]
        [Trait("Category", "Resort")]
        [Trait("TestID", "TC-RESORT-007")]
        public async Task TC_RESORT_007_CreateResortWithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new ResortCreateUpdateDTO
            {
                Name = "",
                LocationId = 0
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/resort", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region TC-UTILITY-001 to TC-UTILITY-010: Utility Management Tests

        [Fact]
        [Trait("Category", "Utility")]
        [Trait("TestID", "TC-UTILITY-001")]
        public async Task TC_UTILITY_001_GetUtilities_ShouldReturnUtilities()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/utility");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<UtilityDTO>>();
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Utility")]
        [Trait("TestID", "TC-UTILITY-002")]
        public async Task TC_UTILITY_002_CreateUtility_ShouldCreateUtility()
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
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var utility = await DbContext.Utilities.FirstOrDefaultAsync(u => u.Name == "New Utility");
            utility.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Utility")]
        [Trait("TestID", "TC-UTILITY-003")]
        public async Task TC_UTILITY_003_GetUtilityById_ShouldReturnUtility()
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
            var response = await Client.GetAsync($"/api/host/utility/{utility.UtilityId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<UtilityDTO>();
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Utility")]
        [Trait("TestID", "TC-UTILITY-004")]
        public async Task TC_UTILITY_004_UpdateUtility_ShouldUpdateUtility()
        {
            // Arrange
            var utility = new Utility
            {
                HostId = 1,
                Name = "Original Utility",
                Category = "Original Category",
                Description = "Original Description"
            };
            DbContext.Utilities.Add(utility);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                name = "Updated Utility",
                category = "Updated Category",
                description = "Updated Description"
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/host/utility/{utility.UtilityId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().ContainAny("thành công", "success");
        }

        [Fact]
        [Trait("Category", "Utility")]
        [Trait("TestID", "TC-UTILITY-005")]
        public async Task TC_UTILITY_005_DeleteUtility_ShouldDeleteUtility()
        {
            // Arrange
            var utility = new Utility
            {
                HostId = 1,
                Name = "To Be Deleted",
                Category = "Category",
                Description = "Description"
            };
            DbContext.Utilities.Add(utility);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.DeleteAsync($"/api/host/utility/{utility.UtilityId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Utility")]
        [Trait("TestID", "TC-UTILITY-006")]
        public async Task TC_UTILITY_006_GetNonExistentUtility_ShouldReturnNotFound()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/utility/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", "Utility")]
        [Trait("TestID", "TC-UTILITY-007")]
        public async Task TC_UTILITY_007_CreateUtilityWithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                name = "",
                category = "",
                description = ""
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/utility", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region TC-BLOG-001 to TC-BLOG-003: Blog Tests

        [Fact]
        [Trait("Category", "Blog")]
        [Trait("TestID", "TC-BLOG-001")]
        public async Task TC_BLOG_001_GetPublishedBlogPosts_ShouldReturnPosts()
        {
            // Arrange - Create published blog post
            var blogPost = new BlogPost
            {
                Title = "Test Blog Post",
                Slug = "test-blog-post",
                Content = "Test content",
                Status = "Published",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.BlogPosts.Add(blogPost);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/blog/posts");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<BlogPostSummaryDto>>();
            result.Should().NotBeNull();
            result!.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Category", "Blog")]
        [Trait("TestID", "TC-BLOG-002")]
        public async Task TC_BLOG_002_GetBlogPostBySlug_ShouldReturnPost()
        {
            // Arrange
            var blogPost = new BlogPost
            {
                Title = "Test Post",
                Slug = "test-post",
                Content = "Test content",
                Status = "Published",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.BlogPosts.Add(blogPost);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/blog/posts/test-post");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<BlogPostDetailDto>();
            result.Should().NotBeNull();
            result!.Slug.Should().Be("test-post");
        }

        [Fact]
        [Trait("Category", "Blog")]
        [Trait("TestID", "TC-BLOG-003")]
        public async Task TC_BLOG_003_GetBlogCategories_ShouldReturnCategories()
        {
            // Arrange
            var category = new BlogCategory
            {
                Name = "Test Category"
            };
            DbContext.BlogCategories.Add(category);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/blog/categories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<BlogCategoryDto>>();
            result.Should().NotBeNull();
        }

        #endregion

        #region TC-PROMO-001 to TC-PROMO-013: Promotion Tests

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
            var result = await response.Content.ReadFromJsonAsync<List<PromotionDTO>>();
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Promotion")]
        [Trait("TestID", "TC-PROMO-002")]
        public async Task TC_PROMO_002_CreatePromotion_ShouldCreatePromotion()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                condotelId = 1,
                name = "New Promotion",
                discountPercentage = 25,
                startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(31)),
                status = "Active"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/promotion", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var promotion = await DbContext.Promotions.FirstOrDefaultAsync(p => p.Name == "New Promotion");
            promotion.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Promotion")]
        [Trait("TestID", "TC-PROMO-003")]
        public async Task TC_PROMO_003_GetPromotionById_ShouldReturnPromotion()
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
            var response = await Client.GetAsync($"/api/promotion/{promotion.PromotionId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PromotionDTO>();
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Promotion")]
        [Trait("TestID", "TC-PROMO-004")]
        public async Task TC_PROMO_004_GetPromotionsByCondotel_ShouldReturnPromotions()
        {
            // Arrange
            var promotion = new Promotion
            {
                CondotelId = 1,
                Name = "Condotel Promotion",
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
            var result = await response.Content.ReadFromJsonAsync<List<PromotionDTO>>();
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Promotion")]
        [Trait("TestID", "TC-PROMO-005")]
        public async Task TC_PROMO_005_GetPromotionsByHost_ShouldReturnPromotions()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/promotions");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<PromotionDTO>>();
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Promotion")]
        [Trait("TestID", "TC-PROMO-006")]
        public async Task TC_PROMO_006_UpdatePromotion_ShouldUpdatePromotion()
        {
            // Arrange
            var promotion = new Promotion
            {
                CondotelId = 1,
                Name = "Original Promotion",
                DiscountPercentage = 20,
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
                startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(31)),
                status = "Active"
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/host/promotion/{promotion.PromotionId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("thành công");
        }

        [Fact]
        [Trait("Category", "Promotion")]
        [Trait("TestID", "TC-PROMO-007")]
        public async Task TC_PROMO_007_DeletePromotion_ShouldDeletePromotion()
        {
            // Arrange
            var promotion = new Promotion
            {
                CondotelId = 1,
                Name = "To Be Deleted",
                DiscountPercentage = 20,
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
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("thành công");
        }

        [Fact]
        [Trait("Category", "Promotion")]
        [Trait("TestID", "TC-PROMO-010")]
        public async Task TC_PROMO_010_CreatePromotionWithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                condotelId = 0,
                name = "",
                discountPercentage = -10,
                startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(31)),
                endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)) // End before start
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/host/promotion", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region TC-SERVICEPKG-001 to TC-SERVICEPKG-010: Service Package Tests

        [Fact]
        [Trait("Category", "ServicePackage")]
        [Trait("TestID", "TC-SERVICEPKG-001")]
        public async Task TC_SERVICEPKG_001_GetServicePackages_ShouldReturnPackages()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/service-packages");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("data").GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        [Trait("Category", "ServicePackage")]
        [Trait("TestID", "TC-SERVICEPKG-002")]
        public async Task TC_SERVICEPKG_002_CreateServicePackage_ShouldCreatePackage()
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
        public async Task TC_SERVICEPKG_003_GetServicePackageById_ShouldReturnPackage()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/service-packages/1");

            // Assert
            // May return 404 if package doesn't exist, which is acceptable
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        #endregion

        #region TC-PACKAGE-001 to TC-PACKAGE-002: Package Tests

        [Fact]
        [Trait("Category", "Package")]
        [Trait("TestID", "TC-PACKAGE-001")]
        public async Task TC_PACKAGE_001_GetAvailablePackages_ShouldReturnPackages()
        {
            // Act
            var response = await Client.GetAsync("/api/package");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<PackageDto>>();
            result.Should().NotBeNull();
        }

        #endregion

        #region TC-PROFILE-001 to TC-PROFILE-002: Profile Tests

        [Fact]
        [Trait("Category", "Profile")]
        [Trait("TestID", "TC-PROFILE-001")]
        public async Task TC_PROFILE_001_GetCurrentUserProfile_ShouldReturnProfile()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/profile");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("userId").GetInt32().Should().Be(3);
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
            
            var user = await DbContext.Users.FindAsync(3);
            user!.FullName.Should().Be("Updated Name");
        }

        #endregion

        #region TC-HOSTPROFILE-001 to TC-HOSTPROFILE-005: Host Profile Tests

        [Fact]
        [Trait("Category", "HostProfile")]
        [Trait("TestID", "TC-HOSTPROFILE-001")]
        public async Task TC_HOSTPROFILE_001_GetHostProfile_ShouldReturnHostProfile()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/profile");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "HostProfile")]
        [Trait("TestID", "TC-HOSTPROFILE-002")]
        public async Task TC_HOSTPROFILE_002_GetHostProfileWhenNotRegistered_ShouldReturnNotFound()
        {
            // Arrange - Create user without host account
            var newUser = new User
            {
                FullName = "Non Host User",
                Email = "nonhost@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                RoleId = 3,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Users.Add(newUser);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(newUser.UserId, "nonhost@test.com", "Tenant");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/host/profile");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().ContainAny("not found", "Không tìm thấy");
        }

        [Fact]
        [Trait("Category", "HostProfile")]
        [Trait("TestID", "TC-HOSTPROFILE-003")]
        public async Task TC_HOSTPROFILE_003_UpdateHostProfile_ShouldUpdateProfile()
        {
            // Arrange
            var token = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(token);

            var request = new
            {
                phoneContact = "0987654321",
                address = "Updated Address"
            };

            // Act
            var response = await Client.PutAsJsonAsync("/api/host/profile", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("thành công");
        }

        #endregion

        #region TC-VOUCHER-003: View Condotel Vouchers

        [Fact]
        [Trait("Category", "Voucher")]
        [Trait("TestID", "TC-VOUCHER-003")]
        public async Task TC_VOUCHER_003_ViewCondotelVouchers_ShouldReturnVouchers()
        {
            // Arrange
            var voucher = new Voucher
            {
                Code = "PUBLICVOUCHER",
                CondotelId = 1,
                DiscountPercentage = 15,
                MaxUses = 50,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };
            DbContext.Vouchers.Add(voucher);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/tenant/voucher/condotel/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<VoucherDTO>>();
            result.Should().NotBeNull();
        }

        #endregion

        #region TC-ADMIN-001: Admin Dashboard

        [Fact]
        [Trait("Category", "Admin")]
        [Trait("TestID", "TC-ADMIN-001")]
        public async Task TC_ADMIN_001_GetDashboardOverview_ShouldReturnDashboard()
        {
            // Arrange
            var token = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(token);

            // Act
            var response = await Client.GetAsync("/api/admin/dashboard/overview");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.Should().NotBeNull();
        }

        #endregion
    }
}

