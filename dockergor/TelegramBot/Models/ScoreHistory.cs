using System;
using System.Collections.Generic;

namespace TelegramBot.Models
{
    public partial class ScoreHistory
    {
        public Guid Id { get; set; }
        public Guid? User { get; set; }
        public string? Type { get; set; }
        public bool? GivenOrWrittenOff { get; set; }
        public long? Score { get; set; }
        public DateTime? Date { get; set; }
        public bool? IsActive { get; set; }

        public virtual User? UserNavigation { get; set; }
    }
}
