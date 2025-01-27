using Microsoft.EntityFrameworkCore;
using Control_Machine_Sistem.Models;

namespace Control_Machine_Sistem.Models
{
    public class AppDbContext : DbContext
    {
        //Utilizacion de la dependencia de conxion.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //Relaciones entre tablas
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

        }
        public DbSet<Control_Machine_Sistem.Models.Customer> Customer { get; set; } = default!;
        public DbSet<Control_Machine_Sistem.Models.Machine> Machine { get; set; } = default!;
        public DbSet<Control_Machine_Sistem.Models.Manual> Manual { get; set; } = default!;
        public DbSet<Control_Machine_Sistem.Models.Service> Service { get; set; } = default!;
        public DbSet<Control_Machine_Sistem.Models.User> User { get; set; } = default!;

    }
}
