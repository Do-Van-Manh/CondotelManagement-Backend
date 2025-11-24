using System;
using System.Collections.Generic;

namespace CondotelManagement.Models;

public partial class HostPackage
{
    public int HostId { get; set; }

    public int PackageId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Status { get; set; } = null!;
    public string OrderCode { get; set; } = string.Empty;

    public int? DurationDays { get; set; }

    public virtual Host Host { get; set; } = null!;

    public virtual Package Package { get; set; } = null!;
}
