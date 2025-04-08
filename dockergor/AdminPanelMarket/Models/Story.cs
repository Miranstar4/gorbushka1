using System;
using System.Collections.Generic;

namespace AdminPanelMarket.Models
{
    public partial class Story
    {
        public Story()
        {
            StoriesVisits = new HashSet<StoriesVisit>();
        }

        public Guid Id { get; set; }
        public string? Text { get; set; }
        public string? Color { get; set; }
        public string? Url { get; set; }
        public DateTime? Date { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<StoriesVisit> StoriesVisits { get; set; }
    }
}
