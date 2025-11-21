using CondotelManagement.DTOs.Payment;
using CondotelManagement.Services.Interfaces.Payment;
using CondotelManagement.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CondotelManagement.Services.Implementations.Payment
{
    public class PayOSService : IPayOSService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly CondotelDbVer1Context _context;
        private readonly string _clientId;
        private readonly string _apiKey;
        private readonly string _checksumKey;
        private readonly string _baseUrl;
        private const int PayOsDescriptionLimit = 25;

        public PayOSService(IConfiguration configuration, HttpClient httpClient, CondotelDbVer1Context context)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _context = context;
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


                // Tạo request object theo format chuẩn PayOS
                var requestBodyDict = new Dictionary<string, object>
                {
                    { "orderCode", request.OrderCode },
                    { "amount", request.Amount },
                    { "description", description },
                    { "returnUrl", returnUrl },
                    { "cancelUrl", cancelUrl },
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
                if (webhookData.Data == null)
                {
                    Console.WriteLine("Webhook data is null");
                    return false;
                }

                // Extract BookingId from OrderCode
                // OrderCode format: BookingId * 1000000 + random 6 digits
                var orderCode = webhookData.Data.OrderCode;
                var bookingId = (int)(orderCode / 1000000);

                Console.WriteLine($"Processing webhook - OrderCode: {orderCode}, BookingId: {bookingId}, Code: {webhookData.Code}, Data.Code: {webhookData.Data.Code}, Success: {webhookData.Success}");

                // Check if payment successful
                // PayOS webhook format:
                // - code: "00" (success) or other (error)
                // - success: true/false
                // - data.code: "00" (Thành công) or other
                // - data.desc: Description
                var isSuccess = (webhookData.Code == "00" || webhookData.Success == true) && 
                    (webhookData.Data.Code == "00" || webhookData.Data.Code == "PAID");
                
                if (isSuccess)
                {
                    // Get booking
                    var booking = await _context.Bookings
                        .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                    if (booking == null)
                    {
                        Console.WriteLine($"Booking {bookingId} not found for webhook");
                        return false;
                    }

                    // Check if already confirmed (idempotent)
                    if (booking.Status == "Confirmed" || booking.Status == "Completed")
                    {
                        Console.WriteLine($"Booking {bookingId} already confirmed, skipping update");
                        return true;
                    }

                    // Update booking status to Confirmed
                    booking.Status = "Confirmed";
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"Booking {bookingId} status updated to Confirmed from webhook (PaymentLinkId: {webhookData.Data.PaymentLinkId}, Reference: {webhookData.Data.Reference})");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Webhook indicates payment not successful - Code: {webhookData.Code}, Data.Code: {webhookData.Data.Code}, Desc: {webhookData.Data.Desc}");
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing webhook: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
