using CondotelManagement.DTOs;
using CondotelManagement.Repositories;

namespace CondotelManagement.Services
{
    public class HostReportService : IHostReportService
    {
        private readonly IHostReportRepository _repo;

        public HostReportService(IHostReportRepository repo)
        {
            _repo = repo;
        }
        public async Task<HostReportDTO> GetReport(int hostId, DateOnly? from, DateOnly? to)
        {
            return await _repo.GetHostReportAsync(hostId, from, to);
        }
    }
}
