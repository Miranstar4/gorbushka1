using System;
using System.Collections.Generic;

namespace TelegramBot.Models
{
    public partial class ManagerDialog
    {
        public Guid Id { get; set; }
        public Guid? Order { get; set; }
        public long? IdTelegramManager { get; set; }

        public virtual Order? OrderNavigation { get; set; }
    }
}
