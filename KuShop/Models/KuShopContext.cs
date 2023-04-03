using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace KuShop.Models
{
    public partial class KuShopContext : DbContext
    {
        public KuShopContext()
        {
        }

        public KuShopContext(DbContextOptions<KuShopContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Brand> Brands { get; set; } = null!;
        public virtual DbSet<BuyDtl> BuyDtls { get; set; } = null!;
        public virtual DbSet<Buying> Buyings { get; set; } = null!;
        public virtual DbSet<Cart> Carts { get; set; } = null!;
        public virtual DbSet<CartDtl> CartDtls { get; set; } = null!;
        public virtual DbSet<Customer> Customers { get; set; } = null!;
        public virtual DbSet<Duty> Duties { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<ProductType> ProductTypes { get; set; } = null!;
        public virtual DbSet<Staff> Staffs { get; set; } = null!;
        public virtual DbSet<Supplier> Suppliers { get; set; } = null!;
        public virtual DbSet<Work> Works { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=127.0.0.1;Initial Catalog=KuShop;User ID=webdev;Password=1234");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.Property(e => e.BrandId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("BrandID");

                entity.Property(e => e.BrandName).HasMaxLength(50);
            });

            modelBuilder.Entity<BuyDtl>(entity =>
            {
                entity.HasKey(e => new { e.BuyId, e.PdId })
                    .HasName("PK_BuyDtl");

                entity.Property(e => e.BuyId)
                    .HasMaxLength(50)
                    .HasColumnName("BuyID");

                entity.Property(e => e.PdId)
                    .HasMaxLength(50)
                    .HasColumnName("PdID");

                entity.Property(e => e.BdtlMoney).HasColumnName("BDtlMoney");

                entity.Property(e => e.BdtlPrice).HasColumnName("BDtlPrice");

                entity.Property(e => e.BdtlQty).HasColumnName("BDtlQty");
            });

            modelBuilder.Entity<Buying>(entity =>
            {
                entity.HasKey(e => e.BuyId);

                entity.ToTable("Buying");

                entity.Property(e => e.BuyId).HasMaxLength(50);

                entity.Property(e => e.BuyDate).HasColumnType("date");

                entity.Property(e => e.BuyDocId)
                    .HasMaxLength(50)
                    .HasColumnName("BuyDocID");

                entity.Property(e => e.BuyRemark).HasColumnType("text");

                entity.Property(e => e.Saleman).HasMaxLength(50);

                entity.Property(e => e.StfId)
                    .HasMaxLength(50)
                    .HasColumnName("StfID");

                entity.Property(e => e.SupId)
                    .HasMaxLength(50)
                    .HasColumnName("SupID");
            });

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.Property(e => e.CartId)
                    .HasMaxLength(50)
                    .HasColumnName("CartID");

                entity.Property(e => e.CartCf)
                    .HasMaxLength(1)
                    .HasColumnName("CartCF")
                    .HasDefaultValueSql("(N'N')")
                    .IsFixedLength();

                entity.Property(e => e.CartDate).HasColumnType("date");

                entity.Property(e => e.CartPay)
                    .HasMaxLength(1)
                    .HasDefaultValueSql("(N'N')")
                    .IsFixedLength();

                entity.Property(e => e.CartSend)
                    .HasMaxLength(1)
                    .HasDefaultValueSql("(N'N')")
                    .IsFixedLength();

                entity.Property(e => e.CartVoid)
                    .HasMaxLength(1)
                    .HasDefaultValueSql("(N'N')")
                    .IsFixedLength();

                entity.Property(e => e.CusId)
                    .HasMaxLength(50)
                    .HasColumnName("CusID");
            });

            modelBuilder.Entity<CartDtl>(entity =>
            {
                entity.HasKey(e => new { e.CartId, e.PdId })
                    .HasName("PK_CartDtl");

                entity.Property(e => e.CartId)
                    .HasMaxLength(50)
                    .HasColumnName("CartID");

                entity.Property(e => e.PdId)
                    .HasMaxLength(50)
                    .HasColumnName("PdID");

                entity.Property(e => e.CdtlMoney).HasColumnName("CDtlMoney");

                entity.Property(e => e.CdtlPrice).HasColumnName("CDtlPrice");

                entity.Property(e => e.CdtlQty).HasColumnName("CDtlQty");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CusId);

                entity.Property(e => e.CusId)
                    .HasMaxLength(50)
                    .HasColumnName("CusID");

                entity.Property(e => e.CusAdd).HasColumnType("text");

                entity.Property(e => e.CusEmail).HasMaxLength(50);

                entity.Property(e => e.CusLogin).HasMaxLength(50);

                entity.Property(e => e.CusName).HasMaxLength(100);

                entity.Property(e => e.CusPass).HasMaxLength(50);

                entity.Property(e => e.LastLogin).HasColumnType("date");

                entity.Property(e => e.StartDate).HasColumnType("date");
            });

            modelBuilder.Entity<Duty>(entity =>
            {
                entity.ToTable("Duty");

                entity.Property(e => e.DutyId)
                    .HasMaxLength(50)
                    .HasColumnName("DutyID");

                entity.Property(e => e.DutyName).HasMaxLength(50);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.PdId);

                entity.Property(e => e.PdId)
                    .HasMaxLength(50)
                    .HasColumnName("PdID");

                entity.Property(e => e.Active)
                    .HasMaxLength(1)
                    .HasDefaultValueSql("(N'Y')");

                entity.Property(e => e.BrandId).HasColumnName("BrandID");

                entity.Property(e => e.PdLastBuy).HasColumnType("date");

                entity.Property(e => e.PdLastSale).HasColumnType("date");

                entity.Property(e => e.PdName).HasMaxLength(50);

                entity.Property(e => e.PdtId).HasColumnName("PdtID");
            });

            modelBuilder.Entity<ProductType>(entity =>
            {
                entity.HasKey(e => e.PdtId);

                entity.Property(e => e.PdtId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("PdtID");

                entity.Property(e => e.PdtName).HasMaxLength(50);
            });

            modelBuilder.Entity<Staff>(entity =>
            {
                entity.HasKey(e => e.StfId)
                    .HasName("PK_Employees");

                entity.Property(e => e.StfId)
                    .HasMaxLength(50)
                    .HasColumnName("StfID");

                entity.Property(e => e.DutyId)
                    .HasMaxLength(50)
                    .HasColumnName("DutyID");

                entity.Property(e => e.QuitDate).HasColumnType("date");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.StfName).HasMaxLength(100);

                entity.Property(e => e.StfPass).HasMaxLength(50);
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.SupAdd).HasColumnType("text");

                entity.Property(e => e.SupContact).HasMaxLength(100);

                entity.Property(e => e.SupEmail).HasMaxLength(50);

                entity.Property(e => e.SupId)
                    .HasMaxLength(50)
                    .HasColumnName("SupID");

                entity.Property(e => e.SupName).HasMaxLength(100);

                entity.Property(e => e.SupRemark).HasColumnType("text");

                entity.Property(e => e.SupTel).HasMaxLength(50);
            });

            modelBuilder.Entity<Work>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.StfId).HasMaxLength(50);

                entity.Property(e => e.WorkDate).HasColumnType("date");

                entity.Property(e => e.WorkIn).HasMaxLength(8);

                entity.Property(e => e.WorkOut).HasMaxLength(8);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
