using System;
using System.Collections.Generic;

namespace TelegramBot.Models
{
    public partial class Admin
    {
        public Admin()
        {
            MessageTelegrams = new HashSet<MessageTelegram>();
            OrderMessages = new HashSet<OrderMessage>();
        }

        public Guid Id { get; set; }
        public string? Username { get; set; }
        public long? Telegramid { get; set; }
        public long? Code { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsManager { get; set; }

        public virtual ICollection<MessageTelegram> MessageTelegrams { get; set; }
        public virtual ICollection<OrderMessage> OrderMessages { get; set; }
    }
}
