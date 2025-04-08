using System;
using System.Collections.Generic;

namespace AdminPanelMarket.Models
{
    public partial class ReferalStartup
    {
        public Guid Id { get; set; }
        public string? Referalid { get; set; }
        public string? Userid { get; set; }
        public bool? IsBonus { get; set; }
    }
}
