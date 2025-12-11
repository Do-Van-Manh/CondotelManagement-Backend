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
    public class VoucherIntegrationTests : TestBase
    {
        public VoucherIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateVoucher_AsHost_CreatesVoucher()
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
        public async Task CreateVoucher_WithDuplicateCode_ReturnsBadRequest()
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
                code = "EXISTING", // Duplicate code
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

        [Fact]
        public async Task GetVouchers_ForCondotel_ReturnsVouchers()
        {
            // Arrange - Create vouchers
            var vouchers = new List<Voucher>
            {
                new Voucher
                {
                    Code = "VOUCHER1",
                    CondotelId = 1,
                    DiscountPercentage = 10,
                    MaxUses = 100,
                    ExpiryDate = DateTime.UtcNow.AddDays(30),
                    Status = "Active"
                },
                new Voucher
                {
                    Code = "VOUCHER2",
                    CondotelId = 1,
                    DiscountAmount = 50000,
                    MaxUses = 50,
                    ExpiryDate = DateTime.UtcNow.AddDays(30),
                    Status = "Active"
                }
            };
            DbContext.Vouchers.AddRange(vouchers);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/tenant/voucher/condotel/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<List<VoucherDTO>>();
            result.Should().NotBeNull();
            result!.Count.Should().BeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task ApplyVoucher_WithValidCode_AppliesDiscount()
        {
            // Arrange - Create voucher
            var voucher = new Voucher
            {
                Code = "APPLYTEST",
                CondotelId = 1,
                DiscountPercentage = 20,
                MaxUses = 100,
                UsedCount = 0,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Status = "Active"
            };
            DbContext.Vouchers.Add(voucher);
            await DbContext.SaveChangesAsync();

            var token = GenerateJwtToken(3, "tenant@test.com", "Tenant");
            SetAuthHeader(token);

            var request = new BookingDTO
            {
                CondotelId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                VoucherId = voucher.VoucherId
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/booking", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }
    }
}

