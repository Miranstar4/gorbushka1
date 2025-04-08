using System;
using System.Collections.Generic;

namespace TelegramBot.Models
{
    public partial class Product
    {
        public Product()
        {
            CharacteristicProducts = new HashSet<CharacteristicProduct>();
            Orders = new HashSet<Order>();
            ProductImages = new HashSet<ProductImage>();
            ProductTypes = new HashSet<ProductType>();
        }

        public Guid Id { get; set; }
        public Guid? Subcategory { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public long? Cost { get; set; }
        public long? Score { get; set; }
        public string? DefaultColor { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsSell { get; set; }

        public virtual Subcategory? SubcategoryNavigation { get; set; }
        public virtual ICollection<CharacteristicProduct> CharacteristicProducts { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; }
        public virtual ICollection<ProductType> ProductTypes { get; set; }
    }
}
