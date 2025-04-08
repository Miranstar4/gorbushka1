using System;
using System.Collections.Generic;

namespace backend6.Models
{
    public partial class PromocodeUserActive
    {
        public Guid Id { get; set; }
        public Guid? User { get; set; }
        public Guid? Promocode { get; set; }
        public DateTime? Data { get; set; }

        public virtual Promocode? PromocodeNavigation { get; set; }
        public virtual User? UserNavigation { get; set; }
    }
}
