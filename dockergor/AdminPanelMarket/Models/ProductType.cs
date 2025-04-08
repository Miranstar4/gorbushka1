using System;
using System.Collections.Generic;

namespace AdminPanelMarket.Models
{
    public partial class ProductType
    {
        public ProductType()
        {
            Orders = new HashSet<Order>();
            ProductImages = new HashSet<ProductImage>();
        }

        public Guid Id { get; set; }
        public Guid? Product { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public long? Cost { get; set; }
        public long? Score { get; set; }
        public string? NameType { get; set; }
        public bool? IsActive { get; set; }
        public string? Color { get; set; }

        public virtual Product? ProductNavigation { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; }
    }
}
