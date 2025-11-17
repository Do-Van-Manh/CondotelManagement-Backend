using CondotelManagement.DTOs;
using CondotelManagement.Models;
using CondotelManagement.Repositories;
using Microsoft.Extensions.Hosting;


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
        // Validation
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Condotel data cannot be null");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Condotel name is required", nameof(dto.Name));

        if (dto.PricePerNight <= 0)
            throw new ArgumentException("Price per night must be greater than 0", nameof(dto.PricePerNight));

        if (dto.Beds <= 0)
            throw new ArgumentException("Number of beds must be greater than 0", nameof(dto.Beds));

        if (dto.Bathrooms <= 0)
            throw new ArgumentException("Number of bathrooms must be greater than 0", nameof(dto.Bathrooms));

        // Validate Host exists
        if (!_condotelRepo.HostExists(dto.HostId))
            throw new InvalidOperationException($"Host with ID {dto.HostId} does not exist");

        // Validate Resort exists (if provided)
        if (!_condotelRepo.ResortExists(dto.ResortId))
            throw new InvalidOperationException($"Resort with ID {dto.ResortId} does not exist");

        // Validate Amenities exist
        if (!_condotelRepo.AmenitiesExist(dto.AmenityIds))
            throw new InvalidOperationException("One or more Amenity IDs are invalid");

        // Validate Utilities exist
        if (!_condotelRepo.UtilitiesExist(dto.UtilityIds))
            throw new InvalidOperationException("One or more Utility IDs are invalid");

        // Validate Price date ranges
        if (dto.Prices != null && dto.Prices.Any())
        {
            foreach (var price in dto.Prices)
            {
                if (price.StartDate >= price.EndDate)
                    throw new ArgumentException($"Price date range is invalid: StartDate must be before EndDate");

                if (price.BasePrice <= 0)
                    throw new ArgumentException("Base price must be greater than 0");
            }
        }

        // Set default Status if not provided
        var status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status;

        // Tạo Condotel chính trước
        var condotel = new Condotel
        {
            HostId = dto.HostId,
            ResortId = dto.ResortId,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            PricePerNight = dto.PricePerNight,
            Beds = dto.Beds,
            Bathrooms = dto.Bathrooms,
            Status = status
        };

        // Add Condotel trước để lấy CondotelId
        _condotelRepo.AddCondotel(condotel);
        
        if (!_condotelRepo.SaveChanges())
            throw new InvalidOperationException("Failed to save condotel to database");

        // Sau khi có CondotelId, tạo các child entities
        var condotelId = condotel.CondotelId;
        var childEntitiesToSave = false;

        // Tạo Images
        if (dto.Images != null && dto.Images.Any(i => !string.IsNullOrWhiteSpace(i.ImageUrl)))
        {
            var images = dto.Images
                .Where(i => !string.IsNullOrWhiteSpace(i.ImageUrl))
                .Select(i => new CondotelImage
                {
                    CondotelId = condotelId,
                    ImageUrl = i.ImageUrl.Trim(),
                    Caption = i.Caption?.Trim()
                }).ToList();
            _condotelRepo.AddCondotelImages(images);
            condotel.CondotelImages = images;
            childEntitiesToSave = true;
        }

        // Tạo Prices
        if (dto.Prices != null && dto.Prices.Any())
        {
            var prices = dto.Prices.Select(p => new CondotelPrice
            {
                CondotelId = condotelId,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                BasePrice = p.BasePrice,
                PriceType = p.PriceType?.Trim() ?? "Normal", // Khớp với database default
                Description = p.Description?.Trim(),
                Status = "Active" // Set Status rõ ràng
            }).ToList();
            _condotelRepo.AddCondotelPrices(prices);
            condotel.CondotelPrices = prices;
            childEntitiesToSave = true;
        }

        // Tạo Details
        if (dto.Details != null && dto.Details.Any())
        {
            var details = dto.Details.Select(d => new CondotelDetail
            {
                CondotelId = condotelId,
                BuildingName = d.BuildingName?.Trim(),
                RoomNumber = d.RoomNumber?.Trim(),
                Beds = d.Beds,
                Bathrooms = d.Bathrooms,
                SafetyFeatures = d.SafetyFeatures?.Trim(),
                HygieneStandards = d.HygieneStandards?.Trim(),
                Status = "Active" // Set Status rõ ràng
            }).ToList();
            _condotelRepo.AddCondotelDetails(details);
            condotel.CondotelDetails = details;
            childEntitiesToSave = true;
        }

        // Tạo Amenities
        if (dto.AmenityIds != null && dto.AmenityIds.Any())
        {
            var amenities = dto.AmenityIds.Select(aid => new CondotelAmenity
            {
                CondotelId = condotelId,
                AmenityId = aid,
                DateAdded = DateOnly.FromDateTime(DateTime.UtcNow),
                Status = "Active" // Set Status rõ ràng
            }).ToList();
            _condotelRepo.AddCondotelAmenities(amenities);
            condotel.CondotelAmenities = amenities;
            childEntitiesToSave = true;
        }

        // Tạo Utilities
        if (dto.UtilityIds != null && dto.UtilityIds.Any())
        {
            var utilities = dto.UtilityIds.Select(uid => new CondotelUtility
            {
                CondotelId = condotelId,
                UtilityId = uid,
                DateAdded = DateOnly.FromDateTime(DateTime.UtcNow),
                Status = "Active" // Set Status rõ ràng
            }).ToList();
            _condotelRepo.AddCondotelUtilities(utilities);
            condotel.CondotelUtilities = utilities;
            childEntitiesToSave = true;
        }

        // Save các child entities nếu có
        if (childEntitiesToSave)
        {
            if (!_condotelRepo.SaveChanges())
                throw new InvalidOperationException("Failed to save condotel child entities to database");
        }

        // Reload Condotel từ database để lấy đầy đủ thông tin (bao gồm IDs của child entities)
        var savedCondotel = _condotelRepo.GetCondotelById(condotel.CondotelId);
        if (savedCondotel == null)
            throw new InvalidOperationException("Failed to retrieve saved condotel from database");

        return new CondotelUpdateDTO
        {
            CondotelId = savedCondotel.CondotelId,
            HostId = savedCondotel.HostId,
            ResortId = savedCondotel.ResortId,
            Name = savedCondotel.Name,
            Description = savedCondotel.Description,
            PricePerNight = savedCondotel.PricePerNight,
            Beds = savedCondotel.Beds,
            Bathrooms = savedCondotel.Bathrooms,
            Status = savedCondotel.Status,

            Images = savedCondotel.CondotelImages?.Select(i => new ImageDTO
            {
                ImageId = i.ImageId,
                ImageUrl = i.ImageUrl,
                Caption = i.Caption
            }).ToList(),

            Prices = savedCondotel.CondotelPrices?.Select(p => new PriceDTO
            {
                PriceId = p.PriceId,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                BasePrice = p.BasePrice,
                PriceType = p.PriceType,
                Description = p.Description
            }).ToList(),

            Details = savedCondotel.CondotelDetails?.Select(d => new DetailDTO
            {
                BuildingName = d.BuildingName,
                RoomNumber = d.RoomNumber,
                Beds = d.Beds,
                Bathrooms = d.Bathrooms,
                SafetyFeatures = d.SafetyFeatures,
                HygieneStandards = d.HygieneStandards
            }).ToList(),

            AmenityIds = savedCondotel.CondotelAmenities?.Select(a => a.AmenityId).ToList(),
            UtilityIds = savedCondotel.CondotelUtilities?.Select(u => u.UtilityId).ToList()
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

    public IEnumerable<CondotelDTO> GetCondtelsByHost(int hostId)
    {
        return _condotelRepo.GetCondtelsByHost(hostId)
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

	public IEnumerable<CondotelDTO> GetCondotelsByNameLocationAndDate(string? name, string? location, DateOnly? fromDate, DateOnly? toDate)
	{
		return _condotelRepo.GetCondotelsByNameLocationAndDate(name, location, fromDate, toDate)
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
