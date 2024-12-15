using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Proiect_DAW.Models;

namespace Proiect_DAW.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductOrder> ProductOrders { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder
        modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //category - product: 1-to-many
            modelBuilder.Entity<Product>()
            .HasOne<Category>(a => a.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Cascade); // comportament implicit, ok aici;

            //product - user: 1-to-many
            modelBuilder.Entity<Product>()
            .HasOne<ApplicationUser>(a => a.User)
            .WithMany(c => c.Products)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            //review - product: 1-to-many
            modelBuilder.Entity<Review>()
            .HasOne<Product>(a => a.Product)
            .WithMany(c => c.Reviews)
            .HasForeignKey(a => a.ProductId);

            //user - product: 1-to-many
            modelBuilder.Entity<Review>()
            .HasOne<ApplicationUser>(a => a.User)
            .WithMany(c => c.Reviews)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Restricționează ștergerea;

            //definire primary key compus
            modelBuilder.Entity<ProductOrder>()
            .HasKey(ac => new { ac.ProductId, ac.OrderId });

            //product - productorder - order: many-to-many
            modelBuilder.Entity<ProductOrder>()
            .HasOne(ac => ac.Product)
            .WithMany(ac => ac.ProductOrders)
            .HasForeignKey(ac => ac.ProductId);

            modelBuilder.Entity<ProductOrder>()
            .HasOne(ac => ac.Order)
            .WithMany(ac => ac.ProductOrders)
            .HasForeignKey(ac => ac.OrderId);

            //order - user: 1-to-many
            modelBuilder.Entity<Order>()
            .HasOne<ApplicationUser>(a => a.User)
            .WithMany(c => c.Orders)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Restricționează ștergerea;
        }
    }
}
