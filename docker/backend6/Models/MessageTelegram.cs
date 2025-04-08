using System;
using System.Collections.Generic;

namespace backend6.Models
{
    public partial class MessageTelegram
    {
        public Guid Id { get; set; }
        public Guid? Admin { get; set; }
        public Guid? Order { get; set; }
        public long? Messageid { get; set; }
        public bool? IsActive { get; set; }

        public virtual Admin? AdminNavigation { get; set; }
        public virtual Order? OrderNavigation { get; set; }
    }
}
