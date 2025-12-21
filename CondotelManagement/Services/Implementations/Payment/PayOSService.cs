using CondotelManagement.DTOs.Payment;
using CondotelManagement.Services.Interfaces.Payment;
using CondotelManagement.Services;
using CondotelManagement.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CondotelManagement.Services.Interfaces.Shared;

namespace CondotelManagement.Services.Implementations.Payment
{
    public class PayOSService : IPayOSService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly CondotelDbVer1Context _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _clientId;
        private readonly string _apiKey;
        private readonly string _checksumKey;
        private readonly string _baseUrl;
        private const int PayOsDescriptionLimit = 25;

        public PayOSService(IConfiguration configuration, HttpClient httpClient, CondotelDbVer1Context context, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _context = context;
            _serviceProvider = serviceProvider;
            _clientId = _configuration["PayOS:ClientId"] ?? throw new InvalidOperationException("PayOS:ClientId is not configured");
            _apiKey = _configuration["PayOS:ApiKey"] ?? throw new InvalidOperationException("PayOS:ApiKey is not configured");
            _checksumKey = _configuration["PayOS:ChecksumKey"] ?? throw new InvalidOperationException("PayOS:ChecksumKey is not configured");
            _baseUrl = _configuration["PayOS:BaseUrl"] ?? "https://api-merchant.payos.vn";
            
            // Log credentials (chỉ log một phần để debug)
            Console.WriteLine($"PayOS Config - BaseUrl: {_baseUrl}, ClientId: {_clientId.Substring(0, Math.Min(8, _clientId.Length))}..., ApiKey: {_apiKey.Substring(0, Math.Min(8, _apiKey.Length))}...");
        }

        public async Task<PayOSCreatePaymentResponse> CreatePaymentLinkAsync(PayOSCreatePaymentRequest request)
        {
            try
            {
                // Validate request
                if (request.Amount <= 0)
                {
                    throw new InvalidOperationException("Amount must be greater than 0");
                }

                if (request.Amount < 10000) // PayOS minimum is 10,000 VND
                {
                    throw new InvalidOperationException("Amount must be at least 10,000 VND");
                }

                if (request.Items == null || !request.Items.Any())
                {
                    throw new InvalidOperationException("Items list cannot be empty");
                }

                // Validate items
                int totalItemsAmount = 0;
                foreach (var item in request.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.Name))
                    {
                        throw new InvalidOperationException("Item name cannot be empty");
                    }
                    if (item.Price <= 0)
                    {
                        throw new InvalidOperationException("Item price must be greater than 0");
                    }
                    if (item.Quantity <= 0)
                    {
                        throw new InvalidOperationException("Item quantity must be greater than 0");
                    }
                    totalItemsAmount += item.Price * item.Quantity;
                }
                
                // PayOS yêu cầu tổng giá items phải bằng amount
                if (totalItemsAmount != request.Amount)
                {
                    throw new InvalidOperationException($"Total items amount ({totalItemsAmount}) must equal request amount ({request.Amount})");
                }
                var description = PrepareDescription(request.Description, request.OrderCode);
                var returnUrl = request.ReturnUrl ?? string.Empty;
                var cancelUrl = request.CancelUrl ?? string.Empty;

                if (!request.ExpiredAt.HasValue)
                {
                    request.ExpiredAt = (int)DateTimeOffset.Now.AddMinutes(3).ToUnixTimeSeconds();
                }
                // Tạo request object theo format chuẩn PayOS
                var requestBodyDict = new Dictionary<string, object>
                {
                    { "orderCode", request.OrderCode },
                    { "amount", request.Amount },
                    { "description", description },
                    { "returnUrl", returnUrl },
                    { "cancelUrl", cancelUrl },
                    { "expiredAt", request.ExpiredAt.Value },
                    { "items", request.Items.Select(i => new Dictionary<string, object>
                    {
                        { "name", i.Name ?? string.Empty },
                        { "quantity", i.Quantity },
                        { "price", i.Price }
                    }).ToList() }
                };

                // Thêm các field optional nếu có giá trị
                if (!string.IsNullOrWhiteSpace(request.BuyerName))
                    requestBodyDict["buyerName"] = request.BuyerName;
                if (!string.IsNullOrWhiteSpace(request.BuyerEmail))
                    requestBodyDict["buyerEmail"] = request.BuyerEmail;
                if (!string.IsNullOrWhiteSpace(request.BuyerPhone))
                    requestBodyDict["buyerPhone"] = request.BuyerPhone;
                if (!string.IsNullOrWhiteSpace(request.BuyerAddress))
                    requestBodyDict["buyerAddress"] = request.BuyerAddress;
                if (request.ExpiredAt.HasValue)
                    requestBodyDict["expiredAt"] = request.ExpiredAt.Value;

