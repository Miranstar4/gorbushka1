using System;
using System.Collections.Generic;

namespace AdminPanelMarket.Models
{
    public partial class User
    {
        public User()
        {
            Games = new HashSet<Game>();
            InverseReferalNavigation = new HashSet<User>();
            Orders = new HashSet<Order>();
            PromocodeUserActives = new HashSet<PromocodeUserActive>();
            Promocodes = new HashSet<Promocode>();
            ReferalStatistics = new HashSet<ReferalStatistic>();
            ScoreHistories = new HashSet<ScoreHistory>();
            StoriesVisits = new HashSet<StoriesVisit>();
        }

        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Telegramid { get; set; }
        public string? Phone { get; set; }
        public string? Fio { get; set; }
        public long? Score { get; set; }
        public string? Lastimg { get; set; }
        public string? Token { get; set; }
        public DateTime? DateRegister { get; set; }
        public Guid? Referal { get; set; }
        public string? ReferalTracker { get; set; }
        public bool? IsActive { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }

        public virtual User? ReferalNavigation { get; set; }
        public virtual ICollection<Game> Games { get; set; }
        public virtual ICollection<User> InverseReferalNavigation { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<PromocodeUserActive> PromocodeUserActives { get; set; }
        public virtual ICollection<Promocode> Promocodes { get; set; }
        public virtual ICollection<ReferalStatistic> ReferalStatistics { get; set; }
        public virtual ICollection<ScoreHistory> ScoreHistories { get; set; }
        public virtual ICollection<StoriesVisit> StoriesVisits { get; set; }
    }
}
