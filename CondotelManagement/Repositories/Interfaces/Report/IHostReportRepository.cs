using CondotelManagement.DTOs;

namespace CondotelManagement.Repositories
{
    public interface IHostReportRepository
    {
        Task<HostReportDTO> GetHostReportAsync(int hostId, DateOnly? from, DateOnly? to);
    }
}
