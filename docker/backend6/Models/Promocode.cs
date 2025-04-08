using System;
using System.Collections.Generic;

namespace backend6.Models
{
    public partial class Promocode
    {
        public Promocode()
        {
            Orders = new HashSet<Order>();
            PromocodeSubcategories = new HashSet<PromocodeSubcategory>();
            PromocodeUserActives = new HashSet<PromocodeUserActive>();
        }

        public Guid Id { get; set; }
        public string? Type { get; set; }
        public string? Code { get; set; }
        public int? BonusProcent { get; set; }
        public long? BonusMoney { get; set; }
        public long? PriceСondition { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid? UserActivePromocode { get; set; }
        public bool? IsActive { get; set; }

        public virtual User? UserActivePromocodeNavigation { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<PromocodeSubcategory> PromocodeSubcategories { get; set; }
        public virtual ICollection<PromocodeUserActive> PromocodeUserActives { get; set; }
    }
}
