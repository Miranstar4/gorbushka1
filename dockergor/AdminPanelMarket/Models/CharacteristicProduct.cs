using System;
using System.Collections.Generic;

namespace AdminPanelMarket.Models
{
    public partial class CharacteristicProduct
    {
        public Guid Id { get; set; }
        public Guid? Characteristic { get; set; }
        public Guid? Product { get; set; }
        public string? Description { get; set; }

        public virtual Characteristic? CharacteristicNavigation { get; set; }
        public virtual Product? ProductNavigation { get; set; }
    }
}
