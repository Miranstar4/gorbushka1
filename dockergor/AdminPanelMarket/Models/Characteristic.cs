using System;
using System.Collections.Generic;

namespace AdminPanelMarket.Models
{
    public partial class Characteristic
    {
        public Characteristic()
        {
            CharacteristicProducts = new HashSet<CharacteristicProduct>();
        }

        public Guid Id { get; set; }
        public Guid? Subcategory { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public int? Order { get; set; }

        public virtual Subcategory? SubcategoryNavigation { get; set; }
        public virtual ICollection<CharacteristicProduct> CharacteristicProducts { get; set; }
    }
}
