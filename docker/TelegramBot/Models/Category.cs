using System;
using System.Collections.Generic;

namespace TelegramBot.Models
{
    public partial class Category
    {
        public Category()
        {
            Subcategories = new HashSet<Subcategory>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<Subcategory> Subcategories { get; set; }
    }
}
