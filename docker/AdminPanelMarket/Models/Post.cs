using System;
using System.Collections.Generic;

namespace AdminPanelMarket.Models
{
    public partial class Post
    {
        public Guid Id { get; set; }
        public string? Text { get; set; }
        public DateTime? DateSend { get; set; }
        public Guid? Admin { get; set; }

        public virtual Admin? AdminNavigation { get; set; }
    }
}
