using CondotelManagement.DTOs.Payment;
using CondotelManagement.Services.Interfaces.Payment;
using CondotelManagement.Services.Interfaces.BookingService;
using CondotelManagement.Services;
using CondotelManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace CondotelManagement.Controllers.Payment
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IPayOSService _payOSService;
        private readonly CondotelDbVer1Context _context;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IPayOSService payOSService,
            CondotelDbVer1Context context,
            IConfiguration configuration)
        {
            _payOSService = payOSService;
            _context = context;
            _configuration = configuration;
        }

        // Backward compatibility endpoint
        [HttpPost("create")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            return await CreatePayOSPayment(request);
        }

        [HttpPost("payos/create")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> CreatePayOSPayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                // Validate user
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user" });
                }

                // Get booking details
                var booking = await _context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.Condotel)
                    .FirstOrDefaultAsync(b => b.BookingId == request.BookingId);

                if (booking == null)
                {
                    return NotFound(new { success = false, message = "Booking not found" });
                }

                if (booking.CustomerId != userId)
                {
                    return Forbid("Access denied");
                }

                if (booking.Status != "Pending")
                {
                    return BadRequest(new { success = false, message = "Booking is not in a payable state. Current status: " + booking.Status });
                }

                if (booking.TotalPrice == null || booking.TotalPrice <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid booking amount. TotalPrice: " + booking.TotalPrice });
                }

                // PayOS minimum amount is 10,000 VND
                if (booking.TotalPrice < 10000)
                {
                    return BadRequest(new { success = false, message = "Amount must be at least 10,000 VND. Current amount: " + booking.TotalPrice });
                }

                // Tạo OrderCode unique: BookingId * 1000000 + random 6 digits
                // Đảm bảo OrderCode unique và có thể extract BookingId từ OrderCode
                var random = new Random();
                var randomSuffix = random.Next(100000, 999999); // 6 chữ số ngẫu nhiên
                var orderCode = (long)request.BookingId * 1000000L + randomSuffix;

                Console.WriteLine($"Creating payment - BookingId: {request.BookingId}, OrderCode: {orderCode}, TotalPrice: {booking.TotalPrice}");

                // Create PayOS payment request
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
                var backendBaseUrl = _configuration["AppSettings:BackendBaseUrl"];
                if (string.IsNullOrWhiteSpace(backendBaseUrl))
                {
                    if (Request?.Host.HasValue == true)
                    {
                        backendBaseUrl = $"{Request.Scheme}://{Request.Host}";
                    }
                    else
                    {
                        backendBaseUrl = frontendUrl;
                    }
                }
                backendBaseUrl = backendBaseUrl.TrimEnd('/');
                // PayOS yêu cầu amount là VND (không phải cents)
                // Nhưng để đảm bảo, kiểm tra cả hai format
                var totalPriceVnd = (int)booking.TotalPrice.Value;
                var amount = totalPriceVnd; // Sử dụng VND trực tiếp

                // PayOS minimum amount is 10,000 VND
                if (amount < 10000)
                {
                    return BadRequest(new { success = false, message = "Amount must be at least 10,000 VND" });
                }

                // PayOS description must be <= 25 characters
                const int payOsDescriptionLimit = 25;
                var condotelName = booking.Condotel?.Name?.Trim();
                var baseDescription = !string.IsNullOrWhiteSpace(condotelName)
                    ? $"{condotelName} #{request.BookingId}"
                    : $"Condotel #{request.BookingId}";
                var description = baseDescription.Length > payOsDescriptionLimit
                    ? baseDescription.Substring(0, payOsDescriptionLimit)
                    : baseDescription;


                // Route PayOS callbacks back to API (it will redirect to frontend after processing)
                var returnUrl = $"{backendBaseUrl}/api/payment/payos/return";
                var cancelUrl = returnUrl;
                var payOSRequest = new PayOSCreatePaymentRequest
                {
                    OrderCode = orderCode,
                    Amount = amount,
                    Description = description,
                    BuyerName = !string.IsNullOrWhiteSpace(booking.Customer?.FullName) ? booking.Customer.FullName : null,
                    BuyerPhone = !string.IsNullOrWhiteSpace(booking.Customer?.Phone) ? booking.Customer.Phone : null,
                    BuyerEmail = !string.IsNullOrWhiteSpace(booking.Customer?.Email) ? booking.Customer.Email : null,
                    Items = new List<PayOSItem>
                    {
                        new PayOSItem
                        {
                            // Giữ nguyên ký tự tiếng Việt trong item name
                            Name = !string.IsNullOrWhiteSpace(booking.Condotel?.Name)
                                ? booking.Condotel.Name
                                : $"Đặt phòng #{request.BookingId}",
                            Quantity = 1,
                            Price = amount
                        }
                    },
                    CancelUrl = cancelUrl,
                    ReturnUrl = returnUrl,
                    ExpiredAt = null // Không gửi ExpiredAt
                };

                PayOSCreatePaymentResponse? response = null;
                int maxRetries = 3;
                int retryCount = 0;

                while (retryCount < maxRetries)
                {
                    try
                    {
                        // Tạo payment link mới
                        response = await _payOSService.CreatePaymentLinkAsync(payOSRequest);

                        // Nếu thành công, break khỏi loop
                        if (response.Code == "00")
                        {
                            break;
                        }
                        else
                        {
                            throw new InvalidOperationException($"PayOS error: {response.Desc} (Code: {response.Code})");
                        }
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("Code: 20") && retryCount < maxRetries - 1)
                    {
                        retryCount++;
                        Console.WriteLine($"Code: 20 error received (attempt {retryCount}/{maxRetries}). Generating new orderCode...");

                        // Tạo orderCode mới với random suffix khác
                        var newRandom = new Random();
                        var newRandomSuffix = newRandom.Next(100000, 999999);
                        orderCode = (long)request.BookingId * 1000000L + newRandomSuffix;
                        payOSRequest.OrderCode = orderCode;

                        Console.WriteLine($"New OrderCode: {orderCode}");
                        await Task.Delay(1000); // Đợi một chút trước khi retry
                    }
                    catch (Exception ex)
                    {
                        // Nếu không phải Code: 20 hoặc đã hết retry, throw exception
                        throw;
                    }
                }

                // Kiểm tra response có giá trị
                if (response == null)
                {
                    throw new InvalidOperationException("Failed to create payment link after retries");
                }

                // Nếu vẫn lỗi sau khi retry
                if (response.Code != "00")
                {
                    throw new InvalidOperationException($"PayOS error: {response.Desc} (Code: {response.Code})");
                }

                if (response.Code == "00" && response.Data != null)
                {
                    // Store paymentLinkId and orderCode for future reference (optional - can be stored in database)
                    Console.WriteLine($"Payment link created - BookingId: {request.BookingId}, OrderCode: {response.Data.OrderCode}, PaymentLinkId: {response.Data.PaymentLinkId}");

                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            checkoutUrl = response.Data.CheckoutUrl,
                            paymentLinkId = response.Data.PaymentLinkId,
                            qrCode = response.Data.QrCode,
                            amount = response.Data.Amount,
                            orderCode = response.Data.OrderCode,
                            bookingId = request.BookingId // Include bookingId for reference
                        }
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = response.Desc ?? "Failed to create payment link"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create PayOS Payment Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Check if it's a connection error
                if (ex.Message.Contains("Cannot connect to PayOS API") ||
                    ex.Message.Contains("timed out") ||
                    ex.InnerException is System.Net.Http.HttpRequestException)
                {
                    return StatusCode(503, new
                    {
                        success = false,
                        message = ex.Message
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message.Contains("PayOS") ? ex.Message : "Internal server error"
                });
            }
        }

        [HttpGet("payos/status/{paymentLinkId}")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> GetPaymentStatus(string paymentLinkId)
        {
            try
            {
                var paymentInfo = await _payOSService.GetPaymentInfoAsync(paymentLinkId);

                if (paymentInfo.Code == "00" && paymentInfo.Data != null)
                {
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            status = paymentInfo.Data.Status,
                            amount = paymentInfo.Data.Amount,
                            amountPaid = paymentInfo.Data.AmountPaid,
                            amountRemaining = paymentInfo.Data.AmountRemaining,
                            orderCode = paymentInfo.Data.OrderCode,
                            transactions = paymentInfo.Data.Transactions
                        }
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = paymentInfo.Desc ?? "Failed to get payment status"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get Payment Status Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Check if it's a connection error
                if (ex.Message.Contains("Cannot connect to PayOS API") ||
                    ex.Message.Contains("timed out") ||
                    ex.InnerException is System.Net.Http.HttpRequestException)
                {
                    return StatusCode(503, new
                    {
                        success = false,
                        message = ex.Message
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message.Contains("PayOS") ? ex.Message : "Internal server error"
                });
            }
        }

        /// <summary>
        /// Handle PayOS Return URL callback
        /// PayOS redirects user to this URL after payment with query params:
        /// - code: "00" (success) or "01" (Invalid Params)
        /// - id: Payment Link Id (string)
        /// - cancel: "true" (cancelled) or "false" (paid/pending)
        /// - status: "PAID", "PENDING", "PROCESSING", "CANCELLED"
        /// - orderCode: Order code (number)
        /// </summary>
        [HttpGet("payos/return")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSReturnUrl(
            [FromQuery] string? code,
            [FromQuery] string? id,
            [FromQuery] string? cancel,
            [FromQuery] string? status,
            [FromQuery] long? orderCode)
        {
            try
            {
                Console.WriteLine($"PayOS Return URL called - code: {code}, id: {id}, cancel: {cancel}, status: {status}, orderCode: {orderCode}");

                // Khai báo frontendUrl một lần ở đầu method
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";

                // Validate required params
                if (orderCode == null)
                {
                    Console.WriteLine("Missing orderCode in Return URL");
                    return Redirect($"{frontendUrl}/payment-error?message=Missing orderCode");
                }

                // Check if code is valid (00 = success, 01 = Invalid Params)
                if (code == "01")
                {
                    Console.WriteLine($"PayOS returned error code: {code}");
                    return Redirect($"{frontendUrl}/payment-error?message=Invalid payment parameters");
                }

                // Extract BookingId from OrderCode
                // OrderCode format: BookingId * 1000000 + random 6 digits (hoặc 999999 cho refund)
                var orderCodeSuffix = orderCode.Value % 1000000;
                var isRefundPayment = orderCodeSuffix == 999999;
                var bookingId = (int)(orderCode.Value / 1000000);

                if (isRefundPayment)
                {
                    // === XỬ LÝ REFUND PAYMENT ===
                    var refundRequest = await _context.RefundRequests
                        .Include(r => r.Booking)
                        .FirstOrDefaultAsync(r => r.BookingId == bookingId && r.Status == "Pending");

                    if (refundRequest == null)
                    {
                        Console.WriteLine($"RefundRequest not found for booking {bookingId} and orderCode {orderCode}");
                        return Redirect($"{frontendUrl}/refund-error?message=Refund request not found");
                    }

                    if (status == "PAID" && cancel != "true")
                    {
                        // Refund payment successful - customer đã nhận tiền
                        refundRequest.Status = "Refunded";
                        refundRequest.ProcessedAt = DateTime.UtcNow;
                        refundRequest.UpdatedAt = DateTime.UtcNow;
                        // Booking status giữ nguyên "Cancelled", không đổi thành "Refunded"
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"RefundRequest {refundRequest.Id} (Booking {bookingId}) status updated to Refunded from Return URL (Payment Link Id: {id})");
                        return Redirect($"{frontendUrl}/refund-success?bookingId={bookingId}&status=success&orderCode={orderCode}&paymentLinkId={id}");
                    }
                    else if (status == "CANCELLED" || cancel == "true")
                    {
                        // Refund payment was cancelled
                        Console.WriteLine($"Refund payment cancelled for booking {bookingId} (Payment Link Id: {id})");
                        return Redirect($"{frontendUrl}/refund-cancel?bookingId={bookingId}&status=cancelled&orderCode={orderCode}&paymentLinkId={id}");
                    }
                    else if (status == "PENDING" || status == "PROCESSING")
                    {
                        // Refund payment is still pending/processing
                        Console.WriteLine($"Refund payment pending/processing for booking {bookingId} (Payment Link Id: {id})");
                        return Redirect($"{frontendUrl}/refund-pending?bookingId={bookingId}&status=pending&orderCode={orderCode}");
                    }
                    else
                    {
                        // Unknown status
                        Console.WriteLine($"Unknown refund payment status '{status}' for booking {bookingId}");
                        return Redirect($"{frontendUrl}/refund-pending?bookingId={bookingId}&status=unknown&orderCode={orderCode}");
                    }
                }

                // === XỬ LÝ BOOKING PAYMENT (BÌNH THƯỜNG) ===
                // Sử dụng transaction với Serializable isolation để tránh race condition
                using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    // Lock booking row để tránh race condition
                    var booking = await _context.Bookings
                        .FromSqlRaw(
                            "SELECT * FROM Booking WITH (UPDLOCK, ROWLOCK) WHERE BookingId = @bookingId",
                            new Microsoft.Data.SqlClient.SqlParameter("@bookingId", bookingId))
                        .FirstOrDefaultAsync();

                    if (booking == null)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Booking {bookingId} not found for orderCode {orderCode}");
                        return Redirect($"{frontendUrl}/payment-error?message=Booking not found");
                    }

                    // Process based on status and cancel flag
                    // Status values: PAID, PENDING, PROCESSING, CANCELLED
                    // Cancel: "true" = cancelled, "false" = paid/pending

                    if (status == "PAID" && cancel != "true")
                    {
                        // Payment successful
                        if (booking.Status == "Confirmed" || booking.Status == "Completed")
                        {
                            // Đã confirm rồi → bỏ qua
                            await transaction.CommitAsync();
                            Console.WriteLine($"[Return URL] Booking {bookingId} đã xác nhận trước đó → bỏ qua");
                            return Redirect($"{frontendUrl}/pay-done?bookingId={bookingId}&status=success&orderCode={orderCode}&paymentLinkId={id}");
                        }

                        // Kiểm tra availability trước khi confirm để tránh double booking
                        var today = DateOnly.FromDateTime(DateTime.UtcNow);
                        var conflictingBookings = await _context.Bookings
                            .FromSqlRaw(@"
                                SELECT * FROM Booking WITH (UPDLOCK, ROWLOCK)
                                WHERE CondotelId = @condotelId 
                                AND BookingId != @currentBookingId
                                AND Status IN ('Confirmed', 'Completed', 'Pending')
                                AND Status != 'Cancelled'
                                AND EndDate >= @today",
                                new Microsoft.Data.SqlClient.SqlParameter("@condotelId", booking.CondotelId),
                                new Microsoft.Data.SqlClient.SqlParameter("@currentBookingId", booking.BookingId),
                                new Microsoft.Data.SqlClient.SqlParameter("@today", today))
                            .ToListAsync();

                        // Kiểm tra overlap với các booking đã confirmed/completed
                        var hasConflict = conflictingBookings
                            .Where(b => b.Status == "Confirmed" || b.Status == "Completed")
                            .Any(b => !(booking.EndDate <= b.StartDate || booking.StartDate >= b.EndDate));

                        if (hasConflict)
                        {
                            // Có conflict với booking đã confirmed/completed → không thể confirm
                            Console.WriteLine($"[Return URL] Booking {bookingId} có conflict với booking đã confirmed/completed → hủy booking");
                            booking.Status = "Cancelled";
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            
                            // TODO: Có thể gửi email thông báo cho customer về việc booking bị hủy do conflict
                            return Redirect($"{frontendUrl}/payment-error?message=Condotel không còn trống trong khoảng thời gian này. Đặt phòng đã bị hủy.");
                        }

                        // Không có conflict → confirm booking
                        booking.Status = "Confirmed";
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        
                        // Tăng Voucher UsedCount khi payment thành công (từ Return URL)
                        if (booking.VoucherId.HasValue)
                        {
                            try
                            {
                                using var scope = HttpContext.RequestServices.CreateScope();
                                var voucherService = scope.ServiceProvider.GetRequiredService<IVoucherService>();
                                await voucherService.ApplyVoucherToBookingAsync(booking.VoucherId.Value);
                                Console.WriteLine($"[Return URL] Đã tăng UsedCount cho Voucher {booking.VoucherId.Value}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[Return URL] Lỗi khi tăng Voucher UsedCount: {ex.Message}");
                            }
                        }
                        
                        Console.WriteLine($"Booking {bookingId} status updated to Confirmed from Return URL (Payment Link Id: {id})");
                        return Redirect($"{frontendUrl}/pay-done?bookingId={bookingId}&status=success&orderCode={orderCode}&paymentLinkId={id}");
                    }
                    else
                    {
                        await transaction.CommitAsync();
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"[Return URL] Lỗi khi xử lý booking payment: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    return Redirect($"{frontendUrl}/payment-error?message=Error processing payment");
                }

                // Xử lý các trường hợp khác (CANCELLED, PENDING, etc.)
                if (status == "CANCELLED" || cancel == "true")
                {
                    // Payment was cancelled - Hủy thanh toán (KHÔNG refund)
                    Console.WriteLine($"Payment cancelled for booking {bookingId} (Payment Link Id: {id})");

                    var bookingForCancel = await _context.Bookings
                        .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                    if (bookingForCancel == null)
                    {
                        return Redirect($"{frontendUrl}/payment-error?message=Booking not found");
                    }

                    if (bookingForCancel.Status != "Cancelled")
                    {
                        // Sử dụng CancelPayment để đảm bảo không refund
                        try
                        {
                            using var scope = HttpContext.RequestServices.CreateScope();
                            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                            var customerId = bookingForCancel.CustomerId;
                            var cancelResult = await bookingService.CancelPayment(bookingId, customerId);
                            
                            if (cancelResult)
                            {
                                Console.WriteLine($"Booking {bookingId} status updated to Cancelled from Return URL (Payment Link Id: {id}) - Payment cancelled, no refund");
                            }
                            else
                            {
                                // Fallback: set status manually nếu CancelPayment fail
                                bookingForCancel.Status = "Cancelled";
                                await _context.SaveChangesAsync();
                                Console.WriteLine($"Booking {bookingId} status updated to Cancelled (fallback) from Return URL");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error calling CancelPayment: {ex.Message}");
                            // Fallback: set status manually
                            bookingForCancel.Status = "Cancelled";
                            await _context.SaveChangesAsync();
                            Console.WriteLine($"Booking {bookingId} status updated to Cancelled (fallback) from Return URL");
                        }
                    }

                    return Redirect($"{frontendUrl}/payment/cancel?bookingId={bookingId}&status=cancelled&orderCode={orderCode}&paymentLinkId={id}");
                }
                else if (status == "PENDING" || status == "PROCESSING")
                {
                    // Payment is still pending/processing
                    Console.WriteLine($"Payment pending/processing for booking {bookingId} (Payment Link Id: {id})");
                    return Redirect($"{frontendUrl}/checkout?bookingId={bookingId}&status=pending&orderCode={orderCode}");
                }
                else
                {
                    // Unknown status
                    Console.WriteLine($"Unknown payment status '{status}' for booking {bookingId}");
                    return Redirect($"{frontendUrl}/checkout?bookingId={bookingId}&status=unknown&orderCode={orderCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PayOS Return URL Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
                return Redirect($"{frontendUrl}/payment-error?message={Uri.EscapeDataString(ex.Message)}");
            }
        }

        [HttpPost("payos/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSWebhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                Console.WriteLine($"PayOS Webhook received: {body}");

                // Log all headers
                foreach (var h in Request.Headers)
                {
                    Console.WriteLine($"Header: {h.Key} = {h.Value}");
                }

                // Get signature from header or body
                var signature = Request.Headers["signature"].FirstOrDefault()
                    ?? Request.Headers["Signature"].FirstOrDefault()
                    ?? Request.Headers["X-Signature"].FirstOrDefault();

                if (string.IsNullOrEmpty(signature))
                {
                    // Try to get from body
                    try
                    {
                        using var doc = JsonDocument.Parse(body);
                        if (doc.RootElement.TryGetProperty("signature", out var sigProp))
                        {
                            signature = sigProp.GetString();
                        }
                    }
                    catch { }
                }

                if (string.IsNullOrEmpty(signature))
                {
                    Console.WriteLine("Missing signature in webhook (header + body)");
                    return BadRequest(new { message = "Missing signature" });
                }

                if (!_payOSService.VerifyWebhookSignature(signature, body))
                {
                    Console.WriteLine("Invalid webhook signature");
                    return BadRequest(new { message = "Invalid signature" });
                }

                // Parse webhook data
                // PayOS webhook format: camelCase properties
                var webhookData = JsonSerializer.Deserialize<PayOSWebhookData>(body, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                });

                if (webhookData == null)
                {
                    Console.WriteLine("Invalid webhook data - failed to parse");
                    return BadRequest(new { message = "Invalid webhook data" });
                }

                Console.WriteLine($"Webhook parsed - Code: {webhookData.Code}, Desc: {webhookData.Desc}, Success: {webhookData.Success}, HasData: {webhookData.Data != null}");

                // Process webhook
                var processed = await _payOSService.ProcessWebhookAsync(webhookData);

                if (processed)
                {
                    return Ok(new { message = "Webhook processed successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to process webhook" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PayOS Webhook Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("payos/cancel/{paymentLinkId}")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> CancelPayment(string paymentLinkId, [FromBody] CancelPaymentRequest? request = null)
        {
            try
            {
                var result = await _payOSService.CancelPaymentLinkAsync(paymentLinkId, request?.Reason ?? "User cancelled");

                if (result.Code == "00")
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Payment cancelled successfully"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Desc ?? "Failed to cancel payment"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cancel Payment Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Check if it's a connection error
                if (ex.Message.Contains("Cannot connect to PayOS API") ||
                    ex.Message.Contains("timed out") ||
                    ex.InnerException is System.Net.Http.HttpRequestException)
                {
                    return StatusCode(503, new
                    {
                        success = false,
                        message = ex.Message
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message.Contains("PayOS") ? ex.Message : "Internal server error"
                });
            }
        }

        /// <summary>
        /// Test endpoint to verify PayOS connection
        /// </summary>
        [HttpGet("payos/test")]
        [AllowAnonymous]
        public IActionResult TestPayOSConnection()
        {
            try
            {
                var clientId = _configuration["PayOS:ClientId"];
                var apiKey = _configuration["PayOS:ApiKey"];
                var baseUrl = _configuration["PayOS:BaseUrl"];

                return Ok(new
                {
                    success = true,
                    message = "PayOS configuration loaded",
                    config = new
                    {
                        baseUrl = baseUrl,
                        clientIdConfigured = !string.IsNullOrEmpty(clientId),
                        apiKeyConfigured = !string.IsNullOrEmpty(apiKey),
                        clientIdPreview = clientId?.Substring(0, Math.Min(8, clientId?.Length ?? 0)) + "...",
                        apiKeyPreview = apiKey?.Substring(0, Math.Min(8, apiKey?.Length ?? 0)) + "..."
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("payos/cancel-by-booking/{bookingId}")]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> CancelPaymentByBooking(int bookingId, [FromBody] CancelPaymentRequest? request = null)
        {
            try
            {
                // Validate user
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user" });
                }

                // Get booking
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    return NotFound(new { success = false, message = "Booking not found" });
                }

                if (booking.CustomerId != userId)
                {
                    return Forbid("Access denied");
                }

                // Try to cancel by OrderCode
                // Since OrderCode format is BookingId * 1000000 + random, we need to try different possible OrderCodes
                // Or better: PayOS might allow canceling by OrderCode range
                // For now, we'll try to cancel using the base OrderCode (BookingId * 1000000)
                // But this won't work if the random suffix is needed

                // Better approach: Try to get payment info using OrderCode pattern
                // Since we can't know exact OrderCode, we'll need to store it when creating payment
                // For now, return error asking for paymentLinkId

                return BadRequest(new
                {
                    success = false,
                    message = "Cannot cancel payment without exact OrderCode. Please use the paymentLinkId returned when creating the payment link, or use endpoint: POST /api/payment/payos/cancel/{paymentLinkId}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cancel Payment By Booking Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // Helper method to convert Vietnamese to ASCII
        private static string ConvertVietnameseToAscii(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            var result = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);

            // Replace specific Vietnamese characters
            var replacements = new Dictionary<string, string>
            {
                { "đ", "d" }, { "Đ", "D" },
                { "à", "a" }, { "á", "a" }, { "ạ", "a" }, { "ả", "a" }, { "ã", "a" },
                { "â", "a" }, { "ầ", "a" }, { "ấ", "a" }, { "ậ", "a" }, { "ẩ", "a" }, { "ẫ", "a" },
                { "ă", "a" }, { "ằ", "a" }, { "ắ", "a" }, { "ặ", "a" }, { "ẳ", "a" }, { "ẵ", "a" },
                { "è", "e" }, { "é", "e" }, { "ẹ", "e" }, { "ẻ", "e" }, { "ẽ", "e" },
                { "ê", "e" }, { "ề", "e" }, { "ế", "e" }, { "ệ", "e" }, { "ể", "e" }, { "ễ", "e" },
                { "ì", "i" }, { "í", "i" }, { "ị", "i" }, { "ỉ", "i" }, { "ĩ", "i" },
                { "ò", "o" }, { "ó", "o" }, { "ọ", "o" }, { "ỏ", "o" }, { "õ", "o" },
                { "ô", "o" }, { "ồ", "o" }, { "ố", "o" }, { "ộ", "o" }, { "ổ", "o" }, { "ỗ", "o" },
                { "ơ", "o" }, { "ờ", "o" }, { "ớ", "o" }, { "ợ", "o" }, { "ở", "o" }, { "ỡ", "o" },
                { "ù", "u" }, { "ú", "u" }, { "ụ", "u" }, { "ủ", "u" }, { "ũ", "u" },
                { "ư", "u" }, { "ừ", "u" }, { "ứ", "u" }, { "ự", "u" }, { "ử", "u" }, { "ữ", "u" },
                { "ỳ", "y" }, { "ý", "y" }, { "ỵ", "y" }, { "ỷ", "y" }, { "ỹ", "y" },
                { "À", "A" }, { "Á", "A" }, { "Ạ", "A" }, { "Ả", "A" }, { "Ã", "A" },
                { "Â", "A" }, { "Ầ", "A" }, { "Ấ", "A" }, { "Ậ", "A" }, { "Ẩ", "A" }, { "Ẫ", "A" },
                { "Ă", "A" }, { "Ằ", "A" }, { "Ắ", "A" }, { "Ặ", "A" }, { "Ẳ", "A" }, { "Ẵ", "A" },
                { "È", "E" }, { "É", "E" }, { "Ẹ", "E" }, { "Ẻ", "E" }, { "Ẽ", "E" },
                { "Ê", "E" }, { "Ề", "E" }, { "Ế", "E" }, { "Ệ", "E" }, { "Ể", "E" }, { "Ễ", "E" },
                { "Ì", "I" }, { "Í", "I" }, { "Ị", "I" }, { "Ỉ", "I" }, { "Ĩ", "I" },
                { "Ò", "O" }, { "Ó", "O" }, { "Ọ", "O" }, { "Ỏ", "O" }, { "Õ", "O" },
                { "Ô", "O" }, { "Ồ", "O" }, { "Ố", "O" }, { "Ộ", "O" }, { "Ổ", "O" }, { "Ỗ", "O" },
                { "Ơ", "O" }, { "Ờ", "O" }, { "Ớ", "O" }, { "Ợ", "O" }, { "Ở", "O" }, { "Ỡ", "O" },
                { "Ù", "U" }, { "Ú", "U" }, { "Ụ", "U" }, { "Ủ", "U" }, { "Ũ", "U" },
                { "Ư", "U" }, { "Ừ", "U" }, { "Ứ", "U" }, { "Ự", "U" }, { "Ử", "U" }, { "Ữ", "U" },
                { "Ỳ", "Y" }, { "Ý", "Y" }, { "Ỵ", "Y" }, { "Ỷ", "Y" }, { "Ỹ", "Y" }
            };

            foreach (var replacement in replacements)
            {
                result = result.Replace(replacement.Key, replacement.Value);
            }

            return result;
        }

        /// <summary>
        /// Tạo QR code để chuyển tiền với thông tin: tên, tài khoản, số tiền
        /// </summary>
        [HttpPost("generate-qr")]
        [AllowAnonymous]
        public IActionResult GenerateQRCode([FromBody] GenerateQRRequestDTO request)
        {
            if (request == null)
                return BadRequest(new { success = false, message = "Request body is required" });

            if (string.IsNullOrWhiteSpace(request.BankCode))
                return BadRequest(new { success = false, message = "Bank code is required" });

            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                return BadRequest(new { success = false, message = "Account number is required" });

            if (request.Amount < 1000)
                return BadRequest(new { success = false, message = "Amount must be at least 1,000 VND" });

            if (string.IsNullOrWhiteSpace(request.AccountHolderName))
                return BadRequest(new { success = false, message = "Account holder name is required" });

            try
            {
                // Tạo nội dung chuyển khoản
                var content = string.IsNullOrWhiteSpace(request.Content) 
                    ? "Chuyen tien" 
                    : request.Content;

                // Encode các tham số
                var encodedContent = Uri.EscapeDataString(content);
                var encodedAccountName = Uri.EscapeDataString(request.AccountHolderName);

                // Tạo URL QR code từ VietQR
                // Format: https://img.vietqr.io/image/{bankCode}-{accountNumber}-{template}.jpg?amount={amount}&addInfo={content}&accountName={accountName}
                var baseUrl = "https://img.vietqr.io/image";
                var qrCodeUrlCompact = $"{baseUrl}/{request.BankCode}-{request.AccountNumber}-compact.jpg?amount={request.Amount}&addInfo={encodedContent}&accountName={encodedAccountName}";
                var qrCodeUrlPrint = $"{baseUrl}/{request.BankCode}-{request.AccountNumber}-print.jpg?amount={request.Amount}&addInfo={encodedContent}&accountName={encodedAccountName}";

                var response = new GenerateQRResponseDTO
                {
                    QrCodeUrl = qrCodeUrlCompact, // Default URL
                    QrCodeUrlCompact = qrCodeUrlCompact,
                    QrCodeUrlPrint = qrCodeUrlPrint,
                    BankCode = request.BankCode,
                    AccountNumber = request.AccountNumber,
                    Amount = request.Amount,
                    AccountHolderName = request.AccountHolderName,
                    Content = content
                };

                return Ok(new
                {
                    success = true,
                    message = "QR code generated successfully",
                    data = response
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Generate QR Code Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error generating QR code: " + ex.Message
                });
            }
        }

        // 1. SỬA DTO: OrderCode là string
        public class CreatePackagePaymentRequest
        {
            public string OrderCode { get; set; } = string.Empty;  // ← STRING LUÔN!
            public int Amount { get; set; }
            public string? Description { get; set; }
        }

        [HttpPost("create-package-payment")]
        [Authorize]
        public async Task<IActionResult> CreatePackagePayment([FromBody] CreatePackagePaymentRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OrderCode) || request.Amount <= 0)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

            try
            {
                // FIX DUY NHẤT – XỬ LÝ ORDERCODE LỚN CHÍNH XÁC 100%
                long orderCodeLong;
                if (!long.TryParse(request.OrderCode.Trim(), System.Globalization.NumberStyles.None, null, out orderCodeLong))
                {
                    // Nếu là dạng khoa học (e+24), ép về string nguyên rồi parse lại
                    var cleanOrderCode = new string(request.OrderCode.Where(char.IsDigit).ToArray());
                    if (!long.TryParse(cleanOrderCode, out orderCodeLong))
                    {
                        return BadRequest(new { success = false, message = "OrderCode không hợp lệ" });
                    }
                }
                // ĐẾN ĐÂY LÀ ORDERCODE ĐÃ LÀ LONG CHÍNH XÁC 100%!!!

                var description = string.IsNullOrWhiteSpace(request.Description)
                    ? "Nâng cấp gói dịch vụ"
                    : request.Description.Length > 25 ? request.Description.Substring(0, 25) : request.Description;

                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
                var returnUrl = $"{frontendUrl}/payment/success?type=package";
                var cancelUrl = $"{frontendUrl}/pricing";

                var payOSRequest = new PayOSCreatePaymentRequest
                {
                    OrderCode = orderCodeLong,  // BÂY GIỜ ĐÃ AN TOÀN HOÀN TOÀN
                    Amount = request.Amount,
                    Description = description,
                    Items = new List<PayOSItem>
        {
            new PayOSItem { Name = description, Quantity = 1, Price = request.Amount }
        },
                    ReturnUrl = returnUrl,
                    CancelUrl = cancelUrl
                };

                var response = await _payOSService.CreatePaymentLinkAsync(payOSRequest);

                if (response?.Code == "00" && response.Data != null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Tạo link thanh toán thành công!",
                        data = new
                        {
                            checkoutUrl = response.Data.CheckoutUrl,
                            qrCode = response.Data.QrCode ?? "",
                            orderCode = response.Data.OrderCode,
                            paymentLinkId = response.Data.PaymentLinkId
                        }
                    });
                }

                return BadRequest(new { success = false, message = response?.Desc ?? "Lỗi từ PayOS" });
            }
            catch (FormatException ex)
            {
                // ← Bắt riêng lỗi parse để dễ debug
                Console.WriteLine($"[CreatePackagePayment] OrderCode không hợp lệ: {request.OrderCode}");
                return StatusCode(500, new { success = false, message = "OrderCode không hợp lệ" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreatePackagePayment] ERROR: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống khi tạo link thanh toán" });
            }
        }
    }


        public class CreatePaymentRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "BookingId is required")]
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "BookingId must be greater than 0")]
        public int BookingId { get; set; }
    }

    public class CancelPaymentRequest
    {
        public string? Reason { get; set; }
    }
}
