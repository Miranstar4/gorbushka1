using System;
using System.Collections.Generic;

namespace AdminPanelMarket.Models
{
    public partial class UserDialog
    {
        public Guid Id { get; set; }
        public Guid? Order { get; set; }
        public bool? IsActive { get; set; }

        public virtual Order? OrderNavigation { get; set; }
    }
}
