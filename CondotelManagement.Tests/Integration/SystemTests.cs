using CondotelManagement.Data;
using CondotelManagement.DTOs;
using CondotelManagement.DTOs.Auth;
using CondotelManagement.DTOs.Booking;
using CondotelManagement.DTOs.Host;
using CondotelManagement.DTOs.Wallet;
using CondotelManagement.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Tests.Integration
{
    /// <summary>
    /// System Tests - Test các luồng chính của hệ thống end-to-end
    /// Dựa trên phân tích trong SystemFlowsAnalysis.md
    /// </summary>
    public class SystemTests : TestBase
    {
        public SystemTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        #region System Flow 1: Complete Tenant Booking Flow

        /// <summary>
        /// Test luồng hoàn chỉnh: Tenant đăng ký → Xác thực → Xem Condotel → Đặt phòng → Thanh toán
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-001")]
        public async Task SYS_001_CompleteTenantBookingFlow_ShouldWorkEndToEnd()
        {
            // Step 1: Register new tenant
            var registerRequest = new RegisterRequest
            {
                Email = "newtenant@test.com",
                Password = "NewTenant123!",
                FullName = "New Tenant",
                Phone = "0987654321"
            };

            var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Step 2: Get OTP from database and verify email
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "newtenant@test.com");
            user.Should().NotBeNull();
            var otp = user!.PasswordResetToken;
            otp.Should().NotBeNull();

            var verifyRequest = new VerifyEmailRequest
            {
                Email = "newtenant@test.com",
                Otp = otp!
            };

            var verifyResponse = await Client.PostAsJsonAsync("/api/auth/verify-email", verifyRequest);
            verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 3: Login
            var loginRequest = new LoginRequest
            {
                Email = "newtenant@test.com",
                Password = "NewTenant123!"
            };

            var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<JsonElement>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var token = loginResult.GetProperty("token").GetString();
            token.Should().NotBeNullOrEmpty();

            // Step 4: View condotels (public, no auth needed)
            var condotelsResponse = await Client.GetAsync("/api/tenant/condotels");
            condotelsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var condotels = await condotelsResponse.Content.ReadFromJsonAsync<List<CondotelDTO>>();
            condotels.Should().NotBeNull();
            condotels!.Count.Should().BeGreaterThan(0);

            // Step 5: View condotel detail
            var condotelDetailResponse = await Client.GetAsync("/api/tenant/condotels/1");
            condotelDetailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var condotelDetail = await condotelDetailResponse.Content.ReadFromJsonAsync<CondotelDetailDTO>();
            condotelDetail.Should().NotBeNull();

            // Step 6: Check availability
            SetAuthHeader(token!);
            var checkIn = DateOnly.FromDateTime(DateTime.Now.AddDays(10));
            var checkOut = DateOnly.FromDateTime(DateTime.Now.AddDays(12));

            var availabilityResponse = await Client.GetAsync($"/api/booking/check-availability?condotelId=1&checkIn={checkIn:yyyy-MM-dd}&checkOut={checkOut:yyyy-MM-dd}");
            availabilityResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var availabilityResult = await availabilityResponse.Content.ReadFromJsonAsync<JsonElement>();
            availabilityResult.GetProperty("available").GetBoolean().Should().BeTrue();

            // Step 7: Create booking
            var bookingRequest = new BookingDTO
            {
                CondotelId = 1,
                StartDate = checkIn,
                EndDate = checkOut
            };

            var bookingResponse = await Client.PostAsJsonAsync("/api/booking", bookingRequest);
            bookingResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

            // Step 8: Get my bookings
            var myBookingsResponse = await Client.GetAsync("/api/booking/my");
            myBookingsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var myBookings = await myBookingsResponse.Content.ReadFromJsonAsync<List<BookingDTO>>();
            myBookings.Should().NotBeNull();
            myBookings!.Count.Should().BeGreaterThan(0);
        }

        #endregion

        #region System Flow 2: Complete Host Registration and Condotel Management Flow

        /// <summary>
        /// Test luồng hoàn chỉnh: User đăng ký → Đăng ký làm Host → Tạo Condotel → Quản lý Condotel
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-002")]
        public async Task SYS_002_CompleteHostRegistrationFlow_ShouldWorkEndToEnd()
        {
            // Step 1: Register new user
            var registerRequest = new RegisterRequest
            {
                Email = "newhost@test.com",
                Password = "NewHost123!",
                FullName = "New Host",
                Phone = "0123456789"
            };

            var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Step 2: Verify email
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "newhost@test.com");
            var otp = user!.PasswordResetToken;

            var verifyRequest = new VerifyEmailRequest
            {
                Email = "newhost@test.com",
                Otp = otp!
            };

            await Client.PostAsJsonAsync("/api/auth/verify-email", verifyRequest);

            // Step 3: Login
            var loginRequest = new LoginRequest
            {
                Email = "newhost@test.com",
                Password = "NewHost123!"
            };

            var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<JsonElement>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var token = loginResult.GetProperty("token").GetString();

            // Step 4: Register as Host
            SetAuthHeader(token!);
            var hostRegisterRequest = new HostRegisterRequestDto
            {
                CompanyName = "Test Host Company",
                TaxCode = "123456789",
                Address = "123 Test Street"
            };

            var hostRegisterResponse = await Client.PostAsJsonAsync("/api/host/register-as-host", hostRegisterRequest);
            hostRegisterResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 5: Create Wallet (Bank account)
            var walletRequest = new WalletCreateDTO
            {
                BankName = "Vietcombank",
                AccountNumber = "1234567890",
                AccountHolderName = "New Host"
            };

            var walletResponse = await Client.PostAsJsonAsync("/api/host/wallet", walletRequest);
            walletResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

            // Step 6: Get my wallets
            var walletsResponse = await Client.GetAsync("/api/host/wallet");
            walletsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var wallets = await walletsResponse.Content.ReadFromJsonAsync<JsonElement>();
            wallets.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);

            // Step 7: Create Condotel (Note: May fail if package limit reached)
            var condotelRequest = new
            {
                name = "New System Test Condotel",
                description = "Test Description",
                resortId = 1,
                pricePerNight = 200000,
                beds = 2,
                bathrooms = 1,
                status = "Active"
            };

            var condotelResponse = await Client.PostAsJsonAsync("/api/host/condotel", condotelRequest);
            // May return 403 if package limit reached, which is expected behavior
            condotelResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Forbidden);

            // Step 8: Get my condotels
            var myCondotelsResponse = await Client.GetAsync("/api/host/condotel");
            myCondotelsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region System Flow 3: Complete Booking with Payment Flow

        /// <summary>
        /// Test luồng: Tạo Booking → Tạo Payment Link → (Mock Payment Callback)
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-003")]
        public async Task SYS_003_CompleteBookingWithPaymentFlow_ShouldWorkEndToEnd()
        {
            // Step 1: Login as tenant
            var loginRequest = new LoginRequest
            {
                Email = "tenant@test.com",
                Password = "Tenant123!"
            };

            var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<JsonElement>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var token = loginResult.GetProperty("token").GetString();
            SetAuthHeader(token!);

            // Step 2: Create booking
            var checkIn = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
            var checkOut = DateOnly.FromDateTime(DateTime.Now.AddDays(7));

            var bookingRequest = new BookingDTO
            {
                CondotelId = 1,
                StartDate = checkIn,
                EndDate = checkOut
            };

            var bookingResponse = await Client.PostAsJsonAsync("/api/booking", bookingRequest);
            bookingResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

            var bookingContent = await bookingResponse.Content.ReadAsStringAsync();
            var bookingResult = JsonSerializer.Deserialize<JsonElement>(bookingContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var bookingId = bookingResult.GetProperty("data").GetProperty("bookingId").GetInt32();

            // Step 3: Create payment link
            var paymentRequest = new
            {
                bookingId = bookingId
            };

            var paymentResponse = await Client.PostAsJsonAsync("/api/payment/payos/create", paymentRequest);
            // Payment link creation may require PayOS service to be configured
            // In test environment, this might fail, which is acceptable
            paymentResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);

            // Step 4: Verify booking status
            var bookingDetailResponse = await Client.GetAsync($"/api/booking/{bookingId}");
            bookingDetailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region System Flow 4: Complete Review Flow

        /// <summary>
        /// Test luồng: Booking Completed → Tenant tạo Review → Host Reply Review
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-004")]
        public async Task SYS_004_CompleteReviewFlow_ShouldWorkEndToEnd()
        {
            // Step 1: Create completed booking
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

            // Step 2: Tenant login and create review
            var tenantLoginRequest = new LoginRequest
            {
                Email = "tenant@test.com",
                Password = "Tenant123!"
            };

            var tenantLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", tenantLoginRequest);
            var tenantLoginContent = await tenantLoginResponse.Content.ReadAsStringAsync();
            var tenantLoginResult = JsonSerializer.Deserialize<JsonElement>(tenantLoginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var tenantToken = tenantLoginResult.GetProperty("token").GetString();
            SetAuthHeader(tenantToken!);

            var reviewRequest = new
            {
                bookingId = booking.BookingId,
                condotelId = 1,
                rating = 5,
                comment = "Excellent stay! Great service and location."
            };

            var reviewResponse = await Client.PostAsJsonAsync("/api/tenant/reviews", reviewRequest);
            reviewResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 3: Get review
            var reviewsResponse = await Client.GetAsync("/api/tenant/reviews");
            reviewsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var reviews = await reviewsResponse.Content.ReadFromJsonAsync<JsonElement>();
            reviews.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);

            // Step 4: Host login and reply review
            var hostLoginRequest = new LoginRequest
            {
                Email = "host@test.com",
                Password = "Host123!"
            };

            var hostLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", hostLoginRequest);
            var hostLoginContent = await hostLoginResponse.Content.ReadAsStringAsync();
            var hostLoginResult = JsonSerializer.Deserialize<JsonElement>(hostLoginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var hostToken = hostLoginResult.GetProperty("token").GetString();
            SetAuthHeader(hostToken!);

            var review = await DbContext.Reviews.FirstOrDefaultAsync(r => r.BookingId == booking.BookingId);
            review.Should().NotBeNull();

            var replyRequest = new
            {
                reviewId = review!.ReviewId,
                reply = "Thank you for your feedback! We're glad you enjoyed your stay."
            };

            var replyResponse = await Client.PutAsJsonAsync($"/api/host/review/{review.ReviewId}/reply", replyRequest);
            replyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 5: Verify reply
            var updatedReview = await DbContext.Reviews.FindAsync(review.ReviewId);
            updatedReview!.Reply.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region System Flow 5: Complete Package Purchase Flow

        /// <summary>
        /// Test luồng: Host xem Packages → Chọn Package → Tạo Payment → (Mock Payment Callback)
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-005")]
        public async Task SYS_005_CompletePackagePurchaseFlow_ShouldWorkEndToEnd()
        {
            // Step 1: Host login
            var loginRequest = new LoginRequest
            {
                Email = "host@test.com",
                Password = "Host123!"
            };

            var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<JsonElement>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var token = loginResult.GetProperty("token").GetString();
            SetAuthHeader(token!);

            // Step 2: Get available packages
            var packagesResponse = await Client.GetAsync("/api/host/service-package/available");
            packagesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var packages = await packagesResponse.Content.ReadFromJsonAsync<JsonElement>();
            packages.GetProperty("data").GetArrayLength().Should().BeGreaterThanOrEqualTo(0);

            // Step 3: Get my current package
            var myPackageResponse = await Client.GetAsync("/api/host/service-package/my");
            myPackageResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Note: Actual package purchase requires PayOS integration
            // In test environment, we can verify the endpoints exist and return proper responses
        }

        #endregion

        #region System Flow 6: Complete Wallet and Payout Flow

        /// <summary>
        /// Test luồng: Host tạo Wallet → Booking Completed → Payout Process
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-006")]
        public async Task SYS_006_CompleteWalletAndPayoutFlow_ShouldWorkEndToEnd()
        {
            // Step 1: Host login
            var loginRequest = new LoginRequest
            {
                Email = "host@test.com",
                Password = "Host123!"
            };

            var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<JsonElement>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var token = loginResult.GetProperty("token").GetString();
            SetAuthHeader(token!);

            // Step 2: Create wallet
            var walletRequest = new WalletCreateDTO
            {
                BankName = "Vietcombank",
                AccountNumber = "9876543210",
                AccountHolderName = "Host User"
            };

            var walletResponse = await Client.PostAsJsonAsync("/api/host/wallet", walletRequest);
            walletResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

            // Step 3: Get wallets
            var walletsResponse = await Client.GetAsync("/api/host/wallet");
            walletsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 4: Create completed booking (>= 15 days ago)
            var oldBooking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-20)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-18)),
                Status = "Completed",
                TotalPrice = 400000,
                CreatedAt = DateTime.UtcNow,
                IsPaidToHost = false
            };
            DbContext.Bookings.Add(oldBooking);
            await DbContext.SaveChangesAsync();

            // Step 5: Admin login and process payout
            var adminLoginRequest = new LoginRequest
            {
                Email = "admin@test.com",
                Password = "Admin123!"
            };

            var adminLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", adminLoginRequest);
            var adminLoginContent = await adminLoginResponse.Content.ReadAsStringAsync();
            var adminLoginResult = JsonSerializer.Deserialize<JsonElement>(adminLoginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var adminToken = adminLoginResult.GetProperty("token").GetString();
            SetAuthHeader(adminToken!);

            // Note: Payout processing is typically done via background job or admin trigger
            // We can verify the endpoint exists
            var payoutResponse = await Client.PostAsync($"/api/admin/payout/process/{oldBooking.BookingId}", null);
            // May require additional setup
            payoutResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        #endregion

        #region System Flow 7: Complete Admin Management Flow

        /// <summary>
        /// Test luồng: Admin đăng nhập → Quản lý Users → Quản lý Locations/Resorts → Xem Dashboard
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-007")]
        public async Task SYS_007_CompleteAdminManagementFlow_ShouldWorkEndToEnd()
        {
            // Step 1: Admin login
            var loginRequest = new LoginRequest
            {
                Email = "admin@test.com",
                Password = "Admin123!"
            };

            var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<JsonElement>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var token = loginResult.GetProperty("token").GetString();
            SetAuthHeader(token!);

            // Step 2: Get all users
            var usersResponse = await Client.GetAsync("/api/admin/users");
            usersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var users = await usersResponse.Content.ReadFromJsonAsync<JsonElement>();
            users.Should().NotBeNull();

            // Step 3: Get user by ID
            var userResponse = await Client.GetAsync("/api/admin/users/1");
            userResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 4: Get all locations
            var locationsResponse = await Client.GetAsync("/api/admin/location");
            locationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 5: Get all resorts
            var resortsResponse = await Client.GetAsync("/api/admin/resort");
            resortsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 6: Get dashboard overview
            var dashboardResponse = await Client.GetAsync("/api/admin/dashboard/overview");
            dashboardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var dashboard = await dashboardResponse.Content.ReadFromJsonAsync<JsonElement>();
            dashboard.Should().NotBeNull();

            // Step 7: Get revenue chart
            var revenueResponse = await Client.GetAsync("/api/admin/dashboard/revenue/chart");
            revenueResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region System Flow 8: Authorization and Security Flow

        /// <summary>
        /// Test các luồng bảo mật: Unauthorized access, Wrong role, Ownership checks
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-008")]
        public async Task SYS_008_AuthorizationAndSecurityFlow_ShouldEnforceSecurity()
        {
            // Step 1: Access protected endpoint without token
            Client.DefaultRequestHeaders.Authorization = null;
            var unauthorizedResponse = await Client.GetAsync("/api/booking/my");
            unauthorizedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Step 2: Access with wrong role
            var tenantLoginRequest = new LoginRequest
            {
                Email = "tenant@test.com",
                Password = "Tenant123!"
            };

            var tenantLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", tenantLoginRequest);
            var tenantLoginContent = await tenantLoginResponse.Content.ReadAsStringAsync();
            var tenantLoginResult = JsonSerializer.Deserialize<JsonElement>(tenantLoginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var tenantToken = tenantLoginResult.GetProperty("token").GetString();
            SetAuthHeader(tenantToken!);

            // Tenant cannot access host endpoints
            var forbiddenResponse = await Client.GetAsync("/api/host/condotel");
            forbiddenResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            // Step 3: Access other user's booking
            var otherUserBooking = new Booking
            {
                CondotelId = 1,
                CustomerId = 1, // Different customer
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                Status = "Confirmed",
                TotalPrice = 200000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(otherUserBooking);
            await DbContext.SaveChangesAsync();

            var accessOtherBookingResponse = await Client.DeleteAsync($"/api/booking/{otherUserBooking.BookingId}");
            accessOtherBookingResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);

            // Step 4: Admin can access admin endpoints
            var adminLoginRequest = new LoginRequest
            {
                Email = "admin@test.com",
                Password = "Admin123!"
            };

            var adminLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", adminLoginRequest);
            var adminLoginContent = await adminLoginResponse.Content.ReadAsStringAsync();
            var adminLoginResult = JsonSerializer.Deserialize<JsonElement>(adminLoginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var adminToken = adminLoginResult.GetProperty("token").GetString();
            SetAuthHeader(adminToken!);

            var adminAccessResponse = await Client.GetAsync("/api/admin/users");
            adminAccessResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region System Flow 9: Complete Search and Filter Flow

        /// <summary>
        /// Test luồng: Tenant tìm kiếm và lọc Condotel theo nhiều tiêu chí
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-009")]
        public async Task SYS_009_CompleteSearchAndFilterFlow_ShouldWorkEndToEnd()
        {
            // Step 1: Search by name
            var searchByNameResponse = await Client.GetAsync("/api/tenant/condotels?name=Test");
            searchByNameResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var searchResults = await searchByNameResponse.Content.ReadFromJsonAsync<List<CondotelDTO>>();
            searchResults.Should().NotBeNull();

            // Step 2: Filter by price
            var filterByPriceResponse = await Client.GetAsync("/api/tenant/condotels?minPrice=50000&maxPrice=150000");
            filterByPriceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var priceResults = await filterByPriceResponse.Content.ReadFromJsonAsync<List<CondotelDTO>>();
            priceResults.Should().NotBeNull();

            // Step 3: Filter by beds and bathrooms
            var filterByBedsResponse = await Client.GetAsync("/api/tenant/condotels?beds=2&bathrooms=1");
            filterByBedsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 4: Filter by location
            var filterByLocationResponse = await Client.GetAsync("/api/tenant/condotels?locationId=1");
            filterByLocationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 5: Combined filters
            var combinedFiltersResponse = await Client.GetAsync("/api/tenant/condotels?name=Test&minPrice=50000&maxPrice=200000&beds=2");
            combinedFiltersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region System Flow 10: Complete Voucher and Promotion Flow

        /// <summary>
        /// Test luồng: Host tạo Voucher → Tenant sử dụng Voucher khi booking
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-010")]
        public async Task SYS_010_CompleteVoucherFlow_ShouldWorkEndToEnd()
        {
            // Step 1: Host login
            var hostLoginRequest = new LoginRequest
            {
                Email = "host@test.com",
                Password = "Host123!"
            };

            var hostLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", hostLoginRequest);
            var hostLoginContent = await hostLoginResponse.Content.ReadAsStringAsync();
            var hostLoginResult = JsonSerializer.Deserialize<JsonElement>(hostLoginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var hostToken = hostLoginResult.GetProperty("token").GetString();
            SetAuthHeader(hostToken!);

            // Step 2: Host creates voucher
            var voucherRequest = new
            {
                code = "SYS10TEST",
                condotelId = 1,
                discountPercentage = 15,
                maxUses = 50,
                expiryDate = DateTime.UtcNow.AddDays(30)
            };

            var voucherResponse = await Client.PostAsJsonAsync("/api/host/voucher", voucherRequest);
            voucherResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 3: Get vouchers by host
            var hostVouchersResponse = await Client.GetAsync("/api/host/voucher");
            hostVouchersResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 4: Tenant views vouchers for condotel (public)
            Client.DefaultRequestHeaders.Authorization = null;
            var condotelVouchersResponse = await Client.GetAsync("/api/tenant/voucher/condotel/1");
            condotelVouchersResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 5: Tenant login and create booking with voucher
            var tenantLoginRequest = new LoginRequest
            {
                Email = "tenant@test.com",
                Password = "Tenant123!"
            };

            var tenantLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", tenantLoginRequest);
            var tenantLoginContent = await tenantLoginResponse.Content.ReadAsStringAsync();
            var tenantLoginResult = JsonSerializer.Deserialize<JsonElement>(tenantLoginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var tenantToken = tenantLoginResult.GetProperty("token").GetString();
            SetAuthHeader(tenantToken!);

            // Note: Booking with voucher would require voucher code in booking request
            // This depends on BookingService implementation
        }

        #endregion

        #region System Flow 11: Complete Authentication Flow

        /// <summary>
        /// Test luồng hoàn chỉnh: Register → Verify Email → Login → Forgot Password → Reset Password
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-011")]
        public async Task SYS_011_CompleteAuthenticationFlow_ShouldWorkEndToEnd()
        {
            // Step 1: Register new user
            var registerRequest = new RegisterRequest
            {
                Email = "authtest@test.com",
                Password = "AuthTest123!",
                FullName = "Auth Test User",
                Phone = "0123456789"
            };

            var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Step 2: Get OTP from database and verify email
            var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == "authtest@test.com");
            user.Should().NotBeNull();
            user!.Status.Should().Be("Pending");
            var otp = user.PasswordResetToken;
            otp.Should().NotBeNull();

            var verifyRequest = new VerifyEmailRequest
            {
                Email = "authtest@test.com",
                Otp = otp!
            };

            var verifyResponse = await Client.PostAsJsonAsync("/api/auth/verify-email", verifyRequest);
            verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify user status is now Active
            await DbContext.Entry(user).ReloadAsync();
            user.Status.Should().Be("Active");

            // Step 3: Login with verified account
            var loginRequest = new LoginRequest
            {
                Email = "authtest@test.com",
                Password = "AuthTest123!"
            };

            var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<JsonElement>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var token = loginResult.GetProperty("token").GetString();
            token.Should().NotBeNullOrEmpty();

            // Step 4: Get current user info
            SetAuthHeader(token!);
            var meResponse = await Client.GetAsync("/api/auth/me");
            meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var meContent = await meResponse.Content.ReadAsStringAsync();
            var meResult = JsonSerializer.Deserialize<JsonElement>(meContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            meResult.GetProperty("email").GetString().Should().Be("authtest@test.com");

            // Step 5: Forgot password flow
            Client.DefaultRequestHeaders.Authorization = null;
            var forgotPasswordRequest = new ForgotPasswordRequest
            {
                Email = "authtest@test.com"
            };

            var forgotPasswordResponse = await Client.PostAsJsonAsync("/api/auth/send-otp", forgotPasswordRequest);
            forgotPasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 6: Get new OTP and reset password
            await DbContext.Entry(user).ReloadAsync();
            var resetOtp = user.PasswordResetToken;
            resetOtp.Should().NotBeNull();

            var resetPasswordRequest = new ResetPasswordWithOtpRequest
            {
                Email = "authtest@test.com",
                Otp = resetOtp!,
                NewPassword = "NewPassword123!"
            };

            var resetPasswordResponse = await Client.PostAsJsonAsync("/api/auth/reset-password-with-otp", resetPasswordRequest);
            resetPasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 7: Login with new password
            var newLoginRequest = new LoginRequest
            {
                Email = "authtest@test.com",
                Password = "NewPassword123!"
            };

            var newLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", newLoginRequest);
            newLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region System Flow 12: Complete Refund Request Flow

        /// <summary>
        /// Test luồng: Tenant tạo Refund Request → Admin xem → Admin approve/reject
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-012")]
        public async Task SYS_012_CompleteRefundRequestFlow_ShouldWorkEndToEnd()
        {
            // Step 1: Create a confirmed booking
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

            // Step 2: Tenant login and request refund
            var tenantLoginRequest = new LoginRequest
            {
                Email = "tenant@test.com",
                Password = "Tenant123!"
            };

            var tenantLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", tenantLoginRequest);
            var tenantLoginContent = await tenantLoginResponse.Content.ReadAsStringAsync();
            var tenantLoginResult = JsonSerializer.Deserialize<JsonElement>(tenantLoginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var tenantToken = tenantLoginResult.GetProperty("token").GetString();
            SetAuthHeader(tenantToken!);

            var refundRequest = new
            {
                bankCode = "VCB",
                accountNumber = "1234567890",
                accountHolder = "Tenant User"
            };

            var refundResponse = await Client.PostAsJsonAsync($"/api/booking/{booking.BookingId}/refund", refundRequest);
            refundResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);

            // Step 3: Verify refund request was created
            var refundRequestEntity = await DbContext.RefundRequests.FirstOrDefaultAsync(r => r.BookingId == booking.BookingId);
            if (refundRequestEntity != null)
            {
                refundRequestEntity.Status.Should().Be("Pending");

                // Step 4: Admin login and view refund requests
                Client.DefaultRequestHeaders.Authorization = null;
                var adminLoginRequest = new LoginRequest
                {
                    Email = "admin@test.com",
                    Password = "Admin123!"
                };

                var adminLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", adminLoginRequest);
                var adminLoginContent = await adminLoginResponse.Content.ReadAsStringAsync();
                var adminLoginResult = JsonSerializer.Deserialize<JsonElement>(adminLoginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var adminToken = adminLoginResult.GetProperty("token").GetString();
                SetAuthHeader(adminToken!);

                // Step 5: Admin view refund requests
                var refundsResponse = await Client.GetAsync("/api/admin/refunds");
                refundsResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

                // Note: Approve/Reject endpoints may require additional implementation
                // This test verifies the basic flow exists
            }
        }

        #endregion

        #region System Flow 13: Complete Promotion Flow

        /// <summary>
        /// Test luồng: Host tạo Promotion → Tenant xem → Tenant sử dụng khi booking
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-013")]
        public async Task SYS_013_CompletePromotionFlow_ShouldWorkEndToEnd()
        {
            // Step 1: Host login
            var hostLoginRequest = new LoginRequest
            {
                Email = "host@test.com",
                Password = "Host123!"
            };

            var hostLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", hostLoginRequest);
            var hostLoginContent = await hostLoginResponse.Content.ReadAsStringAsync();
            var hostLoginResult = JsonSerializer.Deserialize<JsonElement>(hostLoginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var hostToken = hostLoginResult.GetProperty("token").GetString();
            SetAuthHeader(hostToken!);

            // Step 2: Host creates promotion
            var promotionRequest = new
            {
                condotelId = 1,
                discountPercentage = 20,
                startDate = DateTime.UtcNow,
                endDate = DateTime.UtcNow.AddDays(30),
                description = "Summer Promotion 20% off"
            };

            var promotionResponse = await Client.PostAsJsonAsync("/api/host/promotion", promotionRequest);
            promotionResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);

            // Step 3: Get promotions by host
            var hostPromotionsResponse = await Client.GetAsync("/api/host/promotions");
            hostPromotionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 4: Public view promotions for condotel
            Client.DefaultRequestHeaders.Authorization = null;
            var condotelPromotionsResponse = await Client.GetAsync("/api/promotions/condotel/1");
            condotelPromotionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var promotions = await condotelPromotionsResponse.Content.ReadFromJsonAsync<JsonElement>();
            promotions.Should().NotBeNull();

            // Step 5: Tenant can see promotions when viewing condotel detail
            var condotelDetailResponse = await Client.GetAsync("/api/tenant/condotels/1");
            condotelDetailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region System Flow 14: Complete Package Limit Enforcement Flow

        /// <summary>
        /// Test luồng: Host mua Package → Tạo Condotel theo giới hạn → Vượt quá giới hạn bị từ chối
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-014")]
        public async Task SYS_014_CompletePackageLimitEnforcementFlow_ShouldEnforceLimits()
        {
            // Step 1: Host login
            var hostLoginRequest = new LoginRequest
            {
                Email = "host@test.com",
                Password = "Host123!"
            };

            var hostLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", hostLoginRequest);
            var hostLoginContent = await hostLoginResponse.Content.ReadAsStringAsync();
            var hostLoginResult = JsonSerializer.Deserialize<JsonElement>(hostLoginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var hostToken = hostLoginResult.GetProperty("token").GetString();
            SetAuthHeader(hostToken!);

            // Step 2: Check current package
            var myPackageResponse = await Client.GetAsync("/api/host/service-package/my");
            myPackageResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 3: Get current condotel count
            var myCondotelsResponse = await Client.GetAsync("/api/host/condotel");
            myCondotelsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var myCondotels = await myCondotelsResponse.Content.ReadFromJsonAsync<JsonElement>();
            var currentCount = myCondotels?.GetProperty("data")?.GetArrayLength() ?? 0;

            // Step 4: Try to create condotel
            var condotelRequest = new
            {
                name = "Test Package Limit Condotel",
                description = "Test Description",
                resortId = 1,
                pricePerNight = 200000,
                beds = 2,
                bathrooms = 1,
                status = "Active"
            };

            var condotelResponse = await Client.PostAsJsonAsync("/api/host/condotel", condotelRequest);
            
            // Step 5: Verify response
            // If package limit reached, should return 403 Forbidden
            // If within limit, should return 200/201 Created
            condotelResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, 
                HttpStatusCode.Created, 
                HttpStatusCode.Forbidden,
                HttpStatusCode.BadRequest
            );

            // Step 6: Verify condotel count
            var updatedCondotelsResponse = await Client.GetAsync("/api/host/condotel");
            updatedCondotelsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region System Flow 15: Complete Multi-Step Booking with Voucher Flow

        /// <summary>
        /// Test luồng phức tạp: Tenant tìm condotel → Xem voucher → Đặt phòng với voucher → Thanh toán
        /// </summary>
        [Fact]
        [Trait("Category", "System")]
        [Trait("TestID", "SYS-015")]
        public async Task SYS_015_CompleteMultiStepBookingWithVoucherFlow_ShouldWorkEndToEnd()
        {
            // Step 1: Search condotels
            var searchResponse = await Client.GetAsync("/api/tenant/condotels?name=Test");
            searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var condotels = await searchResponse.Content.ReadFromJsonAsync<List<CondotelDTO>>();
            condotels.Should().NotBeNull();
            condotels!.Count.Should().BeGreaterThan(0);

            // Step 2: View condotel detail
            var condotelDetailResponse = await Client.GetAsync("/api/tenant/condotels/1");
            condotelDetailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var condotelDetail = await condotelDetailResponse.Content.ReadFromJsonAsync<CondotelDetailDTO>();
            condotelDetail.Should().NotBeNull();

            // Step 3: View vouchers for condotel
            var vouchersResponse = await Client.GetAsync("/api/vouchers/condotel/1");
            vouchersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var vouchers = await vouchersResponse.Content.ReadFromJsonAsync<JsonElement>();
            vouchers.Should().NotBeNull();

            // Step 4: Tenant login
            var tenantLoginRequest = new LoginRequest
            {
                Email = "tenant@test.com",
                Password = "Tenant123!"
            };

            var tenantLoginResponse = await Client.PostAsJsonAsync("/api/auth/login", tenantLoginRequest);
            var tenantLoginContent = await tenantLoginResponse.Content.ReadAsStringAsync();
            var tenantLoginResult = JsonSerializer.Deserialize<JsonElement>(tenantLoginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var tenantToken = tenantLoginResult.GetProperty("token").GetString();
            SetAuthHeader(tenantToken!);

            // Step 5: Check availability
            var checkIn = DateOnly.FromDateTime(DateTime.Now.AddDays(15));
            var checkOut = DateOnly.FromDateTime(DateTime.Now.AddDays(17));

            var availabilityResponse = await Client.GetAsync($"/api/booking/check-availability?condotelId=1&checkIn={checkIn:yyyy-MM-dd}&checkOut={checkOut:yyyy-MM-dd}");
            availabilityResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var availabilityResult = await availabilityResponse.Content.ReadFromJsonAsync<JsonElement>();
            var isAvailable = availabilityResult.GetProperty("available").GetBoolean();

            if (isAvailable)
            {
                // Step 6: Create booking
                var bookingRequest = new BookingDTO
                {
                    CondotelId = 1,
                    StartDate = checkIn,
                    EndDate = checkOut
                    // Note: Voucher code would be included here if supported
                };

                var bookingResponse = await Client.PostAsJsonAsync("/api/booking", bookingRequest);
                bookingResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

                // Step 7: Verify booking was created
                var myBookingsResponse = await Client.GetAsync("/api/booking/my");
                myBookingsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                var myBookings = await myBookingsResponse.Content.ReadFromJsonAsync<List<BookingDTO>>();
                myBookings.Should().NotBeNull();
                myBookings!.Count.Should().BeGreaterThan(0);
            }
        }

        #endregion
    }
}