                var signature = GenerateCreatePaymentSignature(
                    request.Amount,
                    cancelUrl,
                    description,
                    request.OrderCode,
                    returnUrl);
                requestBodyDict["signature"] = signature;

                var json = JsonSerializer.Serialize(requestBodyDict, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                Console.WriteLine($"=== PayOS Request ===");
                Console.WriteLine($"URL: {_baseUrl}/v2/payment-requests");
                Console.WriteLine($"ClientId: {_clientId.Substring(0, Math.Min(8, _clientId.Length))}...");
                Console.WriteLine($"ApiKey: {_apiKey.Substring(0, Math.Min(8, _apiKey.Length))}...");
                Console.WriteLine($"Request JSON: {json}");
                Console.WriteLine($"=====================");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Log headers
                Console.WriteLine($"Request Headers:");
                foreach (var header in _httpClient.DefaultRequestHeaders)
                {
                    Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                }
                
                var response = await _httpClient.PostAsync("/v2/payment-requests", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"PayOS Response Status: {response.StatusCode}");
                Console.WriteLine($"PayOS Response: {responseContent}");

                // Parse response để kiểm tra code trong body
                PayOSCreatePaymentResponse? result = null;
                try
                {
                    result = JsonSerializer.Deserialize<PayOSCreatePaymentResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException($"Failed to parse PayOS response: {ex.Message}");
                }

                if (result == null)
                    throw new InvalidOperationException("Failed to parse PayOS response");

                // Kiểm tra cả HTTP status code và code trong response body
                if (!response.IsSuccessStatusCode || result.Code != "00")
                {
                    var errorMessage = result.Desc ?? "Unknown error";
                    var errorCode = result.Code ?? "Unknown";
                    throw new InvalidOperationException($"PayOS error: {errorMessage} (Code: {errorCode})");
                }

                return result;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating PayOS payment link: {ex.Message}", ex);
            }
        }

        private string PrepareDescription(string? providedDescription, long orderCode)
        {
            var description = string.IsNullOrWhiteSpace(providedDescription)
                ? $"Condotel #{orderCode}"
                : providedDescription.Trim();

            return description.Length > PayOsDescriptionLimit
                ? description.Substring(0, PayOsDescriptionLimit)
                : description;
        }

        private string GenerateCreatePaymentSignature(int amount, string cancelUrl, string description, long orderCode, string returnUrl)
        {
            // PayOS signature format: amount, cancelUrl, description, orderCode, returnUrl sorted alphabetically
            var payload = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_checksumKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        public async Task<PayOSPaymentInfo> GetPaymentInfoAsync(string paymentLinkId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/v2/payment-requests/{paymentLinkId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"PayOS API error: {response.StatusCode} - {responseContent}");
                }

                var result = JsonSerializer.Deserialize<PayOSPaymentInfo>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? throw new InvalidOperationException("Failed to parse PayOS response");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting PayOS payment info: {ex.Message}", ex);
            }
        }

        public async Task<PayOSPaymentInfo?> GetPaymentInfoByOrderCodeAsync(long orderCode)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/v2/payment-requests/id/{orderCode}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null; // OrderCode không tồn tại
                }

                if (!response.IsSuccessStatusCode)
                {
                    return null; // Có lỗi, trả về null
                }

                var result = JsonSerializer.Deserialize<PayOSPaymentInfo>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch
            {
                return null; // Có lỗi, trả về null
            }
        }

        public async Task<PayOSCreatePaymentResponse> CancelPaymentLinkAsync(string paymentLinkId, string? cancellationReason = null)
        {
            try
            {
                var cancelData = new { cancellationReason = cancellationReason ?? "User cancelled" };
                var json = JsonSerializer.Serialize(cancelData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/v2/payment-requests/{paymentLinkId}/cancel", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"PayOS API error: {response.StatusCode} - {responseContent}");
                }

                var result = JsonSerializer.Deserialize<PayOSCreatePaymentResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? throw new InvalidOperationException("Failed to parse PayOS response");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error cancelling PayOS payment link: {ex.Message}", ex);
            }
        }

        public async Task<PayOSCreatePaymentResponse> CancelPaymentLinkByOrderCodeAsync(long orderCode, string? cancellationReason = null)
        {
            try
            {
                var cancelData = new { cancellationReason = cancellationReason ?? "User cancelled" };
                var json = JsonSerializer.Serialize(cancelData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/v2/payment-requests/{orderCode}/cancel", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"PayOS API error: {response.StatusCode} - {responseContent}");
                }

                var result = JsonSerializer.Deserialize<PayOSCreatePaymentResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? throw new InvalidOperationException("Failed to parse PayOS response");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error cancelling PayOS payment link: {ex.Message}", ex);
            }
        }

        public bool VerifyWebhookSignature(string signature, string body)
        {
            try
            {
                if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(body))
                    return false;

                // Parse body to get webhook data
                var webhookData = JsonSerializer.Deserialize<PayOSWebhookData>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookData?.Data == null)
                    return false;

                // Format: code|orderCode|amount|transactionDateTime|currency
                var dataString = $"{webhookData.Code}|{webhookData.Data.OrderCode}|{webhookData.Data.Amount}|{webhookData.Data.TransactionDateTime}|{webhookData.Data.Currency}";
                
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_checksumKey));
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString));
                var computedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                return computedSignature.Equals(signature.ToLower(), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ProcessWebhookAsync(PayOSWebhookData webhookData)
        {
            try
            {
                // --- 1. VALIDATE DỮ LIỆU (GIỮ NGUYÊN) ---
                if (webhookData.Data == null)
                {
                    Console.WriteLine("Webhook data is null");
                    return false;
                }

                var orderCode = webhookData.Data.OrderCode;
                var isSuccess = (webhookData.Code == "00" || webhookData.Success == true) &&
                                (webhookData.Data.Code == "00" || webhookData.Data.Code == "PAID");

                Console.WriteLine($"[Webhook] Nhận thanh toán - OrderCode: {orderCode}, Status: {(isSuccess ? "THÀNH CÔNG" : "THẤT BẠI")}");

                if (!isSuccess)
                {
                    return false;
                }

                // ========================================================================
                // PHẦN 1: XỬ LÝ REFUND PAYMENT (Kiểm tra trước tiên)
                // ========================================================================
                // Kiểm tra xem có phải refund payment không (OrderCode có suffix 999999)
                var orderCodeSuffix = orderCode % 1000000;
                var isRefundPayment = orderCodeSuffix == 999999;

                if (isRefundPayment)
                {
                    // === XỬ LÝ REFUND PAYMENT ===
                    var bookingId = (int)(orderCode / 1000000);
                    var refundRequest = await _context.RefundRequests
                        .Include(r => r.Booking)
                        .FirstOrDefaultAsync(r => r.BookingId == bookingId && r.Status == "Pending");

                    if (refundRequest != null)
                    {
                        refundRequest.Status = "Refunded";
                        refundRequest.ProcessedAt = DateTime.UtcNow;
                        refundRequest.UpdatedAt = DateTime.UtcNow;
                        
                        // Rollback Voucher UsedCount khi refund thành công
                        if (refundRequest.Booking?.VoucherId.HasValue == true)
                        {
                            try
                            {
                                using var scope = _serviceProvider.CreateScope();
                                var voucherService = scope.ServiceProvider.GetRequiredService<IVoucherService>();
                                await voucherService.RollbackVoucherUsageAsync(refundRequest.Booking.VoucherId.Value);
                                Console.WriteLine($"[Webhook] Đã rollback UsedCount cho Voucher {refundRequest.Booking.VoucherId.Value}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[Webhook] Lỗi khi rollback Voucher UsedCount: {ex.Message}");
                                // Không throw để không ảnh hưởng đến refund
                            }
                        }
                        // Booking status giữ nguyên "Cancelled", không đổi thành "Refunded"
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"[Webhook] ĐÃ HOÀN TIỀN THÀNH CÔNG CHO REFUND REQUEST {refundRequest.Id} (Booking {bookingId})!");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"[Webhook] Không tìm thấy RefundRequest Pending cho Booking {bookingId}");
                        return false;
                    }
                }

                // ========================================================================
                // PHẦN 2: XỬ LÝ BOOKING PAYMENT
                // ========================================================================
                var bookingId_normal = (int)(orderCode / 1000000);
                
                // Sử dụng transaction với Serializable isolation để tránh race condition
                using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

                try
                {
                    // Lock booking row để tránh race condition
                    var booking = await _context.Bookings
                        .FromSqlRaw(
                            "SELECT * FROM Booking WITH (UPDLOCK, ROWLOCK) WHERE BookingId = @bookingId",
                            new SqlParameter("@bookingId", bookingId_normal))
                        .FirstOrDefaultAsync();
                    if (booking == null)
                    {
                        await transaction.CommitAsync();
                        Console.WriteLine($"[Webhook] Không tìm thấy Booking {bookingId_normal}");
                        return false;
                    }
                    var customer = await _context.Users
      .Where(u => u.UserId == booking.CustomerId)
      .Select(u => new
      {
          u.Email,
          u.FullName
      })
      .FirstOrDefaultAsync();

                    if (customer == null)
                    {
                        await transaction.CommitAsync();
                        Console.WriteLine("[EMAIL] Không tìm thấy customer");
                        return true;
                    }

                   
                    if (booking != null)
                    {
                        if (booking.Status == "Confirmed" || booking.Status == "Completed")
                        {
                            Console.WriteLine($"[Webhook] Booking {bookingId_normal} đã xác nhận trước đó → bỏ qua");
                            await transaction.CommitAsync();
                            return true;
                        }

                        // Kiểm tra availability trước khi confirm để tránh double booking
                        var today = DateOnly.FromDateTime(DateTime.Now);
                        var conflictingBookings = await _context.Bookings
                            .FromSqlRaw(@"
                                SELECT * FROM Booking WITH (UPDLOCK, ROWLOCK)
                                WHERE CondotelId = @condotelId 
                                AND BookingId != @currentBookingId
                                AND Status IN ('Confirmed', 'Completed', 'Pending','InStay')
                                AND Status != 'Cancelled'
                                AND EndDate >= @today",
                                new SqlParameter("@condotelId", booking.CondotelId),
                                new SqlParameter("@currentBookingId", booking.BookingId),
                                new SqlParameter("@today", today))
                            .ToListAsync();

                        // Kiểm tra overlap với các booking đã confirmed/completed
                        // CHỈ check conflict với Confirmed/Completed, KHÔNG check với Pending
                        // Vì nếu có Pending khác trùng thời gian, booking nào thanh toán trước sẽ được confirm
                        var hasConflict = conflictingBookings
                            .Where(b => b.Status == "Confirmed" || b.Status == "Completed")
                            .Any(b => !(booking.EndDate <= b.StartDate || booking.StartDate >= b.EndDate));

                        if (hasConflict)
                        {
                            // Có conflict với booking đã confirmed/completed → không thể confirm
                            Console.WriteLine($"[Webhook] Booking {bookingId_normal} có conflict với booking đã confirmed/completed → hủy booking");
                            booking.Status = "Cancelled";
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            
                            // TODO: Có thể gửi email thông báo cho customer về việc booking bị hủy do conflict
                            return false; // Trả về false để PayOS biết có lỗi
                        }

                        // Không có conflict → confirm booking
                        booking.Status = "Confirmed";

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                      

                     


                // Tăng Voucher UsedCount khi payment thành công
                if (booking.VoucherId.HasValue)
                        {
                            try
                            {
                                using var scope = _serviceProvider.CreateScope();
                                var voucherService = scope.ServiceProvider.GetRequiredService<IVoucherService>();
                                await voucherService.ApplyVoucherToBookingAsync(booking.VoucherId.Value);
                                Console.WriteLine($"[Webhook] Đã tăng UsedCount cho Voucher {booking.VoucherId.Value}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[Webhook] Lỗi khi tăng Voucher UsedCount: {ex.Message}");
                                // Không throw để không ảnh hưởng đến booking confirmation
                            }
                        }
                        
                        Console.WriteLine($"[Webhook] ĐÃ XÁC NHẬN BOOKING {bookingId_normal} THÀNH CÔNG!");
                        return true;
                    }
                    else
                    {
                        await transaction.CommitAsync();
                        Console.WriteLine($"[Webhook] Không tìm thấy Booking {bookingId_normal}");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"[Webhook] Lỗi khi xử lý booking payment: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    throw;
                }

                // ========================================================================
                // PHẦN 3: XỬ LÝ PACKAGE PAYMENT (PHẦN CỦA BẠN - ĐÃ SỬA DÙNG SQL UPDATE TRỰC TIẾP)
                // ========================================================================

                // Tính toán ID
                var hostId = (int)(orderCode / 1_000_000_000L);
                var packageId = (int)((orderCode % 1_000_000_000L) / 1_000_000L);

                Console.WriteLine($"[Webhook] Đang check Package cho Host: {hostId}, Pack: {packageId}");

                // Kiểm tra xem đơn hàng này có tồn tại không
                // Không cần Include Package hay Host ở đây để tối ưu query
                var packageOrderExists = await _context.HostPackages
                    .Where(hp => hp.HostId == hostId
                              && hp.PackageId == packageId
                              && hp.Status == "PendingPayment")
                    .Select(hp => new { hp.DurationDays }) // Chỉ lấy cái cần
                    .FirstOrDefaultAsync();

                if (packageOrderExists != null)
                {
                    var today = DateOnly.FromDateTime(DateTime.UtcNow);
                    var durationDays = packageOrderExists.DurationDays ?? 30;
                    var endDate = today.AddDays(durationDays);

                    // A. UPDATE HOST PACKAGE (Dùng ExecuteUpdateAsync cho chắc chắn)
                    var rowsPkg = await _context.HostPackages
                        .Where(hp => hp.HostId == hostId && hp.PackageId == packageId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(hp => hp.Status, "Active")
                            .SetProperty(hp => hp.StartDate, today)
                            .SetProperty(hp => hp.EndDate, endDate)
                        );

                    if (rowsPkg > 0)
                    {
                        Console.WriteLine($"[Webhook] ✅ Đã Active gói HostPackage (SQL Direct Update).");
                    }

                    // B. UPDATE ROLE USER (Dùng ExecuteUpdateAsync)
                    // Tìm UserId từ bảng Host
                    var userId = await _context.Hosts
                        .Where(h => h.HostId == hostId)
                        .Select(h => h.UserId)
                        .FirstOrDefaultAsync();

                    if (userId != 0) // Kiểm tra khác 0 (int mặc định là 0 nếu không tìm thấy)
                    {
                        // Bắn SQL Update Role ngay lập tức
                        await _context.Users
                            .Where(u => u.UserId == userId && u.RoleId != 4)
                            .ExecuteUpdateAsync(s => s.SetProperty(u => u.RoleId, 4));

                        Console.WriteLine($"[Webhook] ✅ Đã nâng RoleID = 4 cho UserID: {userId}");
                    }
                    else
                    {
                        Console.WriteLine($"[Webhook] ⚠️ Không tìm thấy UserID cho HostID: {hostId}");
                    }

                    return true;
                }

                // ========================================================================

                Console.WriteLine($"[Webhook] Không tìm thấy đơn hàng nào khớp với OrderCode: {orderCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Webhook CRITICAL ERROR] {ex.Message}");
                return false;
            }
        }

        public async Task<PayOSCreatePaymentResponse> CreateRefundPaymentLinkAsync(int bookingId, decimal refundAmount, string customerName, string? customerEmail = null, string? customerPhone = null)
        {
            try
            {
                // Validate amount
                var amount = (int)refundAmount;
                if (amount <= 0)
                {
                    throw new InvalidOperationException("Refund amount must be greater than 0");
                }

                if (amount < 10000) // PayOS minimum is 10,000 VND
                {
                    throw new InvalidOperationException("Refund amount must be at least 10,000 VND");
                }

                // Tạo OrderCode cho refund: BookingId * 1000000 + 999999 (để phân biệt với payment gốc)
                var refundOrderCode = (long)bookingId * 1000000L + 999999L;

                // Tạo description
                var description = $"Hoan tien #{bookingId}";
                if (description.Length > PayOsDescriptionLimit)
                {
                    description = description.Substring(0, PayOsDescriptionLimit);
                }

                // Frontend URL để customer nhận tiền
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
                var returnUrl = $"{frontendUrl}/refund-success?bookingId={bookingId}";
                var cancelUrl = $"{frontendUrl}/refund-cancel?bookingId={bookingId}";

                // Tạo payment link request (customer sẽ nhận tiền qua link này)
                var requestBodyDict = new Dictionary<string, object>
                {
                    { "orderCode", refundOrderCode },
                    { "amount", amount },
                    { "description", description },
                    { "returnUrl", returnUrl },
                    { "cancelUrl", cancelUrl },
                    { "items", new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            { "name", $"Hoan tien dat phong #{bookingId}" },
                            { "quantity", 1 },
                            { "price", amount }
                        }
                    }}
                };

                // Thêm thông tin customer nếu có
                if (!string.IsNullOrWhiteSpace(customerName))
                    requestBodyDict["buyerName"] = customerName;
                if (!string.IsNullOrWhiteSpace(customerEmail))
                    requestBodyDict["buyerEmail"] = customerEmail;
                if (!string.IsNullOrWhiteSpace(customerPhone))
                    requestBodyDict["buyerPhone"] = customerPhone;

                // Generate signature
                var signature = GenerateCreatePaymentSignature(
                    amount,
                    cancelUrl,
                    description,
                    refundOrderCode,
                    returnUrl);
                requestBodyDict["signature"] = signature;

                var json = JsonSerializer.Serialize(requestBodyDict, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                Console.WriteLine($"=== PayOS Refund Payment Link Request ===");
                Console.WriteLine($"BookingId: {bookingId}, RefundOrderCode: {refundOrderCode}, Amount: {amount}");
                Console.WriteLine($"Request JSON: {json}");
                Console.WriteLine($"=====================");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/v2/payment-requests", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"PayOS Refund Response Status: {response.StatusCode}");
                Console.WriteLine($"PayOS Refund Response: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"PayOS API error: {response.StatusCode} - {responseContent}");
                }

                var result = JsonSerializer.Deserialize<PayOSCreatePaymentResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                    throw new InvalidOperationException("Failed to parse PayOS response");

                if (result.Code != "00")
                {
                    var errorMessage = result.Desc ?? "Unknown error";
                    var errorCode = result.Code ?? "Unknown";
                    throw new InvalidOperationException($"PayOS error: {errorMessage} (Code: {errorCode})");
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating PayOS refund payment link: {ex.Message}", ex);
            }
        }



        public async Task<PayOSCreatePaymentResponse> CreatePackageRefundPaymentLinkAsync(int hostId, long originalOrderCode, decimal refundAmount, string hostName, string? hostEmail = null, string? hostPhone = null)
        {
            try
            {
                // Validate amount
                var amount = (int)refundAmount;
                if (amount <= 0)
                {
                    throw new InvalidOperationException("Refund amount must be greater than 0");
                }

                if (amount < 10000) // PayOS minimum is 10,000 VND
                {
                    throw new InvalidOperationException("Refund amount must be at least 10,000 VND");
                }

                // Tạo OrderCode cho refund package: HostId * 1000000000 + PackageId * 1000000 + 888888 (để phân biệt)
                var refundOrderCode = (long)hostId * 1000000000L + 888888L;

                // Tạo description
                var description = $"Hoan tien package Host #{hostId}";
                if (description.Length > PayOsDescriptionLimit)
                {
                    description = description.Substring(0, PayOsDescriptionLimit);
                }

                // Frontend URL để host nhận tiền
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
                var returnUrl = $"{frontendUrl}/package-refund-success?hostId={hostId}";
                var cancelUrl = $"{frontendUrl}/package-refund-cancel?hostId={hostId}";

                // Tạo payment link request (host sẽ nhận tiền qua link này)
                var request = new PayOSCreatePaymentRequest
                {
                    OrderCode = refundOrderCode,
                    Amount = amount,
                    Description = description,
                    BuyerName = hostName,
                    BuyerEmail = hostEmail ?? "",
                    BuyerPhone = hostPhone ?? "",
                    Items = new List<PayOSItem>
                    {
                        new PayOSItem
                        {
                            Name = "Hoàn tiền package",
                            Quantity = 1,
                            Price = amount
                        }
                    },
                    ReturnUrl = returnUrl,
                    CancelUrl = cancelUrl
                };

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                Console.WriteLine($"=== PayOS Package Refund Payment Link Request ===");
                Console.WriteLine($"HostId: {hostId}, OriginalOrderCode: {originalOrderCode}, RefundOrderCode: {refundOrderCode}, Amount: {amount}");
                Console.WriteLine($"Request JSON: {json}");
                Console.WriteLine($"=====================");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/v2/payment-requests", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"PayOS Package Refund Response Status: {response.StatusCode}");
                Console.WriteLine($"PayOS Package Refund Response: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"PayOS API error: {response.StatusCode} - {responseContent}");
                }

                var result = JsonSerializer.Deserialize<PayOSCreatePaymentResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                    throw new InvalidOperationException("Failed to parse PayOS response");

                if (result.Code != "00")
                {
                    var errorMessage = result.Desc ?? "Unknown error";
                    var errorCode = result.Code ?? "Unknown";
                    throw new InvalidOperationException($"PayOS error: {errorMessage} (Code: {errorCode})");
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating PayOS package refund payment link: {ex.Message}", ex);
            }
        }
    }
}
