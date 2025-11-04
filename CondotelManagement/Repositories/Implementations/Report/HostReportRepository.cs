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

            int totalRooms = condotelIds.Count;

            int totalBookings = await bookings.CountAsync();

            int totalCancellations = await bookings
                .Where(b => b.Status == "Cancelled")
                .CountAsync();

            int roomsBooked = await bookings
                .Where(b => b.Status == "Confirmed" || b.Status == "Completed")
                .Select(b => b.CondotelId)
                .Distinct()
                .CountAsync();

            decimal revenue = await bookings
                .Where(b => b.Status == "Completed")
                .SumAsync(b => (decimal?)b.TotalPrice ?? 0m);

            // Nếu không truyền from/to → lấy toàn bộ khoảng thời gian
            var minDate = await _db.Bookings.MinAsync(b => b.StartDate);
            var maxDate = await _db.Bookings.MaxAsync(b => b.EndDate);
            var fromDate = from ?? minDate;
            var toDate = to ?? maxDate;

            // tính days
            var days = (toDate.ToDateTime(TimeOnly.MinValue) - fromDate.ToDateTime(TimeOnly.MinValue)).TotalDays + 1;

            double bookedNights = await bookings
                .Where(b => b.Status != "Cancelled")
                .SumAsync(b =>
                    (double)EF.Functions.DateDiffDay(
                        b.StartDate < from ? from : b.StartDate,
                        b.EndDate > to ? to : b.EndDate
                    )
                );

            double possibleNights = totalRooms * days;
            //Occupancy = (Booked Nights / (Total Rooms * Number of Days)) * 100
            //Occupancy rate = số đêm phòng được đặt / số đêm phòng có thể bán trong khoảng thời gian đó
            double occupancyRate = possibleNights > 0 ? (bookedNights / possibleNights) * 100 : 0;

            return new HostReportDTO
            {
                Revenue = Math.Round(revenue, 2),
                TotalRooms = totalRooms,
                RoomsBooked = roomsBooked,
                OccupancyRate = Math.Round(occupancyRate, 2),
                TotalBookings = totalBookings,
                TotalCancellations = totalCancellations
            };
        }
    }
}
