using CondotelManagement.Data;
using CondotelManagement.DTOs.Booking;
using CondotelManagement.DTOs.Host;
using CondotelManagement.DTOs.Admin;
using CondotelManagement.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CondotelManagement.Tests.Integration
{
    /// <summary>
    /// Integration tests cho chức năng Refund (hoàn tiền) và Payout (thanh toán cho host)
    /// </summary>
    public class RefundAndPayoutTests : TestBase
    {
        public RefundAndPayoutTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        #region Refund Tests - Hoàn tiền khi hủy phòng

        [Fact]
        [Trait("Category", "Refund")]
        [Trait("TestID", "TC-REFUND-001")]
        public async Task TC_REFUND_001_TenantCancelBookingWithRefund_ShouldCreateRefundRequest()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Tạo booking đã thanh toán (Confirmed)
            var booking = new Booking
            {
                BookingId = 100,
                CustomerId = 3,
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), // 5 ngày sau (đủ điều kiện refund >= 2 ngày)
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                TotalPrice = 1000000,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            // Act - Hủy booking (tự động tạo refund request)
            var response = await Client.DeleteAsync($"/api/booking/{booking.BookingId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Kiểm tra booking status đã chuyển thành Cancelled
            var updatedBooking = await DbContext.Bookings.FindAsync(booking.BookingId);
            updatedBooking.Should().NotBeNull();
            updatedBooking!.Status.Should().Be("Cancelled");

            // Kiểm tra RefundRequest đã được tạo
            var refundRequest = await DbContext.RefundRequests
                .FirstOrDefaultAsync(r => r.BookingId == booking.BookingId);
            refundRequest.Should().NotBeNull();
            refundRequest!.Status.Should().Be("Pending");
            refundRequest.RefundAmount.Should().Be(1000000);
            refundRequest.CustomerId.Should().Be(3);
        }

        [Fact]
        [Trait("Category", "Refund")]
        [Trait("TestID", "TC-REFUND-002")]
        public async Task TC_REFUND_002_TenantRequestRefundWithBankInfo_ShouldCreateRefundRequest()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Tạo booking đã thanh toán (Confirmed)
            var booking = new Booking
            {
                BookingId = 101,
                CustomerId = 3,
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                TotalPrice = 2000000,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            var refundRequest = new RefundBookingRequestDTO
            {
                BankCode = "VCB",
                AccountNumber = "1234567890",
                AccountHolder = "Tenant User"
            };

            // Act
            var response = await Client.PostAsJsonAsync($"/api/booking/{booking.BookingId}/refund", refundRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("success").GetBoolean().Should().BeTrue();

            // Kiểm tra RefundRequest đã được tạo với thông tin ngân hàng
            var refund = await DbContext.RefundRequests
                .FirstOrDefaultAsync(r => r.BookingId == booking.BookingId);
            refund.Should().NotBeNull();
            refund!.BankCode.Should().Be("VCB");
            refund.AccountNumber.Should().Be("1234567890");
            refund.AccountHolder.Should().Be("Tenant User");
        }

        [Fact]
        [Trait("Category", "Refund")]
        [Trait("TestID", "TC-REFUND-003")]
        public async Task TC_REFUND_003_TenantCancelBookingLessThan2Days_ShouldNotRefund()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Tạo booking với check-in trong 1 ngày (không đủ điều kiện refund)
            var booking = new Booking
            {
                BookingId = 102,
                CustomerId = 3,
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), // Chỉ còn 1 ngày
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                TotalPrice = 1000000,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            var refundRequest = new RefundBookingRequestDTO
            {
                BankCode = "VCB",
                AccountNumber = "1234567890",
                AccountHolder = "Tenant User"
            };

            // Act
            var response = await Client.PostAsJsonAsync($"/api/booking/{booking.BookingId}/refund", refundRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("success").GetBoolean().Should().BeFalse();
            result.GetProperty("message").GetString().Should().ContainAny("2 days", "2 ngày", "ít nhất");
        }

        [Fact]
        [Trait("Category", "Refund")]
        [Trait("TestID", "TC-REFUND-004")]
        public async Task TC_REFUND_004_AdminGetRefundRequests_ShouldReturnList()
        {
            // Arrange
            var adminToken = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(adminToken);

            // Tạo refund requests
            var refund1 = new RefundRequest
            {
                BookingId = 1,
                CustomerId = 3,
                CustomerName = "Tenant User",
                CustomerEmail = "tenant@test.com",
                RefundAmount = 1000000,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            var refund2 = new RefundRequest
            {
                BookingId = 2,
                CustomerId = 3,
                CustomerName = "Tenant User",
                CustomerEmail = "tenant@test.com",
                RefundAmount = 2000000,
                Status = "Completed",
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            };
            DbContext.RefundRequests.AddRange(refund1, refund2);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/admin/refund-requests");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("success").GetBoolean().Should().BeTrue();
            var data = result.GetProperty("data").EnumerateArray().ToList();
            data.Count.Should().BeGreaterThanOrEqualTo(2);
        }

        [Fact]
        [Trait("Category", "Refund")]
        [Trait("TestID", "TC-REFUND-005")]
        public async Task TC_REFUND_005_AdminConfirmRefund_ShouldUpdateStatus()
        {
            // Arrange
            var adminToken = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(adminToken);

            // Tạo booking và refund request
            var booking = new Booking
            {
                BookingId = 103,
                CustomerId = 3,
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                TotalPrice = 1500000,
                Status = "Cancelled",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);

            var refundRequest = new RefundRequest
            {
                BookingId = 103,
                CustomerId = 3,
                CustomerName = "Tenant User",
                CustomerEmail = "tenant@test.com",
                RefundAmount = 1500000,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.RefundRequests.Add(refundRequest);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.PostAsync($"/api/admin/refund-requests/{refundRequest.Id}/confirm", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("success").GetBoolean().Should().BeTrue();

            // Kiểm tra status đã được cập nhật
            var updatedRefund = await DbContext.RefundRequests.FindAsync(refundRequest.Id);
            updatedRefund.Should().NotBeNull();
            updatedRefund!.Status.Should().Be("Completed");
            updatedRefund.ProcessedAt.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Refund")]
        [Trait("TestID", "TC-REFUND-006")]
        public async Task TC_REFUND_006_TenantRefundPendingBooking_ShouldReturnBadRequest()
        {
            // Arrange
            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            // Tạo booking chưa thanh toán (Pending)
            var booking = new Booking
            {
                BookingId = 104,
                CustomerId = 3,
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                TotalPrice = 1000000,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            var refundRequest = new RefundBookingRequestDTO
            {
                BankCode = "VCB",
                AccountNumber = "1234567890",
                AccountHolder = "Tenant User"
            };

            // Act
            var response = await Client.PostAsJsonAsync($"/api/booking/{booking.BookingId}/refund", refundRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("success").GetBoolean().Should().BeFalse();
        }

        #endregion

        #region Payout Tests - Admin thanh toán cho host

        [Fact]
        [Trait("Category", "Payout")]
        [Trait("TestID", "TC-PAYOUT-001")]
        public async Task TC_PAYOUT_001_AdminGetPendingPayouts_ShouldReturnList()
        {
            // Arrange
            var adminToken = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(adminToken);

            // Tạo booking đã completed >= 15 ngày, chưa được trả tiền
            var booking = new Booking
            {
                BookingId = 200,
                CustomerId = 3,
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)), // Đã kết thúc 20 ngày trước
                TotalPrice = 5000000,
                Status = "Completed",
                IsPaidToHost = false,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };
            DbContext.Bookings.Add(booking);

            // Tạo Wallet cho host
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
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/admin/payouts/pending");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("success").GetBoolean().Should().BeTrue();
            var data = result.GetProperty("data").EnumerateArray().ToList();
            data.Should().NotBeEmpty();
            
            // Kiểm tra booking có trong danh sách
            var payoutItem = data.FirstOrDefault(d => d.GetProperty("bookingId").GetInt32() == 200);
            payoutItem.Should().NotBeNull();
            payoutItem!.GetProperty("isPaid").GetBoolean().Should().BeFalse();
            payoutItem.GetProperty("amount").GetDecimal().Should().Be(5000000);
        }

        [Fact]
        [Trait("Category", "Payout")]
        [Trait("TestID", "TC-PAYOUT-002")]
        public async Task TC_PAYOUT_002_AdminProcessAllPayouts_ShouldMarkBookingsAsPaid()
        {
            // Arrange
            var adminToken = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(adminToken);

            // Tạo booking đã completed >= 15 ngày
            var booking = new Booking
            {
                BookingId = 201,
                CustomerId = 3,
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)),
                TotalPrice = 3000000,
                Status = "Completed",
                IsPaidToHost = false,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };
            DbContext.Bookings.Add(booking);

            // Tạo Wallet cho host
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
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.PostAsync("/api/admin/payouts/process-all", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<HostPayoutResponseDTO>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.ProcessedCount.Should().BeGreaterThan(0);
            result.TotalAmount.Should().BeGreaterThan(0);
            result.Message.Should().Contain("Đã xử lý");

            // Kiểm tra booking đã được đánh dấu là đã trả tiền
            var updatedBooking = await DbContext.Bookings.FindAsync(booking.BookingId);
            updatedBooking.Should().NotBeNull();
            updatedBooking!.IsPaidToHost.Should().BeTrue();
            updatedBooking.PaidToHostAt.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Payout")]
        [Trait("TestID", "TC-PAYOUT-003")]
        public async Task TC_PAYOUT_003_AdminConfirmPayoutForBooking_ShouldMarkAsPaid()
        {
            // Arrange
            var adminToken = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(adminToken);

            // Tạo booking đã completed >= 15 ngày
            var booking = new Booking
            {
                BookingId = 202,
                CustomerId = 3,
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)),
                TotalPrice = 4000000,
                Status = "Completed",
                IsPaidToHost = false,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };
            DbContext.Bookings.Add(booking);

            // Tạo Wallet cho host
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
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.PostAsync($"/api/admin/payouts/{booking.BookingId}/confirm", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<HostPayoutResponseDTO>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.ProcessedCount.Should().Be(1);
            result.Message.Should().Contain("Đã trả");

            // Kiểm tra booking đã được đánh dấu là đã trả tiền
            var updatedBooking = await DbContext.Bookings.FindAsync(booking.BookingId);
            updatedBooking.Should().NotBeNull();
            updatedBooking!.IsPaidToHost.Should().BeTrue();
            updatedBooking.PaidToHostAt.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Payout")]
        [Trait("TestID", "TC-PAYOUT-004")]
        public async Task TC_PAYOUT_004_AdminProcessPayoutForBookingLessThan15Days_ShouldReturnBadRequest()
        {
            // Arrange
            var adminToken = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(adminToken);

            // Tạo booking mới completed (chưa đủ 15 ngày)
            var booking = new Booking
            {
                BookingId = 203,
                CustomerId = 3,
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)), // Chỉ mới kết thúc 5 ngày
                TotalPrice = 2000000,
                Status = "Completed",
                IsPaidToHost = false,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.PostAsync($"/api/admin/payouts/{booking.BookingId}/confirm", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<HostPayoutResponseDTO>();
            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
            result.Message.Should().ContainAny("15 days", "15 ngày", "ít nhất");
        }

        [Fact]
        [Trait("Category", "Payout")]
        [Trait("TestID", "TC-PAYOUT-005")]
        public async Task TC_PAYOUT_005_AdminGetPaidPayouts_ShouldReturnList()
        {
            // Arrange
            var adminToken = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(adminToken);

            // Tạo booking đã được trả tiền
            var booking = new Booking
            {
                BookingId = 204,
                CustomerId = 3,
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)),
                TotalPrice = 6000000,
                Status = "Completed",
                IsPaidToHost = true,
                PaidToHostAt = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/admin/payouts/paid");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("success").GetBoolean().Should().BeTrue();
            var data = result.GetProperty("data").EnumerateArray().ToList();
            
            // Kiểm tra booking có trong danh sách
            var payoutItem = data.FirstOrDefault(d => d.GetProperty("bookingId").GetInt32() == 204);
            payoutItem.Should().NotBeNull();
            payoutItem!.GetProperty("isPaid").GetBoolean().Should().BeTrue();
            payoutItem.GetProperty("amount").GetDecimal().Should().Be(6000000);
        }

        [Fact]
        [Trait("Category", "Payout")]
        [Trait("TestID", "TC-PAYOUT-006")]
        public async Task TC_PAYOUT_006_AdminProcessPayoutWithRefundRequest_ShouldNotProcess()
        {
            // Arrange
            var adminToken = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(adminToken);

            // Tạo booking đã completed >= 15 ngày nhưng có refund request pending
            var booking = new Booking
            {
                BookingId = 205,
                CustomerId = 3,
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)),
                TotalPrice = 5000000,
                Status = "Completed",
                IsPaidToHost = false,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };
            DbContext.Bookings.Add(booking);

            // Tạo refund request pending
            var refundRequest = new RefundRequest
            {
                BookingId = 205,
                CustomerId = 3,
                CustomerName = "Tenant User",
                RefundAmount = 5000000,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            DbContext.RefundRequests.Add(refundRequest);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.PostAsync($"/api/admin/payouts/{booking.BookingId}/confirm", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<HostPayoutResponseDTO>();
            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
            result.Message.Should().ContainAny("refund", "hoàn tiền");
        }

        [Fact]
        [Trait("Category", "Payout")]
        [Trait("TestID", "TC-PAYOUT-007")]
        public async Task TC_PAYOUT_007_AdminProcessAlreadyPaidBooking_ShouldReturnBadRequest()
        {
            // Arrange
            var adminToken = GenerateJwtToken(1, "admin@test.com", "Admin");
            SetAuthHeader(adminToken);

            // Tạo booking đã được trả tiền
            var booking = new Booking
            {
                BookingId = 206,
                CustomerId = 3,
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)),
                TotalPrice = 3000000,
                Status = "Completed",
                IsPaidToHost = true,
                PaidToHostAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };
            DbContext.Bookings.Add(booking);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.PostAsync($"/api/admin/payouts/{booking.BookingId}/confirm", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<HostPayoutResponseDTO>();
            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
            result.Message.Should().ContainAny("already", "đã", "trả");
        }

        #endregion
    }
}

