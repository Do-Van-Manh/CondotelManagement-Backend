using System;
using System.Collections.Generic;

namespace CondotelManagement.Models;

public partial class BlogCategory
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}
