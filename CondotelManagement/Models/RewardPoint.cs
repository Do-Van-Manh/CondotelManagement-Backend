using System;
using System.Collections.Generic;

namespace CondotelManagement.Models;

public partial class RewardPoint
{
    public int PointId { get; set; }

    public int CustomerId { get; set; }

    public int Points { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual User Customer { get; set; } = null!;
}
