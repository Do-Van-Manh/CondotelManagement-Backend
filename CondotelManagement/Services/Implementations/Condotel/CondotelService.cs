using CondotelManagement.DTOs;
using CondotelManagement.Models;
using CondotelManagement.Repositories;

namespace CondotelManagement.Services;
public class CondotelService : ICondotelService
{
    private readonly ICondotelRepository _condotelRepo;

    public CondotelService(ICondotelRepository condotelRepo)
    {
        _condotelRepo = condotelRepo;
    }
    public CondotelUpdateDTO CreateCondotel(CondotelCreateDTO dto)
    {
        var condotel = new Condotel
        {
            HostId = dto.HostId,
            ResortId = dto.ResortId,
            Name = dto.Name,
            Description = dto.Description,
            PricePerNight = dto.PricePerNight,
            Beds = dto.Beds,
            Bathrooms = dto.Bathrooms,
            Status = dto.Status,
            CondotelImages = dto.Images?.Select(i => new CondotelImage
            {
                ImageUrl = i.ImageUrl,
                Caption = i.Caption
            }).ToList(),
            CondotelPrices = dto.Prices?.Select(p => new CondotelPrice
            {
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                BasePrice = p.BasePrice,
                PriceType = p.PriceType,
                Description = p.Description
            }).ToList(),
            CondotelDetails = dto.Details?.Select(d => new CondotelDetail
            {
                BuildingName = d.BuildingName,
                RoomNumber = d.RoomNumber,
                Beds = d.Beds,
                Bathrooms = d.Bathrooms,
                SafetyFeatures = d.SafetyFeatures,
                HygieneStandards = d.HygieneStandards
            }).ToList(),
            CondotelAmenities = dto.AmenityIds?.Select(aid => new CondotelAmenity
            {
                AmenityId = aid,
                DateAdded = DateOnly.FromDateTime(DateTime.Now)
            }).ToList(),
            CondotelUtilities = dto.UtilityIds?.Select(uid => new CondotelUtility
            {
                UtilityId = uid,
                DateAdded = DateOnly.FromDateTime(DateTime.Now)
            }).ToList()
        };

        _condotelRepo.AddCondotel(condotel);
        _condotelRepo.SaveChanges();
        return new CondotelUpdateDTO
        {
            CondotelId = condotel.CondotelId,
            HostId = condotel.HostId,
            ResortId = condotel.ResortId,
            Name = condotel.Name,
            Description = condotel.Description,
            PricePerNight = condotel.PricePerNight,
            Beds = condotel.Beds,
            Bathrooms = condotel.Bathrooms,
            Status = condotel.Status,

            Images = condotel.CondotelImages?.Select(i => new ImageDTO
            {
                ImageId = i.ImageId,
                ImageUrl = i.ImageUrl,
                Caption = i.Caption
            }).ToList(),

            Prices = condotel.CondotelPrices?.Select(p => new PriceDTO
            {
                PriceId = p.PriceId,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                BasePrice = p.BasePrice,
                PriceType = p.PriceType,
                Description = p.Description
            }).ToList(),

            Details = condotel.CondotelDetails?.Select(d => new DetailDTO
            {
                BuildingName = d.BuildingName,
                RoomNumber = d.RoomNumber,
                Beds = d.Beds,
                Bathrooms = d.Bathrooms,
                SafetyFeatures = d.SafetyFeatures,
                HygieneStandards = d.HygieneStandards
            }).ToList(),

            AmenityIds = condotel.CondotelAmenities?.Select(a => a.AmenityId).ToList(),
            UtilityIds = condotel.CondotelUtilities?.Select(u => u.UtilityId).ToList()
        };
    }

    public bool DeleteCondotel(int id)
    {
        _condotelRepo.DeleteCondotel(id);
        return _condotelRepo.SaveChanges();
    }

