using CondotelManagement.DTOs;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Promotion
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        // GET /api/promotion
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromotionDTO>>> GetAll()
        {
            var promotions = await _promotionService.GetAllAsync();
            return Ok(promotions);
        }

        // GET /api/promotion/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PromotionDTO>> GetById(int id)
        {
            var promotion = await _promotionService.GetByIdAsync(id);
            if (promotion == null)
                return NotFound(new { message = "Promotion not found" });

            return Ok(promotion);
        }

        // GET /api/promotion/condotel/{condotelId}
        [HttpGet("condotel/{condotelId}")]
        public async Task<ActionResult<IEnumerable<PromotionDTO>>> GetByCondotelId(int condotelId)
        {
            var promotions = await _promotionService.GetByCondotelIdAsync(condotelId);
            return Ok(promotions);
        }

        // POST /api/promotion
        [HttpPost]
        [Authorize(Roles = "Host,Admin")]
        public async Task<ActionResult<PromotionDTO>> Create([FromBody] PromotionCreateUpdateDTO dto)
        {
            if (dto == null) return BadRequest(new { message = "Invalid promotion data" });
            
            var created = await _promotionService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), "Promotion", new { id = created.PromotionId }, created);
        }

        // PUT /api/promotion/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Host,Admin")]
        public async Task<ActionResult> Update(int id, [FromBody] PromotionCreateUpdateDTO dto)
        {
            if (dto == null) return BadRequest(new { message = "Invalid promotion data" });
            
            var success = await _promotionService.UpdateAsync(id, dto);
            if (!success) return NotFound(new { message = "Promotion not found" });
            
            return Ok(new { message = "Promotion updated successfully" });
        }

        // DELETE /api/promotion/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Host,Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var success = await _promotionService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Promotion not found" });
            
            return Ok(new { message = "Promotion deleted successfully" });
        }
    }
}

