using System;
using System.Collections.Generic;

namespace TelegramBot.Models
{
    public partial class OrderMessage
    {
        public Guid Id { get; set; }
        public Guid? Order { get; set; }
        public Guid? Admin { get; set; }
        public bool? IsWrittenAdmin { get; set; }
        public DateTime? Date { get; set; }
        public string? Message { get; set; }
        public long Minteger { get; set; }

        public virtual Admin? AdminNavigation { get; set; }
        public virtual Order? OrderNavigation { get; set; }
    }
}