    public CondotelDetailDTO GetCondotelById(int id)
    {
        var c = _condotelRepo.GetCondotelById(id);
        if (c == null) return null;

        return new CondotelDetailDTO
		{
            CondotelId = c.CondotelId,
            HostId = c.HostId,
            Resort = new ResortDTO
            {
                ResortId = c.Resort.ResortId,
                Name = c.Resort.Name
            },
            Name = c.Name,
            Description = c.Description,
            PricePerNight = c.PricePerNight,
            Beds = c.Beds,
            Bathrooms = c.Bathrooms,
            Status = c.Status,
            Images = c.CondotelImages?.Select(i => new ImageDTO
            {
                ImageId = i.ImageId,
                ImageUrl = i.ImageUrl,
                Caption = i.Caption
            }).ToList(),
            Prices = c.CondotelPrices?.Select(p => new PriceDTO
            {
                PriceId = p.PriceId,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                BasePrice = p.BasePrice,
                PriceType = p.PriceType
            }).ToList(),
            Details = c.CondotelDetails?.Select(d => new DetailDTO
            {
                BuildingName = d.BuildingName,
                RoomNumber = d.RoomNumber,
                Beds = d.Beds,
                Bathrooms = d.Bathrooms,
                SafetyFeatures = d.SafetyFeatures,
                HygieneStandards = d.HygieneStandards
            }).ToList(),
			Amenities = c.CondotelAmenities?.Select(ca => new AmenityDTO
			{
				AmenityId = ca.AmenityId,
				Name = ca.Amenity.Name
			}).ToList(),
			Utilities = c.CondotelUtilities?.Select(u => new UtilityDTO
            {
                UtilityId = u.UtilityId,
                Name = u.Utility.Name
            }).ToList(),
			Promotions = c.Promotions?.Select(p => new PromotionDTO
			{
				PromotionId = p.PromotionId,
				Name = p.Name,
				StartDate = p.StartDate,
				EndDate = p.EndDate,
				DiscountPercentage = p.DiscountPercentage
			}).ToList()
        };
    }

    public IEnumerable<CondotelDTO> GetCondotels()
    {
        return _condotelRepo.GetCondtels()
                .Select(c => new CondotelDTO
                {
                    CondotelId = c.CondotelId,
                    Name = c.Name,
                    PricePerNight = c.PricePerNight,
                    Beds = c.Beds,
                    Bathrooms = c.Bathrooms,
                    Status = c.Status,
                    ThumbnailUrl = c.CondotelImages?.FirstOrDefault()?.ImageUrl,
                    ResortName = c.Resort?.Name,
                    HostName = c.Host?.CompanyName
                });
    }

    public CondotelUpdateDTO UpdateCondotel(CondotelUpdateDTO dto)
    {
        var c = _condotelRepo.GetCondotelById(dto.CondotelId);
        if (c == null) return null;
        var condotel = new Condotel
        {
            CondotelId = dto.CondotelId,
            HostId = dto.HostId,
            ResortId = dto.ResortId,
            Name = dto.Name,
            Description = dto.Description,
            PricePerNight = dto.PricePerNight,
            Beds = dto.Beds,
            Bathrooms = dto.Bathrooms,
            Status = dto.Status,
            CondotelImages = dto.Images?.Select(i => new CondotelImage
            {
                CondotelId = dto.CondotelId,
                ImageUrl = i.ImageUrl,
                Caption = i.Caption
            }).ToList(),
            CondotelPrices = dto.Prices?.Select(p => new CondotelPrice
            {
                CondotelId = dto.CondotelId,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                BasePrice = p.BasePrice,
                PriceType = p.PriceType,
                Description = p.Description
            }).ToList(),
            CondotelDetails = dto.Details?.Select(d => new CondotelDetail
            {
                CondotelId = dto.CondotelId,
                BuildingName = d.BuildingName,
                RoomNumber = d.RoomNumber,
                Beds = d.Beds,
                Bathrooms = d.Bathrooms,
                SafetyFeatures = d.SafetyFeatures,
                HygieneStandards = d.HygieneStandards
            }).ToList(),
            CondotelAmenities = dto.AmenityIds?.Select(aid => new CondotelAmenity
            {
                CondotelId = dto.CondotelId,
                AmenityId = aid,
                DateAdded = DateOnly.FromDateTime(DateTime.Now)
            }).ToList(),
            CondotelUtilities = dto.UtilityIds?.Select(uid => new CondotelUtility
            {
                CondotelId = dto.CondotelId,
                UtilityId = uid,
                DateAdded = DateOnly.FromDateTime(DateTime.Now)
            }).ToList()
        };

        _condotelRepo.UpdateCondotel(condotel);
        _condotelRepo.SaveChanges();
        return dto;
    }
}
