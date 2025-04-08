using System;
using System.Collections.Generic;

namespace AdminPanelMarket.Models
{
    public partial class StoriesVisit
    {
        public Guid Id { get; set; }
        public Guid? User { get; set; }
        public Guid? Stories { get; set; }
        public DateTime? Date { get; set; }
        public bool? IsActive { get; set; }

        public virtual Story? StoriesNavigation { get; set; }
        public virtual User? UserNavigation { get; set; }
    }
}
