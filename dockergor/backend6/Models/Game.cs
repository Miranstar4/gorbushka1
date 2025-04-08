using System;
using System.Collections.Generic;

namespace backend6.Models
{
    public partial class Game
    {
        public Guid Id { get; set; }
        public Guid? User { get; set; }
        public string? Belt { get; set; }
        public long? Score { get; set; }
        public long? Woods { get; set; }
        public bool? IsActive { get; set; }

        public virtual User? UserNavigation { get; set; }
    }
}
