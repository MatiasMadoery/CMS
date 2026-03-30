using Microsoft.EntityFrameworkCore;
using Control_Machine_Sistem.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Control_Machine_Sistem.Models
{
    public class AppDbContext : DbContext
    {
        //Use the connection dependency
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //Relationships between tables
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            modelBuilder.Entity<Machine>()
                .HasMany(m => m.Services)
                .WithOne(s => s.Machine)
                .HasForeignKey(s => s.MachineId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Machine>()
                .HasMany(m => m.OtherMaintenances)
                .WithOne(s => s.Machine)
                .HasForeignKey(s => s.MachineId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OwnerHistory>()
                .HasOne(oh => oh.Machine)
                .WithMany(m => m.OwnerHistories)
                .HasForeignKey(oh => oh.MachineId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Model>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Models)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Machine>()
                .Property(m => m.DeliveryDate)
                .IsRequired(false);

            modelBuilder.Entity<Machine>()
                .Property(m => m.WarrantyExpirationDate)
                .IsRequired(false);

            modelBuilder.Entity<Machine>()
                .HasOne(m => m.Ubication)
                .WithMany(u => u.Machines)
                .HasForeignKey(m => m.UbicationId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Configuración exclusiva para ImageUrls ---

            // 1. Definimos el Conversor: Transforma la lista en un JSON string para la DB y viceversa
            modelBuilder.Entity<Machine>()
                .Property(m => m.ImageUrls)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v), // De C# a la Base de Datos (JSON)
                    v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>() // De la DB a C#
                );

            // 2. Definimos el Comparador: Necesario para que EF detecte cambios dentro de la lista
            var imageComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );

            modelBuilder.Entity<Machine>()
                .Property(m => m.ImageUrls)
                .Metadata.SetValueComparer(imageComparer);
        }

        public DbSet<Customer> Customers { get; set; } = default!;
        public DbSet<Machine> Machines { get; set; } = default!;
        public DbSet<Model> Models { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<OwnerHistory> OwnerHistories { get; set; } = default!;
        public DbSet<Service> Services { get; set; } = default!;
        public DbSet<OtherMaintenance> OtherMaintenances { get; set; } = default!;
        public DbSet<Accessory> Accessories { get; set; } = default!;
        public DbSet<Ubication> Ubications { get; set; } = default!;
    }
}