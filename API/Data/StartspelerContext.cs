using API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class StartspelerContext : IdentityDbContext<Gebruiker>
    {
        public DbSet<BesteldProduct> BesteldeProducten { get; set; } = default!;

        public DbSet<Bestelling> Bestellingen { get; set; } = default!;

        public DbSet<Community> Communities { get; set; } = default!;

        public DbSet<Evenement> Evenementen { get; set; } = default!;

        public DbSet<EvenementenRegistratie> EvenementenRegistraties { get; set; } = default!;

        public DbSet<GebruikerRechten> GebruikerRechten { get; set; } = default!;
        public DbSet<Gebruiker> Gebruikers { get; set; } = default!;
        public DbSet<Product> Producten { get; set; } = default!;

        public DbSet<Rechten> Rechten { get; set; } = default!;

        public StartspelerContext(DbContextOptions<StartspelerContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>().ToTable("Product");
            modelBuilder.Entity<BesteldProduct>().ToTable("BesteldProduct");
            modelBuilder.Entity<Bestelling>().ToTable("Bestelling");
            modelBuilder.Entity<Community>().ToTable("Community");
            modelBuilder.Entity<Evenement>().ToTable("Evenement");
            modelBuilder.Entity<EvenementenRegistratie>().ToTable("EvenementenRegistratie");
            modelBuilder.Entity<Gebruiker>().ToTable("Gebruiker");
            modelBuilder.Entity<GebruikerRechten>().ToTable("GebruikerRechten");
            modelBuilder.Entity<Rechten>().ToTable("Rechten");
        }
    }
}