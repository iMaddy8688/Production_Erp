using Microsoft.EntityFrameworkCore;
using Production_Erp_Web_App.Domain.Entities;

namespace Production_Erp_Web_App.DbApp
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options)
        {
        }

        public DbSet<Item> Items => Set<Item>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
        public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
        public DbSet<Employee> Employees => Set<Employee>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // NOTE: as the Master Data module grows (per the design documents),
            // move each of these into its own IEntityTypeConfiguration<T> class
            // under Persistence/Configurations/. Kept inline here to keep this
            // first Dashboard slice small and easy to review.

            modelBuilder.Entity<Item>(e =>
            {
                e.Property(x => x.Sku).HasMaxLength(50).IsRequired();
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                e.Property(x => x.Rate).HasColumnType("decimal(18,2)");
                e.HasIndex(x => x.Sku).IsUnique();
            });

            modelBuilder.Entity<Customer>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                e.Property(x => x.Email).HasMaxLength(200);
                e.Property(x => x.Phone).HasMaxLength(30);
            });

            modelBuilder.Entity<Supplier>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                e.Property(x => x.Email).HasMaxLength(200);
                e.Property(x => x.Phone).HasMaxLength(30);
            });

            modelBuilder.Entity<SalesInvoice>(e =>
            {
                e.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
                e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
                e.HasIndex(x => x.InvoiceNumber).IsUnique();

                e.HasOne(x => x.Customer)
                    .WithMany(c => c.SalesInvoices)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PurchaseInvoice>(e =>
            {
                e.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
                e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
                e.HasIndex(x => x.InvoiceNumber).IsUnique();

                e.HasOne(x => x.Supplier)
                    .WithMany(s => s.PurchaseInvoices)
                    .HasForeignKey(x => x.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Employee>(e =>
            {
                e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
                e.Property(x => x.Designation).HasMaxLength(100);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
