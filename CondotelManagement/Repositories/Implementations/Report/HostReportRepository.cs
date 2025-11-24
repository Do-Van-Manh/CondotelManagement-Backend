using CondotelManagement.Data;
using CondotelManagement.DTOs;
using Microsoft.EntityFrameworkCore;
using System;

namespace CondotelManagement.Repositories 
{
    public class HostReportRepository : IHostReportRepository
    {
        private readonly CondotelDbVer1Context _db;

        public HostReportRepository(CondotelDbVer1Context db)
        {
            _db = db;
        }
        public async Task<HostReportDTO> GetHostReportAsync(int hostId, DateOnly? from, DateOnly? to)
        {
            var condotelIds = await _db.Condotels
            .Where(c => c.HostId == hostId)
            .Select(c => c.CondotelId)
            .ToListAsync();

            var bookings = _db.Bookings
                .Where(b => condotelIds.Contains(b.CondotelId));

            // Nếu truyền from/to → lọc
            if (from.HasValue)
            {
                bookings = bookings.Where(b => b.EndDate >= from.Value);
            }

            if (to.HasValue)
            {
                bookings = bookings.Where(b => b.StartDate <= to.Value);
            }

			int totalRooms = await _db.CondotelDetails
	            .CountAsync(d => condotelIds.Contains(d.CondotelId));

			int totalBookings = await bookings.CountAsync();

            int totalCancellations = await bookings
                .Where(b => b.Status == "Cancelled")
                .CountAsync();

			// ======== TÍNH SỐ PHÒNG ĐANG ĐƯỢC ĐẶT ========
			// Vì mỗi booking condotel = đặt tất cả phòng trong condotel đó
			int roomsBooked = await bookings
				.Where(b => b.Status == "Confirmed")
				.Join(_db.CondotelDetails,
					  b => b.CondotelId,
					  d => d.CondotelId,
					  (b, d) => d.DetailId)
				.Distinct()
				.CountAsync();

			// ======== TÍNH DOANH THU ========
			decimal revenue = await bookings
				.Where(b => b.Status == "Completed")
				.SumAsync(b => (decimal?)b.TotalPrice ?? 0m);

			// ======== TÍNH OCCUPANCY RATE ========
			var minDate = await _db.Bookings.MinAsync(b => b.StartDate);
			var maxDate = await _db.Bookings.MaxAsync(b => b.EndDate);
			var fromDate = from ?? minDate;
			var toDate = to ?? maxDate;

			var days = (toDate.ToDateTime(TimeOnly.MinValue) - fromDate.ToDateTime(TimeOnly.MinValue)).TotalDays + 1;

			// bookedNights = tổng số đêm * số phòng condotel được đặt
			var bookedNights = await bookings
				.Where(b => b.Status != "Cancelled")
				.Join(_db.CondotelDetails,
					  b => b.CondotelId,
					  d => d.CondotelId,
					  (b, d) => new { b, d })
				.SumAsync(x =>
					(double)EF.Functions.DateDiffDay(
						x.b.StartDate < from ? from : x.b.StartDate,
						x.b.EndDate > to ? to : x.b.EndDate
					)
				);

			double possibleNights = totalRooms * days;
			//Occupancy = (Booked Nights / (Total Rooms * Number of Days)) * 100
			//Occupancy rate = số đêm phòng được đặt / số đêm phòng có thể bán trong khoảng thời gian đó
			double occupancyRate = possibleNights > 0 ? (bookedNights / possibleNights) * 100 : 0;

			int completedBookings = await bookings
				.Where(b => b.Status == "Completed")
				.CountAsync();

			return new HostReportDTO
			{
				Revenue = Math.Round(revenue, 2),
				TotalRooms = totalRooms,
				RoomsBooked = roomsBooked,
				OccupancyRate = Math.Round(occupancyRate, 2),
				TotalBookings = totalBookings,
				TotalCancellations = totalCancellations,
				CompletedBookings = completedBookings
			};
        }
    }
}
