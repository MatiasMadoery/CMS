using Microsoft.EntityFrameworkCore;
using Control_Machine_Sistem.Models;

namespace Control_Machine_Sistem.Models
{
    public class AppDbContext : DbContext
    {
        //Use the connection dependency
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //Relationships between tables
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ////One to many relationship between Machine and Customer
            //modelBuilder.Entity<Machine>()
            //    .HasOne(p => p.Customer)
            //    .WithMany(c => c.Machines)
            //    .HasForeignKey(p => p.CustomerId);

            //modelBuilder.Entity<Machine>()
            //  .HasKey(ac => new { ac.CustomerId, ac.ModelId });

            modelBuilder.Entity<Machine>()
                .HasOne(ac => ac.Customer)
                .WithMany(p => p.Machines)
                .HasForeignKey(ac => ac.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Machine>()
                .HasOne(ac => ac.Model)
                .WithMany(a => a.Machines)
                .HasForeignKey(ac => ac.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

        }
        public DbSet<Customer> Customers{ get; set; } = default!;
        public DbSet<Machine> Machines { get; set; } = default!;        
        public DbSet<Model> Models { get; set; } = default!;      
        public DbSet<User> Users { get; set; } = default!;        

    }
}
