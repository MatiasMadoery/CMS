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
            //One to many relationship between Machine and Customer
            modelBuilder.Entity<Machine>()
                .HasOne(p => p.Customer)
                .WithMany(c => c.Machines)
                .HasForeignKey(p => p.CustomerId);

            //One to many relationship between Service and Machine
            modelBuilder.Entity<Service>()
               .HasOne(p => p.Machine)
               .WithMany(c => c.Services)
               .HasForeignKey(p => p.MachineId);

            //One to many relationship between Manual and Machine
            modelBuilder.Entity<Manual>()
               .HasOne(p => p.Machine)
               .WithMany(c => c.Manuals)
               .HasForeignKey(p => p.MachineId);


        }
        public DbSet<Customer> Customers{ get; set; } = default!;
        public DbSet<Machine> Machines { get; set; } = default!;
        public DbSet<Manual> Manuals { get; set; } = default!;
        public DbSet<Service> Services { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;

    }
}
