using System;
using System.Collections.Generic;

namespace backend6.Models
{
    public partial class ReferalStartup
    {
        public Guid Id { get; set; }
        public string? Referalid { get; set; }
        public string? Userid { get; set; }
        public bool? IsBonus { get; set; }
    }
}
