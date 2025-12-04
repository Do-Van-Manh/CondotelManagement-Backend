using CondotelManagement.DTOs;
using CondotelManagement.Models;
using CondotelManagement.Repositories;
using CondotelManagement.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore.Storage;


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

        // Validate Utilities belong to the host
        if (!_condotelRepo.UtilitiesBelongToHost(dto.UtilityIds, dto.HostId))
            throw new InvalidOperationException("One or more Utilities do not belong to this host");

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

        // Add Condotel trước để có thể lấy CondotelId
        _condotelRepo.AddCondotel(condotel);
        
        // Save condotel để lấy CondotelId (EF Core sẽ tự động generate ID)
        if (!_condotelRepo.SaveChanges())
            throw new InvalidOperationException("Failed to save condotel to database");

        // Sau khi có CondotelId, tạo và gán các child entities
        var condotelId = condotel.CondotelId;

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
                PriceType = p.PriceType?.Trim() ?? "Normal",
                Description = p.Description?.Trim(),
                Status = "Active"
            }).ToList();
            _condotelRepo.AddCondotelPrices(prices);
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
                Status = "Active"
            }).ToList();
            _condotelRepo.AddCondotelDetails(details);
        }

        // Tạo Amenities
        if (dto.AmenityIds != null && dto.AmenityIds.Any())
        {
            var amenities = dto.AmenityIds.Select(aid => new CondotelAmenity
            {
                CondotelId = condotelId,
                AmenityId = aid,
                DateAdded = DateOnly.FromDateTime(DateTime.UtcNow),
                Status = "Active"
            }).ToList();
            _condotelRepo.AddCondotelAmenities(amenities);
        }

        // Tạo Utilities
        if (dto.UtilityIds != null && dto.UtilityIds.Any())
        {
            var utilities = dto.UtilityIds.Select(uid => new CondotelUtility
            {
                CondotelId = condotelId,
                UtilityId = uid,
                DateAdded = DateOnly.FromDateTime(DateTime.UtcNow),
                Status = "Active"
            }).ToList();
            _condotelRepo.AddCondotelUtilities(utilities);
        }

        // Save tất cả child entities trong một transaction
        // Nếu có bất kỳ child entity nào, save chúng
        var hasChildEntities = (dto.Images != null && dto.Images.Any(i => !string.IsNullOrWhiteSpace(i.ImageUrl))) ||
                               (dto.Prices != null && dto.Prices.Any()) ||
                               (dto.Details != null && dto.Details.Any()) ||
                               (dto.AmenityIds != null && dto.AmenityIds.Any()) ||
                               (dto.UtilityIds != null && dto.UtilityIds.Any());

        if (hasChildEntities)
        {
            if (!_condotelRepo.SaveChanges())
            {
                // Nếu save child entities thất bại, condotel đã được tạo nhưng không có child entities
                // Có thể xóa condotel đã tạo hoặc để admin xử lý
                throw new InvalidOperationException("Failed to save condotel child entities to database. Condotel may have been partially created.");
            }
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
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var condotels = _condotelRepo.GetCondtels().ToList();
        
        return condotels.Select(c => new CondotelDTO
        {
            CondotelId = c.CondotelId,
            Name = c.Name,
            PricePerNight = c.PricePerNight,
            Beds = c.Beds,
            Bathrooms = c.Bathrooms,
            Status = c.Status,
            ThumbnailUrl = c.CondotelImages?.FirstOrDefault()?.ImageUrl,
            ResortName = c.Resort?.Name,
            HostName = c.Host?.CompanyName,
            // Lấy promotion đang active (Status = "Active" và trong khoảng thời gian hiện tại)
            ActivePromotion = c.Promotions
                .Where(p => p.Status == "Active" 
                    && p.StartDate <= today 
                    && p.EndDate >= today)
                .OrderByDescending(p => p.DiscountPercentage) // Ưu tiên promotion có discount cao nhất
                .Select(p => new PromotionDTO
                {
                    PromotionId = p.PromotionId,
                    Name = p.Name,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    DiscountPercentage = p.DiscountPercentage,
                    TargetAudience = p.TargetAudience,
                    Status = p.Status,
                    CondotelId = p.CondotelId
                })
                .FirstOrDefault()
        });
    }

    public IEnumerable<CondotelDTO> GetCondtelsByHost(int hostId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var condotels = _condotelRepo.GetCondtelsByHost(hostId).ToList();
        
        return condotels.Select(c => new CondotelDTO
        {
            CondotelId = c.CondotelId,
            Name = c.Name,
            PricePerNight = c.PricePerNight,
            Beds = c.Beds,
            Bathrooms = c.Bathrooms,
            Status = c.Status,
            ThumbnailUrl = c.CondotelImages?.FirstOrDefault()?.ImageUrl,
            ResortName = c.Resort?.Name,
            HostName = c.Host?.CompanyName,
            // Lấy promotion đang active (Status = "Active" và trong khoảng thời gian hiện tại)
            ActivePromotion = c.Promotions
                .Where(p => p.Status == "Active" 
                    && p.StartDate <= today 
                    && p.EndDate >= today)
                .OrderByDescending(p => p.DiscountPercentage) // Ưu tiên promotion có discount cao nhất
                .Select(p => new PromotionDTO
                {
                    PromotionId = p.PromotionId,
                    Name = p.Name,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    DiscountPercentage = p.DiscountPercentage,
                    TargetAudience = p.TargetAudience,
                    Status = p.Status,
                    CondotelId = p.CondotelId
                })
                .FirstOrDefault()
        });
    }

	public IEnumerable<CondotelDTO> GetCondotelsByFilters(
			string? name,
			string? location,
			int? locationId,
			DateOnly? fromDate,
			DateOnly? toDate,
			decimal? minPrice,
			decimal? maxPrice,
			int? beds,
			int? bathrooms)
	{
		var today = DateOnly.FromDateTime(DateTime.UtcNow);
		var condotels = _condotelRepo.GetCondotelsByFilters(name, location, locationId, fromDate, toDate, minPrice, maxPrice, beds, bathrooms).ToList();
		
		return condotels.Select(c => new CondotelDTO
		{
			CondotelId = c.CondotelId,
			Name = c.Name,
			PricePerNight = c.PricePerNight,
			Beds = c.Beds,
			Bathrooms = c.Bathrooms,
			Status = c.Status,
			ThumbnailUrl = c.CondotelImages?.FirstOrDefault()?.ImageUrl,
			ResortName = c.Resort?.Name,
			HostName = c.Host?.CompanyName,
			// Lấy promotion đang active (Status = "Active" và trong khoảng thời gian hiện tại)
			ActivePromotion = c.Promotions
				.Where(p => p.Status == "Active" 
					&& p.StartDate <= today 
					&& p.EndDate >= today)
				.OrderByDescending(p => p.DiscountPercentage) // Ưu tiên promotion có discount cao nhất
				.Select(p => new PromotionDTO
				{
					PromotionId = p.PromotionId,
					Name = p.Name,
					StartDate = p.StartDate,
					EndDate = p.EndDate,
					DiscountPercentage = p.DiscountPercentage,
					TargetAudience = p.TargetAudience,
					Status = p.Status,
					CondotelId = p.CondotelId
				})
				.FirstOrDefault()
		});
	}

    public CondotelUpdateDTO UpdateCondotel(CondotelUpdateDTO dto)
    {
        // Validation
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Condotel data cannot be null");

        if (dto.CondotelId <= 0)
            throw new ArgumentException("Condotel ID is required", nameof(dto.CondotelId));

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Condotel name is required", nameof(dto.Name));

        if (dto.PricePerNight <= 0)
            throw new ArgumentException("Price per night must be greater than 0", nameof(dto.PricePerNight));

        if (dto.Beds <= 0)
            throw new ArgumentException("Number of beds must be greater than 0", nameof(dto.Beds));

        if (dto.Bathrooms <= 0)
            throw new ArgumentException("Number of bathrooms must be greater than 0", nameof(dto.Bathrooms));

        // Kiểm tra condotel có tồn tại không
        var existing = _condotelRepo.GetCondotelById(dto.CondotelId);
        if (existing == null)
            throw new InvalidOperationException($"Condotel with ID {dto.CondotelId} does not exist");

        // Validate Host ownership - đảm bảo HostId không thay đổi
        if (existing.HostId != dto.HostId)
            throw new UnauthorizedAccessException("Cannot change condotel ownership");

        // Validate Resort exists (if provided)
        if (!_condotelRepo.ResortExists(dto.ResortId))
            throw new InvalidOperationException($"Resort with ID {dto.ResortId} does not exist");

        // Validate Amenities exist
        if (dto.AmenityIds != null && dto.AmenityIds.Any())
        {
            if (!_condotelRepo.AmenitiesExist(dto.AmenityIds))
                throw new InvalidOperationException("One or more Amenity IDs are invalid");
        }

        // Validate Utilities exist
        if (dto.UtilityIds != null && dto.UtilityIds.Any())
        {
            if (!_condotelRepo.UtilitiesExist(dto.UtilityIds))
                throw new InvalidOperationException("One or more Utility IDs are invalid");
        }

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

        // Tạo entity để update
        var condotel = new Condotel
        {
            CondotelId = dto.CondotelId,
            HostId = dto.HostId, // Giữ nguyên HostId
            ResortId = dto.ResortId,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            PricePerNight = dto.PricePerNight,
            Beds = dto.Beds,
            Bathrooms = dto.Bathrooms,
            Status = dto.Status,
            CondotelImages = dto.Images?.Where(i => !string.IsNullOrWhiteSpace(i.ImageUrl))
                .Select(i => new CondotelImage
                {
                    CondotelId = dto.CondotelId,
                    ImageUrl = i.ImageUrl.Trim(),
                    Caption = i.Caption?.Trim()
                }).ToList(),
            CondotelPrices = dto.Prices?.Select(p => new CondotelPrice
            {
                CondotelId = dto.CondotelId,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                BasePrice = p.BasePrice,
                PriceType = p.PriceType?.Trim() ?? "Normal",
                Description = p.Description?.Trim(),
                Status = "Active"
            }).ToList(),
            CondotelDetails = dto.Details?.Select(d => new CondotelDetail
            {
                CondotelId = dto.CondotelId,
                BuildingName = d.BuildingName?.Trim(),
                RoomNumber = d.RoomNumber?.Trim(),
                Beds = d.Beds,
                Bathrooms = d.Bathrooms,
                SafetyFeatures = d.SafetyFeatures?.Trim(),
                HygieneStandards = d.HygieneStandards?.Trim(),
                Status = "Active"
            }).ToList(),
            CondotelAmenities = dto.AmenityIds?.Select(aid => new CondotelAmenity
            {
                CondotelId = dto.CondotelId,
                AmenityId = aid,
                DateAdded = DateOnly.FromDateTime(DateTime.UtcNow),
                Status = "Active"
            }).ToList(),
            CondotelUtilities = dto.UtilityIds?.Select(uid => new CondotelUtility
            {
                CondotelId = dto.CondotelId,
                UtilityId = uid,
                DateAdded = DateOnly.FromDateTime(DateTime.UtcNow),
                Status = "Active"
            }).ToList()
        };

        // Update condotel
        _condotelRepo.UpdateCondotel(condotel);
        
        if (!_condotelRepo.SaveChanges())
            throw new InvalidOperationException("Failed to update condotel in database");

        // Reload từ database để lấy đầy đủ thông tin
        var updatedCondotel = _condotelRepo.GetCondotelById(dto.CondotelId);
        if (updatedCondotel == null)
            throw new InvalidOperationException("Failed to retrieve updated condotel from database");

        // Map lại sang DTO
        return new CondotelUpdateDTO
        {
            CondotelId = updatedCondotel.CondotelId,
            HostId = updatedCondotel.HostId,
            ResortId = updatedCondotel.ResortId,
            Name = updatedCondotel.Name,
            Description = updatedCondotel.Description,
            PricePerNight = updatedCondotel.PricePerNight,
            Beds = updatedCondotel.Beds,
            Bathrooms = updatedCondotel.Bathrooms,
            Status = updatedCondotel.Status,
            Images = updatedCondotel.CondotelImages?.Select(i => new ImageDTO
            {
                ImageId = i.ImageId,
                ImageUrl = i.ImageUrl,
                Caption = i.Caption
            }).ToList(),
            Prices = updatedCondotel.CondotelPrices?.Select(p => new PriceDTO
            {
                PriceId = p.PriceId,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                BasePrice = p.BasePrice,
                PriceType = p.PriceType,
                Description = p.Description
            }).ToList(),
            Details = updatedCondotel.CondotelDetails?.Select(d => new DetailDTO
            {
                BuildingName = d.BuildingName,
                RoomNumber = d.RoomNumber,
                Beds = d.Beds,
                Bathrooms = d.Bathrooms,
                SafetyFeatures = d.SafetyFeatures,
                HygieneStandards = d.HygieneStandards
            }).ToList(),
            AmenityIds = updatedCondotel.CondotelAmenities?.Select(a => a.AmenityId).ToList(),
            UtilityIds = updatedCondotel.CondotelUtilities?.Select(u => u.UtilityId).ToList()
        };
    }
}
