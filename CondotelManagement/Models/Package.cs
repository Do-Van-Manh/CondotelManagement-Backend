using System;
using System.Collections.Generic;

namespace CondotelManagement.Models;

public partial class Package
{
    public int PackageId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Duration { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<HostPackage> HostPackages { get; set; } = new List<HostPackage>();
}
