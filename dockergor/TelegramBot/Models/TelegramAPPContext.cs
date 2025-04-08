using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TelegramBot.Models
{
    public partial class TelegramAPPContext : DbContext
    {
        public TelegramAPPContext()
        {
        }

        public TelegramAPPContext(DbContextOptions<TelegramAPPContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Admin> Admins { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Characteristic> Characteristics { get; set; } = null!;
        public virtual DbSet<CharacteristicProduct> CharacteristicProducts { get; set; } = null!;
        public virtual DbSet<ErrorLog> ErrorLogs { get; set; } = null!;
        public virtual DbSet<Game> Games { get; set; } = null!;
        public virtual DbSet<GameUser> GameUsers { get; set; } = null!;
        public virtual DbSet<ManagerDialog> ManagerDialogs { get; set; } = null!;
        public virtual DbSet<MessageTelegram> MessageTelegrams { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderMessage> OrderMessages { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<ProductImage> ProductImages { get; set; } = null!;
        public virtual DbSet<ProductType> ProductTypes { get; set; } = null!;
        public virtual DbSet<ReferalStartup> ReferalStartups { get; set; } = null!;
        public virtual DbSet<ReferalStatistic> ReferalStatistics { get; set; } = null!;
        public virtual DbSet<ScoreHistory> ScoreHistories { get; set; } = null!;
        public virtual DbSet<StoriesVisit> StoriesVisits { get; set; } = null!;
        public virtual DbSet<Story> Stories { get; set; } = null!;
        public virtual DbSet<Subcategory> Subcategories { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserDialog> UserDialogs { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("data source=HYPECODER;initial catalog=TelegramAPP;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("Admin");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Code).HasColumnName("code");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.IsManager)
                    .IsRequired()
                    .HasColumnName("isManager")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Telegramid).HasColumnName("telegramid");

                entity.Property(e => e.Username).HasColumnName("username");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Image).HasColumnName("image");

                entity.Property(e => e.IsActive)
                    .HasColumnName("isActive")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<Characteristic>(entity =>
            {
                entity.ToTable("Characteristic");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.IsActive)
                    .HasColumnName("isActive")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.Order).HasColumnName("order");

                entity.Property(e => e.Subcategory).HasColumnName("subcategory");

                entity.HasOne(d => d.SubcategoryNavigation)
                    .WithMany(p => p.Characteristics)
                    .HasForeignKey(d => d.Subcategory)
                    .HasConstraintName("FK_Characteristic_To_Subcategory");
            });

            modelBuilder.Entity<CharacteristicProduct>(entity =>
            {
                entity.ToTable("CharacteristicProduct");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Characteristic).HasColumnName("characteristic");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.Product).HasColumnName("product");

                entity.HasOne(d => d.CharacteristicNavigation)
                    .WithMany(p => p.CharacteristicProducts)
                    .HasForeignKey(d => d.Characteristic)
                    .HasConstraintName("FK_CharacteristicProduct_To_Characteristic");

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany(p => p.CharacteristicProducts)
                    .HasForeignKey(d => d.Product)
                    .HasConstraintName("FK_CharacteristicProduct_To_Product");
            });

            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");
            });

            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("Game");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Belt).HasColumnName("belt");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.Score).HasColumnName("score");

                entity.Property(e => e.User).HasColumnName("user");

                entity.Property(e => e.Woods).HasColumnName("woods");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.Games)
                    .HasForeignKey(d => d.User)
                    .HasConstraintName("FK_Game_To_User");
            });

            modelBuilder.Entity<GameUser>(entity =>
            {
                entity.ToTable("GameUser");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Belt).HasColumnName("belt");

                entity.Property(e => e.Login).HasColumnName("login");

                entity.Property(e => e.Score).HasColumnName("score");

                entity.Property(e => e.Telegramid).HasColumnName("telegramid");

                entity.Property(e => e.Token).HasColumnName("token");

                entity.Property(e => e.Username).HasColumnName("username");

                entity.Property(e => e.Woods).HasColumnName("woods");
            });

            modelBuilder.Entity<ManagerDialog>(entity =>
            {
                entity.ToTable("ManagerDialog");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.IdTelegramManager).HasColumnName("idTelegramManager");

                entity.Property(e => e.Order).HasColumnName("order");

                entity.HasOne(d => d.OrderNavigation)
                    .WithMany(p => p.ManagerDialogs)
                    .HasForeignKey(d => d.Order)
                    .HasConstraintName("FK_ManagerDialog_To_Order");
            });

            modelBuilder.Entity<MessageTelegram>(entity =>
            {
                entity.ToTable("MessageTelegram");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Admin).HasColumnName("admin");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.Messageid).HasColumnName("messageid");

                entity.Property(e => e.Order).HasColumnName("order");

                entity.HasOne(d => d.AdminNavigation)
                    .WithMany(p => p.MessageTelegrams)
                    .HasForeignKey(d => d.Admin)
                    .HasConstraintName("FK_MessageTelegram_To_Admin");

                entity.HasOne(d => d.OrderNavigation)
                    .WithMany(p => p.MessageTelegrams)
                    .HasForeignKey(d => d.Order)
                    .HasConstraintName("FK_MessageTelegram_To_Order");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CardData).HasColumnName("cardData");

                entity.Property(e => e.City).HasColumnName("city");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.Fio).HasColumnName("fio");

                entity.Property(e => e.IsFinish)
                    .HasColumnName("isFinish")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.IsScoreAdd)
                    .HasColumnName("isScoreAdd")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LastUpdateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("lastUpdateDate");

                entity.Property(e => e.NameTrack).HasColumnName("nameTrack");

                entity.Property(e => e.Phone).HasColumnName("phone");

                entity.Property(e => e.Product).HasColumnName("product");

                entity.Property(e => e.ProductType).HasColumnName("productType");

                entity.Property(e => e.Score).HasColumnName("score");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Street).HasColumnName("street");

                entity.Property(e => e.TrackNumber).HasColumnName("trackNumber");

                entity.Property(e => e.User).HasColumnName("user");

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.Product)
                    .HasConstraintName("FK_Order_To_Product");

                entity.HasOne(d => d.ProductTypeNavigation)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.ProductType)
                    .HasConstraintName("FK_Order_To_ProductType");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.User)
                    .HasConstraintName("FK_Order_To_User");
            });

            modelBuilder.Entity<OrderMessage>(entity =>
            {
                entity.ToTable("OrderMessage");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Admin).HasColumnName("admin");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.IsWrittenAdmin)
                    .IsRequired()
                    .HasColumnName("isWrittenAdmin")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Message).HasColumnName("message");

                entity.Property(e => e.Minteger).HasColumnName("minteger");

                entity.Property(e => e.Order).HasColumnName("order");

                entity.HasOne(d => d.AdminNavigation)
                    .WithMany(p => p.OrderMessages)
                    .HasForeignKey(d => d.Admin)
                    .HasConstraintName("FK_OrderMessage_To_Admin");

                entity.HasOne(d => d.OrderNavigation)
                    .WithMany(p => p.OrderMessages)
                    .HasForeignKey(d => d.Order)
                    .HasConstraintName("FK_OrderMessage_To_Order");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Cost).HasColumnName("cost");

                entity.Property(e => e.DefaultColor).HasColumnName("defaultColor");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.IsActive)
                    .HasColumnName("isActive")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsSell)
                    .HasColumnName("isSell")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.Score).HasColumnName("score");

                entity.Property(e => e.Subcategory).HasColumnName("subcategory");

                entity.HasOne(d => d.SubcategoryNavigation)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.Subcategory)
                    .HasConstraintName("FK_Product_To_Subcategory");
            });

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("ProductImage");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Image).HasColumnName("image");

                entity.Property(e => e.Order).HasColumnName("order");

                entity.Property(e => e.Product).HasColumnName("product");

                entity.Property(e => e.ProductType).HasColumnName("productType");

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany(p => p.ProductImages)
                    .HasForeignKey(d => d.Product)
                    .HasConstraintName("FK_ProductImage_To_Product");

                entity.HasOne(d => d.ProductTypeNavigation)
                    .WithMany(p => p.ProductImages)
                    .HasForeignKey(d => d.ProductType)
                    .HasConstraintName("FK_ProductImage_To_ProductType");
            });

            modelBuilder.Entity<ProductType>(entity =>
            {
                entity.ToTable("ProductType");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Color).HasColumnName("color");

                entity.Property(e => e.Cost).HasColumnName("cost");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.IsActive)
                    .HasColumnName("isActive")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.NameType).HasColumnName("nameType");

                entity.Property(e => e.Product).HasColumnName("product");

                entity.Property(e => e.Score).HasColumnName("score");

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany(p => p.ProductTypes)
                    .HasForeignKey(d => d.Product)
                    .HasConstraintName("FK_ProductType_To_Product");
            });

            modelBuilder.Entity<ReferalStartup>(entity =>
            {
                entity.ToTable("ReferalStartup");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.IsBonus)
                    .HasColumnName("isBonus")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Referalid).HasColumnName("referalid");

                entity.Property(e => e.Userid).HasColumnName("userid");
            });

            modelBuilder.Entity<ReferalStatistic>(entity =>
            {
                entity.ToTable("ReferalStatistic");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.AllScore).HasColumnName("allScore");

                entity.Property(e => e.ClickOnLink).HasColumnName("clickOnLink");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.MadeOrder).HasColumnName("madeOrder");

                entity.Property(e => e.Paid).HasColumnName("paid");

                entity.Property(e => e.ShippedOrders).HasColumnName("shippedOrders");

                entity.Property(e => e.User).HasColumnName("user");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.ReferalStatistics)
                    .HasForeignKey(d => d.User)
                    .HasConstraintName("FK_ReferalStatistic_To_User");
            });

            modelBuilder.Entity<ScoreHistory>(entity =>
            {
                entity.ToTable("ScoreHistory");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.GivenOrWrittenOff).HasColumnName("givenOrWrittenOff");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.Score).HasColumnName("score");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.User).HasColumnName("user");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.ScoreHistories)
                    .HasForeignKey(d => d.User)
                    .HasConstraintName("FK_ScoreHistory_To_User");
            });

            modelBuilder.Entity<StoriesVisit>(entity =>
            {
                entity.ToTable("StoriesVisit");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.Stories).HasColumnName("stories");

                entity.Property(e => e.User).HasColumnName("user");

                entity.HasOne(d => d.StoriesNavigation)
                    .WithMany(p => p.StoriesVisits)
                    .HasForeignKey(d => d.Stories)
                    .HasConstraintName("FK_StoriesVisit_To_Stories");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.StoriesVisits)
                    .HasForeignKey(d => d.User)
                    .HasConstraintName("FK_StoriesVisit_To_User");
            });

            modelBuilder.Entity<Story>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Color).HasColumnName("color");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.Text).HasColumnName("text");

                entity.Property(e => e.Url)
                    .HasColumnType("text")
                    .HasColumnName("url");
            });

            modelBuilder.Entity<Subcategory>(entity =>
            {
                entity.ToTable("Subcategory");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Category).HasColumnName("category");

                entity.Property(e => e.Image).HasColumnName("image");

                entity.Property(e => e.IsActive)
                    .HasColumnName("isActive")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.HasOne(d => d.CategoryNavigation)
                    .WithMany(p => p.Subcategories)
                    .HasForeignKey(d => d.Category)
                    .HasConstraintName("FK_Subcategory_To_Category");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.City).HasColumnName("city");

                entity.Property(e => e.DateRegister)
                    .HasColumnType("datetime")
                    .HasColumnName("dateRegister");

                entity.Property(e => e.Fio).HasColumnName("fio");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.Lastimg).HasColumnName("lastimg");

                entity.Property(e => e.Phone).HasColumnName("phone");

                entity.Property(e => e.Referal).HasColumnName("referal");

                entity.Property(e => e.ReferalTracker).HasColumnName("referalTracker");

                entity.Property(e => e.Score).HasColumnName("score");

                entity.Property(e => e.Street).HasColumnName("street");

                entity.Property(e => e.Telegramid).HasColumnName("telegramid");

                entity.Property(e => e.Token).HasColumnName("token");

                entity.Property(e => e.Username).HasColumnName("username");

                entity.HasOne(d => d.ReferalNavigation)
                    .WithMany(p => p.InverseReferalNavigation)
                    .HasForeignKey(d => d.Referal)
                    .HasConstraintName("FK_Referal_To_User");
            });

            modelBuilder.Entity<UserDialog>(entity =>
            {
                entity.ToTable("UserDialog");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("isActive")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Order).HasColumnName("order");

                entity.HasOne(d => d.OrderNavigation)
                    .WithMany(p => p.UserDialogs)
                    .HasForeignKey(d => d.Order)
                    .HasConstraintName("FK_UserDialog_To_Order");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
