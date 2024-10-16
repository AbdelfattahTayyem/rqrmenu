using Microsoft.EntityFrameworkCore;
using rqrmenu.Areas.Dashboard.Models;

namespace rqrmenu.Areas.Dashboard.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Category { get; set; }
        public DbSet<Contactus> Contactus { get; set; }
        public DbSet<Extra> Extra { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<Table> Table { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);



            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => oi.Id);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Item)
                .WithMany(i => i.OrderItems)
                .HasForeignKey(oi => oi.ItemId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            // Configure many-to-many relationship through OrderItemExtra
            modelBuilder.Entity<OrderItemExtra>()
                .HasKey(oe => new { oe.OrderItemId, oe.ExtraId });

            modelBuilder.Entity<OrderItemExtra>()
                .HasOne(oe => oe.OrderItem)
                .WithMany(oi => oi.OrderItemExtras)
                .HasForeignKey(oe => oe.OrderItemId);

            modelBuilder.Entity<OrderItemExtra>()
                .HasOne(oe => oe.Extra)
                .WithMany(e => e.OrderItemExtras)
                .HasForeignKey(oe => oe.ExtraId);
        }

    }
}
