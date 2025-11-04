using CondotelManagement.DTOs;
using CondotelManagement.Models;
using CondotelManagement.Repositories;

namespace CondotelManagement.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepo;

        public PromotionService(IPromotionRepository promotionRepo)
        {
            _promotionRepo = promotionRepo;
        }

        public async Task<IEnumerable<PromotionDTO>> GetAllAsync()
        {
            var promotions = await _promotionRepo.GetAllAsync();
            return promotions.Select(p => new PromotionDTO
            {
                PromotionId = p.PromotionId,
                Name = p.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                DiscountPercentage = p.DiscountPercentage,
                TargetAudience = p.TargetAudience,
                Status = p.Status,
                CondotelId = p.CondotelId,
                CondotelName = p.Condotel?.Name
            });
        }

        public async Task<PromotionDTO?> GetByIdAsync(int id)
        {
            var p = await _promotionRepo.GetByIdAsync(id);
            if (p == null) return null;

            return new PromotionDTO
            {
                PromotionId = p.PromotionId,
                Name = p.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                DiscountPercentage = p.DiscountPercentage,
                TargetAudience = p.TargetAudience,
                Status = p.Status,
                CondotelId = p.CondotelId,
                CondotelName = p.Condotel?.Name
            };
        }

        public async Task<IEnumerable<PromotionDTO>> GetByCondotelIdAsync(int condotelId)
        {
            var promotions = await _promotionRepo.GetByCondotelIdAsync(condotelId);
            return promotions.Select(p => new PromotionDTO
            {
                PromotionId = p.PromotionId,
                Name = p.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                DiscountPercentage = p.DiscountPercentage,
                TargetAudience = p.TargetAudience,
                Status = p.Status,
                CondotelId = p.CondotelId,
                CondotelName = p.Condotel?.Name
            });
        }

        public async Task<PromotionDTO> CreateAsync(PromotionCreateUpdateDTO dto)
        {
            var promotion = new Promotion
            {
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                DiscountPercentage = dto.DiscountPercentage,
                TargetAudience = dto.TargetAudience,
                Status = dto.Status,
                CondotelId = dto.CondotelId
            };

            await _promotionRepo.AddAsync(promotion);
            
            // Reload để lấy Condotel navigation property
            var created = await _promotionRepo.GetByIdAsync(promotion.PromotionId);
            if (created == null)
                throw new InvalidOperationException("Failed to retrieve created promotion");

            return new PromotionDTO
            {
                PromotionId = created.PromotionId,
                Name = created.Name,
                StartDate = created.StartDate,
                EndDate = created.EndDate,
                DiscountPercentage = created.DiscountPercentage,
                TargetAudience = created.TargetAudience,
                Status = created.Status,
                CondotelId = created.CondotelId,
                CondotelName = created.Condotel?.Name
            };
        }

        public async Task<bool> UpdateAsync(int id, PromotionCreateUpdateDTO dto)
        {
            var promotion = await _promotionRepo.GetByIdAsync(id);
            if (promotion == null) return false;

            promotion.Name = dto.Name;
            promotion.StartDate = dto.StartDate;
            promotion.EndDate = dto.EndDate;
            promotion.DiscountPercentage = dto.DiscountPercentage;
            promotion.TargetAudience = dto.TargetAudience;
            promotion.Status = dto.Status;
            promotion.CondotelId = dto.CondotelId;

            await _promotionRepo.UpdateAsync(promotion);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var promotion = await _promotionRepo.GetByIdAsync(id);
            if (promotion == null) return false;

            await _promotionRepo.DeleteAsync(promotion);
            return true;
        }
    }
}

