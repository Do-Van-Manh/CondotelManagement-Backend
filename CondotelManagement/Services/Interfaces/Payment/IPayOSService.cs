using CondotelManagement.DTOs.Payment;

namespace CondotelManagement.Services.Interfaces.Payment
{
    public interface IPayOSService
    {
        Task<PayOSCreatePaymentResponse> CreatePaymentLinkAsync(PayOSCreatePaymentRequest request);
        Task<PayOSPaymentInfo> GetPaymentInfoAsync(string paymentLinkId);
        Task<PayOSPaymentInfo?> GetPaymentInfoByOrderCodeAsync(long orderCode);
        Task<PayOSCreatePaymentResponse> CancelPaymentLinkAsync(string paymentLinkId, string? cancellationReason = null);
        Task<PayOSCreatePaymentResponse> CancelPaymentLinkByOrderCodeAsync(long orderCode, string? cancellationReason = null);
        bool VerifyWebhookSignature(string signature, string body);
        Task<bool> ProcessWebhookAsync(PayOSWebhookData webhookData);
    }
}

