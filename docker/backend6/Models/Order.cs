using System;
using System.Collections.Generic;

namespace backend6.Models
{
    public partial class Order
    {
        public Order()
        {
            ManagerDialogs = new HashSet<ManagerDialog>();
            MessageTelegrams = new HashSet<MessageTelegram>();
            OrderMessages = new HashSet<OrderMessage>();
            UserDialogs = new HashSet<UserDialog>();
        }

        public Guid Id { get; set; }
        public Guid? User { get; set; }
        public Guid? Product { get; set; }
        public Guid? ProductType { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public bool? IsFinish { get; set; }
        public string? Status { get; set; }
        public long? Score { get; set; }
        public bool? IsScoreAdd { get; set; }
        public string? Phone { get; set; }
        public string? Fio { get; set; }
        public string? CardData { get; set; }
        public string? TrackNumber { get; set; }
        public string? NameTrack { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public Guid? Promocode { get; set; }

        public virtual Product? ProductNavigation { get; set; }
        public virtual ProductType? ProductTypeNavigation { get; set; }
        public virtual Promocode? PromocodeNavigation { get; set; }
        public virtual User? UserNavigation { get; set; }
        public virtual ICollection<ManagerDialog> ManagerDialogs { get; set; }
        public virtual ICollection<MessageTelegram> MessageTelegrams { get; set; }
        public virtual ICollection<OrderMessage> OrderMessages { get; set; }
        public virtual ICollection<UserDialog> UserDialogs { get; set; }
    }
}
