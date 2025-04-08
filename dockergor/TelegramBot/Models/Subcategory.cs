using System;
using System.Collections.Generic;

namespace TelegramBot.Models
{
    public partial class Subcategory
    {
        public Subcategory()
        {
            Characteristics = new HashSet<Characteristic>();
            Products = new HashSet<Product>();
        }

        public Guid Id { get; set; }
        public Guid? Category { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public bool? IsActive { get; set; }

        public virtual Category? CategoryNavigation { get; set; }
        public virtual ICollection<Characteristic> Characteristics { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
