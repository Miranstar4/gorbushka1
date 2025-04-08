using System;
using System.Collections.Generic;

namespace TelegramBot.Models
{
    public partial class GameUser
    {
        public Guid Id { get; set; }
        public long? Telegramid { get; set; }
        public string? Username { get; set; }
        public string? Login { get; set; }
        public string? Token { get; set; }
        public long? Score { get; set; }
        public string? Belt { get; set; }
        public long? Woods { get; set; }
    }
}
