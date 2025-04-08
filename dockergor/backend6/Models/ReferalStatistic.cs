using System;
using System.Collections.Generic;

namespace backend6.Models
{
    public partial class ReferalStatistic
    {
        public Guid Id { get; set; }
        public Guid? User { get; set; }
        public long? ClickOnLink { get; set; }
        public long? MadeOrder { get; set; }
        public long? Paid { get; set; }
        public long? ShippedOrders { get; set; }
        public long? AllScore { get; set; }
        public bool? IsActive { get; set; }

        public virtual User? UserNavigation { get; set; }
    }
}
