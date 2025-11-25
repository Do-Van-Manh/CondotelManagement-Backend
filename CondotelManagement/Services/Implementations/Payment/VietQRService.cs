using CondotelManagement.DTOs.Payment;
using CondotelManagement.Services.Interfaces.Payment;
using System.Text.Json;

namespace CondotelManagement.Services.Implementations.Payment
{
    public class VietQRService : IVietQRService
    {
        private readonly HttpClient _httpClient;

        public VietQRService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<VietQRBankListResponse> GetBanksAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/v2/banks");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"VietQR API error: {response.StatusCode} - {responseContent}");
                }

                var result = JsonSerializer.Deserialize<VietQRBankListResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? throw new InvalidOperationException("Failed to parse VietQR response");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting banks from VietQR: {ex.Message}", ex);
            }
        }
    }
}
