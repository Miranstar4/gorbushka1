using System;
using System.Collections.Generic;

namespace backend6.Models
{
    public partial class ProductTop
    {
        public Guid Id { get; set; }
        public Guid? Product { get; set; }
        public string? Date { get; set; }
        public bool? IsActive { get; set; }

        public virtual Product? ProductNavigation { get; set; }
    }
}
