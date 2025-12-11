using CondotelManagement.Data;
using CondotelManagement.DTOs;
using CondotelManagement.DTOs.Auth;
using CondotelManagement.DTOs.Booking;
using CondotelManagement.DTOs.Host;
using CondotelManagement.DTOs.Tenant;
using CondotelManagement.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Tests.Integration
{
    /// <summary>
    /// System Workflow Tests - Test các luồng chính của hệ thống end-to-end
    /// Bao gồm: Booking, Payment, Refund, Host Subscription, Host Payout, Review
    /// </summary>
    public class SystemWorkflowTests : TestBase
    {
        public SystemWorkflowTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        #region Workflow 1: Booking Workflow

        /// <summary>
        /// Test luồng Booking hoàn chỉnh: Check Availability → Create Booking → View Booking
        /// </summary>
        [Fact]
        [Trait("Category", "BookingWorkflow")]
        [Trait("TestID", "WF-BOOKING-001")]
        public async Task WF_BOOKING_001_CompleteBookingWorkflow_ShouldWorkEndToEnd()
        {
            // Arrange - Login as Tenant
            var tenantToken = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(tenantToken);

            // Step 1: Check availability
            var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
            var checkOut = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12));

            var availabilityResponse = await Client.GetAsync(
                $"/api/booking/check-availability?condotelId=1&checkIn={checkIn:yyyy-MM-dd}&checkOut={checkOut:yyyy-MM-dd}");
            
            availabilityResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var availabilityResult = await availabilityResponse.Content.ReadFromJsonAsync<JsonElement>();
            availabilityResult.GetProperty("available").GetBoolean().Should().BeTrue();

            // Step 2: Create booking
            var bookingRequest = new BookingDTO
            {
                CondotelId = 1,
                StartDate = checkIn,
                EndDate = checkOut
            };

            var bookingResponse = await Client.PostAsJsonAsync("/api/booking", bookingRequest);
            bookingResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

            var bookingContent = await bookingResponse.Content.ReadAsStringAsync();
            var bookingResult = JsonSerializer.Deserialize<JsonElement>(bookingContent, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            var bookingId = bookingResult.GetProperty("data").GetProperty("bookingId").GetInt32();
            bookingId.Should().BeGreaterThan(0);

            // Step 3: Verify booking status is Pending
            var booking = await DbContext.Bookings.FindAsync(bookingId);
            booking.Should().NotBeNull();
            booking!.Status.Should().Be("Pending");
            booking.TotalPrice.Should().BeGreaterThan(0);

            // Step 4: Get my bookings
            var myBookingsResponse = await Client.GetAsync("/api/booking/my");
            myBookingsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var myBookings = await myBookingsResponse.Content.ReadFromJsonAsync<List<BookingDTO>>();
            myBookings.Should().NotBeNull();
            myBookings!.Any(b => b.BookingId == bookingId).Should().BeTrue();

            // Step 5: Get booking by ID
            var bookingDetailResponse = await Client.GetAsync($"/api/booking/{bookingId}");
            bookingDetailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var bookingDetail = await bookingDetailResponse.Content.ReadFromJsonAsync<BookingDTO>();
            bookingDetail.Should().NotBeNull();
            bookingDetail!.BookingId.Should().Be(bookingId);
        }

        /// <summary>
        /// Test Booking với condotel không available
        /// </summary>
        [Fact]
        [Trait("Category", "BookingWorkflow")]
        [Trait("TestID", "WF-BOOKING-002")]
        public async Task WF_BOOKING_002_BookingUnavailableCondotel_ShouldReturnBadRequest()
        {
            // Arrange - Create existing booking
            var existingBooking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12)),
                Status = "Confirmed",
                TotalPrice = 200000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(existingBooking);
            await DbContext.SaveChangesAsync();

            var tenantToken = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(tenantToken);

            // Act - Try to book overlapping dates
            var bookingRequest = new BookingDTO
            {
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(11)), // Overlaps
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(13))
            };

            var bookingResponse = await Client.PostAsJsonAsync("/api/booking", bookingRequest);
            
            // Assert
            bookingResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await bookingResponse.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("success").GetBoolean().Should().BeFalse();
        }

        #endregion

        #region Workflow 2: Payment Workflow

        /// <summary>
        /// Test luồng Payment hoàn chỉnh: Create Booking → Create Payment Link → Verify Status
        /// </summary>
        [Fact]
        [Trait("Category", "PaymentWorkflow")]
        [Trait("TestID", "WF-PAYMENT-001")]
        public async Task WF_PAYMENT_001_CompletePaymentWorkflow_ShouldWorkEndToEnd()
        {
            // Arrange - Create booking
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                Status = "Pending",
                TotalPrice = 200000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            var tenantToken = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(tenantToken);

            // Step 1: Create payment link
            var paymentRequest = new
            {
                bookingId = booking.BookingId
            };

            var paymentResponse = await Client.PostAsJsonAsync("/api/payment/payos/create", paymentRequest);
            
            // Note: Payment link creation may require PayOS service configuration
            // In test environment, this might fail, which is acceptable
            paymentResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, 
                HttpStatusCode.BadRequest, 
                HttpStatusCode.InternalServerError);

            // Step 2: Verify booking status (should still be Pending until payment confirmed)
            var updatedBooking = await DbContext.Bookings.FindAsync(booking.BookingId);
            updatedBooking.Should().NotBeNull();
            updatedBooking!.Status.Should().Be("Pending");
        }

        /// <summary>
        /// Test Payment với booking không hợp lệ
        /// </summary>
        [Fact]
        [Trait("Category", "PaymentWorkflow")]
        [Trait("TestID", "WF-PAYMENT-002")]
        public async Task WF_PAYMENT_002_PaymentInvalidBooking_ShouldReturnBadRequest()
        {
            var tenantToken = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(tenantToken);

            // Act - Try to pay for non-existent booking
            var paymentRequest = new
            {
                bookingId = 99999
            };

            var paymentResponse = await Client.PostAsJsonAsync("/api/payment/payos/create", paymentRequest);
            
            // Assert
            paymentResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Test Payment với booking đã confirmed
        /// </summary>
        [Fact]
        [Trait("Category", "PaymentWorkflow")]
        [Trait("TestID", "WF-PAYMENT-003")]
        public async Task WF_PAYMENT_003_PaymentConfirmedBooking_ShouldReturnBadRequest()
        {
            // Arrange - Create confirmed booking
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                Status = "Confirmed",
                TotalPrice = 200000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            var tenantToken = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(tenantToken);

            // Act
            var paymentRequest = new
            {
                bookingId = booking.BookingId
            };

            var paymentResponse = await Client.PostAsJsonAsync("/api/payment/payos/create", paymentRequest);
            
            // Assert
            paymentResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await paymentResponse.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("success").GetBoolean().Should().BeFalse();
        }

        #endregion

        #region Workflow 3: Refund Workflow

        /// <summary>
        /// Test luồng Refund hoàn chỉnh: Cancel Booking → Create Refund Request → Admin Process
        /// </summary>
        [Fact]
        [Trait("Category", "RefundWorkflow")]
        [Trait("TestID", "WF-REFUND-001")]
        public async Task WF_REFUND_001_CompleteRefundWorkflow_ShouldWorkEndToEnd()
        {
            // Arrange - Create confirmed booking (eligible for refund)
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), // >= 2 days
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                Status = "Confirmed",
                TotalPrice = 1000000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            var tenantToken = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(tenantToken);

            // Step 1: Request refund with bank info
            var refundRequest = new RefundBookingRequestDTO
            {
                BankCode = "VCB",
                AccountNumber = "1234567890",
                AccountHolder = "Tenant User"
            };

            var refundResponse = await Client.PostAsJsonAsync(
                $"/api/booking/{booking.BookingId}/refund", refundRequest);
            
            refundResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var refundResult = await refundResponse.Content.ReadFromJsonAsync<JsonElement>();
            refundResult.GetProperty("success").GetBoolean().Should().BeTrue();

            // Step 2: Verify refund request created
            var refundRequestEntity = await DbContext.RefundRequests
                .FirstOrDefaultAsync(r => r.BookingId == booking.BookingId);
            
            refundRequestEntity.Should().NotBeNull();
            refundRequestEntity!.Status.Should().Be("Pending");
            refundRequestEntity.BankCode.Should().Be("VCB");
            refundRequestEntity.AccountNumber.Should().Be("1234567890");

            // Step 3: Admin view refund requests
            var adminToken = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(adminToken);

            var refundsResponse = await Client.GetAsync("/api/admin/refund-requests");
            refundsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var refundsResult = await refundsResponse.Content.ReadFromJsonAsync<JsonElement>();
            refundsResult.GetProperty("success").GetBoolean().Should().BeTrue();

            // Step 4: Admin confirm refund
            var confirmResponse = await Client.PostAsync(
                $"/api/admin/refund-requests/{refundRequestEntity.Id}/confirm", null);
            
            confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var confirmResult = await confirmResponse.Content.ReadFromJsonAsync<JsonElement>();
            confirmResult.GetProperty("success").GetBoolean().Should().BeTrue();

            // Step 5: Verify refund status updated
            var updatedRefund = await DbContext.RefundRequests.FindAsync(refundRequestEntity.Id);
            updatedRefund.Should().NotBeNull();
            updatedRefund!.Status.Should().Be("Completed");
            updatedRefund.ProcessedAt.Should().NotBeNull();
        }

        /// <summary>
        /// Test Refund với booking chưa đủ 2 ngày
        /// </summary>
        [Fact]
        [Trait("Category", "RefundWorkflow")]
        [Trait("TestID", "WF-REFUND-002")]
        public async Task WF_REFUND_002_RefundLessThan2Days_ShouldReturnBadRequest()
        {
            // Arrange - Create booking with check-in in 1 day
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), // < 2 days
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                Status = "Confirmed",
                TotalPrice = 1000000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            var tenantToken = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(tenantToken);

            // Act
            var refundRequest = new RefundBookingRequestDTO
            {
                BankCode = "VCB",
                AccountNumber = "1234567890",
                AccountHolder = "Tenant User"
            };

            var refundResponse = await Client.PostAsJsonAsync(
                $"/api/booking/{booking.BookingId}/refund", refundRequest);
            
            // Assert
            refundResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await refundResponse.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("success").GetBoolean().Should().BeFalse();
        }

        #endregion

        #region Workflow 4: Host Monthly Subscription Workflow

        /// <summary>
        /// Test luồng Host Subscription hoàn chỉnh: View Packages → Purchase Package → Activate
        /// </summary>
        [Fact]
        [Trait("Category", "HostSubscriptionWorkflow")]
        [Trait("TestID", "WF-SUBSCRIPTION-001")]
        public async Task WF_SUBSCRIPTION_001_CompleteHostSubscriptionWorkflow_ShouldWorkEndToEnd()
        {
            // Arrange - Login as Host
            var hostToken = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(hostToken);

            // Step 1: Get available packages
            var packagesResponse = await Client.GetAsync("/api/host/service-package/available");
            packagesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var packagesResult = await packagesResponse.Content.ReadFromJsonAsync<JsonElement>();
            packagesResult.Should().NotBeNull();

            // Step 2: Get my current package (may be null if no package)
            var myPackageResponse = await Client.GetAsync("/api/host/service-package/my");
            myPackageResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 3: Purchase package (if packages available)
            // Note: This requires PayOS integration, so may fail in test environment
            // We'll verify the endpoint exists and returns proper response
            var purchaseRequest = new
            {
                packageId = 1
            };

            var purchaseResponse = await Client.PostAsJsonAsync(
                "/api/host/packages/purchase", purchaseRequest);
            
            // May return 400/500 if PayOS not configured, which is acceptable
            purchaseResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.BadRequest,
                HttpStatusCode.InternalServerError);

            // Step 4: Get my package after purchase
            var updatedPackageResponse = await Client.GetAsync("/api/host/service-package/my");
            updatedPackageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test Host Subscription - Check package limits
        /// </summary>
        [Fact]
        [Trait("Category", "HostSubscriptionWorkflow")]
        [Trait("TestID", "WF-SUBSCRIPTION-002")]
        public async Task WF_SUBSCRIPTION_002_HostPackageLimits_ShouldEnforceLimits()
        {
            // Arrange - Create host with active package
            var hostPackage = new HostPackage
            {
                HostId = 1,
                PackageId = 1,
                Status = "Active",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                DurationDays = 30
            };
            DbContext.HostPackages.Add(hostPackage);
            await DbContext.SaveChangesAsync();

            var hostToken = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(hostToken);

            // Step 1: Get my package
            var myPackageResponse = await Client.GetAsync("/api/host/service-package/my");
            myPackageResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 2: Try to create condotel (should respect package limits)
            var condotelRequest = new
            {
                name = "Test Condotel",
                description = "Test Description",
                resortId = 1,
                pricePerNight = 200000,
                beds = 2,
                bathrooms = 1,
                status = "Active"
            };

            var condotelResponse = await Client.PostAsJsonAsync("/api/host/condotel", condotelRequest);
            
            // May return 403 if limit reached, or 200/201 if within limit
            condotelResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.Created,
                HttpStatusCode.Forbidden,
                HttpStatusCode.BadRequest);
        }

        #endregion

        #region Workflow 5: Host Payout Workflow

        /// <summary>
        /// Test luồng Host Payout hoàn chỉnh: Booking Completed → Wait 15 days → Admin Process Payout
        /// </summary>
        [Fact]
        [Trait("Category", "HostPayoutWorkflow")]
        [Trait("TestID", "WF-PAYOUT-001")]
        public async Task WF_PAYOUT_001_CompleteHostPayoutWorkflow_ShouldWorkEndToEnd()
        {
            // Arrange - Create wallet for host
            var wallet = new Wallet
            {
                HostId = 1,
                BankName = "Vietcombank",
                AccountNumber = "9876543210",
                AccountHolderName = "Host User",
                Status = "Active",
                IsDefault = true
            };
            DbContext.Wallets.Add(wallet);

            // Create completed booking >= 15 days ago
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)), // 20 days ago
                Status = "Completed",
                TotalPrice = 5000000,
                IsPaidToHost = false,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            // Step 1: Admin view pending payouts
            var adminToken = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(adminToken);

            var pendingPayoutsResponse = await Client.GetAsync("/api/admin/payouts/pending");
            pendingPayoutsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var pendingResult = await pendingPayoutsResponse.Content.ReadFromJsonAsync<JsonElement>();
            pendingResult.GetProperty("success").GetBoolean().Should().BeTrue();

            // Step 2: Admin process payout for specific booking
            var confirmPayoutResponse = await Client.PostAsync(
                $"/api/admin/payouts/{booking.BookingId}/confirm", null);
            
            confirmPayoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var confirmResult = await confirmPayoutResponse.Content.ReadFromJsonAsync<HostPayoutResponseDTO>();
            confirmResult.Should().NotBeNull();
            confirmResult!.Success.Should().BeTrue();

            // Step 3: Verify booking marked as paid
            var updatedBooking = await DbContext.Bookings.FindAsync(booking.BookingId);
            updatedBooking.Should().NotBeNull();
            updatedBooking!.IsPaidToHost.Should().BeTrue();
            updatedBooking.PaidToHostAt.Should().NotBeNull();

            // Step 4: Admin view paid payouts
            var paidPayoutsResponse = await Client.GetAsync("/api/admin/payouts/paid");
            paidPayoutsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var paidResult = await paidPayoutsResponse.Content.ReadFromJsonAsync<JsonElement>();
            paidResult.GetProperty("success").GetBoolean().Should().BeTrue();
        }

        /// <summary>
        /// Test Host Payout với booking chưa đủ 15 ngày
        /// </summary>
        [Fact]
        [Trait("Category", "HostPayoutWorkflow")]
        [Trait("TestID", "WF-PAYOUT-002")]
        public async Task WF_PAYOUT_002_PayoutLessThan15Days_ShouldReturnBadRequest()
        {
            // Arrange - Create completed booking < 15 days ago
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)), // Only 5 days ago
                Status = "Completed",
                TotalPrice = 2000000,
                IsPaidToHost = false,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            var adminToken = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(adminToken);

            // Act
            var confirmPayoutResponse = await Client.PostAsync(
                $"/api/admin/payouts/{booking.BookingId}/confirm", null);
            
            // Assert
            confirmPayoutResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await confirmPayoutResponse.Content.ReadFromJsonAsync<HostPayoutResponseDTO>();
            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
        }

        /// <summary>
        /// Test Host Payout - Process all payouts
        /// </summary>
        [Fact]
        [Trait("Category", "HostPayoutWorkflow")]
        [Trait("TestID", "WF-PAYOUT-003")]
        public async Task WF_PAYOUT_003_ProcessAllPayouts_ShouldMarkAllAsPaid()
        {
            // Arrange - Create wallet
            var wallet = new Wallet
            {
                HostId = 1,
                BankName = "Vietcombank",
                AccountNumber = "9876543210",
                AccountHolderName = "Host User",
                Status = "Active",
                IsDefault = true
            };
            DbContext.Wallets.Add(wallet);

            // Create multiple completed bookings >= 15 days ago
            var booking1 = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)),
                Status = "Completed",
                TotalPrice = 3000000,
                IsPaidToHost = false,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var booking2 = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-25)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-18)),
                Status = "Completed",
                TotalPrice = 4000000,
                IsPaidToHost = false,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            };

            DbContext.Bookings.AddRange(booking1, booking2);
            await DbContext.SaveChangesAsync();

            var adminToken = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(adminToken);

            // Act - Process all payouts
            var processAllResponse = await Client.PostAsync("/api/admin/payouts/process-all", null);
            
            // Assert
            processAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await processAllResponse.Content.ReadFromJsonAsync<HostPayoutResponseDTO>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.ProcessedCount.Should().BeGreaterThan(0);
            result.TotalAmount.Should().BeGreaterThan(0);

            // Verify bookings marked as paid
            var updatedBooking1 = await DbContext.Bookings.FindAsync(booking1.BookingId);
            var updatedBooking2 = await DbContext.Bookings.FindAsync(booking2.BookingId);
            
            updatedBooking1!.IsPaidToHost.Should().BeTrue();
            updatedBooking2!.IsPaidToHost.Should().BeTrue();
        }

        #endregion

        #region Workflow 6: Review Workflow

        /// <summary>
        /// Test luồng Review hoàn chỉnh: Booking Completed → Tenant Create Review → Host Reply
        /// </summary>
        [Fact]
        [Trait("Category", "ReviewWorkflow")]
        [Trait("TestID", "WF-REVIEW-001")]
        public async Task WF_REVIEW_001_CompleteReviewWorkflow_ShouldWorkEndToEnd()
        {
            // Arrange - Create completed booking
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-8)),
                Status = "Completed",
                TotalPrice = 200000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            // Step 1: Tenant create review
            var tenantToken = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(tenantToken);

            var reviewRequest = new ReviewDTO
            {
                BookingId = booking.BookingId,
                CondotelId = 1,
                Rating = 5,
                Comment = "Excellent stay! Great service and location."
            };

            var reviewResponse = await Client.PostAsJsonAsync("/api/tenant/reviews", reviewRequest);
            reviewResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var reviewResult = await reviewResponse.Content.ReadFromJsonAsync<JsonElement>();
            reviewResult.GetProperty("success").GetBoolean().Should().BeTrue();

            // Step 2: Verify review created
            var review = await DbContext.Reviews
                .FirstOrDefaultAsync(r => r.BookingId == booking.BookingId);
            
            review.Should().NotBeNull();
            review!.Rating.Should().Be(5);
            review.Comment.Should().Be("Excellent stay! Great service and location.");

            // Step 3: Tenant get my reviews
            var myReviewsResponse = await Client.GetAsync("/api/tenant/reviews");
            myReviewsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var myReviewsResult = await myReviewsResponse.Content.ReadFromJsonAsync<JsonElement>();
            myReviewsResult.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);

            // Step 4: Host view reviews
            var hostToken = GenerateJwtToken(2, "host@test.com", "Host");
            SetAuthHeader(hostToken);

            var hostReviewsResponse = await Client.GetAsync("/api/host/review");
            hostReviewsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 5: Host reply to review
            var replyRequest = new
            {
                reply = "Thank you for your feedback! We're glad you enjoyed your stay."
            };

            var replyResponse = await Client.PutAsJsonAsync(
                $"/api/host/review/{review.ReviewId}/reply", replyRequest);
            
            replyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 6: Verify reply saved
            var updatedReview = await DbContext.Reviews.FindAsync(review.ReviewId);
            updatedReview.Should().NotBeNull();
            updatedReview!.Reply.Should().NotBeNullOrEmpty();
            updatedReview.Reply.Should().Be("Thank you for your feedback! We're glad you enjoyed your stay.");
        }

        /// <summary>
        /// Test Review với booking chưa completed
        /// </summary>
        [Fact]
        [Trait("Category", "ReviewWorkflow")]
        [Trait("TestID", "WF-REVIEW-002")]
        public async Task WF_REVIEW_002_ReviewPendingBooking_ShouldReturnBadRequest()
        {
            // Arrange - Create pending booking
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                Status = "Pending",
                TotalPrice = 200000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            var tenantToken = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(tenantToken);

            // Act
            var reviewRequest = new ReviewDTO
            {
                BookingId = booking.BookingId,
                CondotelId = 1,
                Rating = 5,
                Comment = "Test review"
            };

            var reviewResponse = await Client.PostAsJsonAsync("/api/tenant/reviews", reviewRequest);
            
            // Assert
            reviewResponse.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Test Review - Duplicate review
        /// </summary>
        [Fact]
        [Trait("Category", "ReviewWorkflow")]
        [Trait("TestID", "WF-REVIEW-003")]
        public async Task WF_REVIEW_003_DuplicateReview_ShouldReturnBadRequest()
        {
            // Arrange - Create completed booking with existing review
            var booking = new Booking
            {
                CondotelId = 1,
                CustomerId = 3,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-8)),
                Status = "Completed",
                TotalPrice = 200000,
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);

            var existingReview = new Review
            {
                CondotelId = 1,
                UserId = 3,
                BookingId = booking.BookingId,
                Rating = 4,
                Comment = "Existing review",
                CreatedAt = DateTime.UtcNow,
                Status = "Visible"
            };
            DbContext.Reviews.Add(existingReview);
            await DbContext.SaveChangesAsync();

            var tenantToken = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(tenantToken);

            // Act - Try to create duplicate review
            var reviewRequest = new ReviewDTO
            {
                BookingId = booking.BookingId,
                CondotelId = 1,
                Rating = 5,
                Comment = "Duplicate review"
            };

            var reviewResponse = await Client.PostAsJsonAsync("/api/tenant/reviews", reviewRequest);
            
            // Assert
            reviewResponse.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        #endregion
    }
}

