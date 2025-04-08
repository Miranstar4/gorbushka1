using System;
using System.Collections.Generic;

namespace TelegramBot.Models
{
    public partial class ProductImage
    {
        public Guid Id { get; set; }
        public Guid? Product { get; set; }
        public Guid? ProductType { get; set; }
        public int? Order { get; set; }
        public string? Image { get; set; }

        public virtual Product? ProductNavigation { get; set; }
        public virtual ProductType? ProductTypeNavigation { get; set; }
    }
}
