using System;
using System.Collections.Generic;

namespace CondotelManagement.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int CondotelId { get; set; }

    public int CustomerId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal? TotalPrice { get; set; }

    public string Status { get; set; } = null!;

    public int? PromotionId { get; set; }

    public bool IsUsingRewardPoints { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? VoucherId { get; set; }

    public bool IsPaidToHost { get; set; }

    public DateTime? PaidToHostAt { get; set; }

    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();

    public virtual Condotel Condotel { get; set; } = null!;

    public virtual User Customer { get; set; } = null!;

    public virtual Promotion? Promotion { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Voucher? Voucher { get; set; }
}
