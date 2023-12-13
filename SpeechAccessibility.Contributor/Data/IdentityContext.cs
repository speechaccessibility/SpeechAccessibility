using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpeechAccessibility.Models;
using Microsoft.AspNetCore.Identity;
using SpeechAccessibility.Data.Entities;

namespace SpeechAccessibility.Data
{
    public class IdentityContext : IdentityDbContext<IdentityUser>
    {
        private readonly IConfiguration _config;

        public IdentityContext(IConfiguration config)
        {
            _config = config;
        }

        public DbSet<Contributor> Contributor { get; set; }

        public DbSet<ContributorStatus> ContributorStatus { get; set; }

        public DbSet<ContributorDetails> ContributorDetails { get; set; }   

        public DbSet<Consent> Consent { get; set; }

        public DbSet<RacialGroup> RacialGroup { get; set; }

        public DbSet<ContributorRace> ContributorRace { get; set; }

        public DbSet<LoginSession> LoginSession { get; set; }  
        
        public DbSet<Etiology> Etiology { get; set; }  
        
        public DbSet<LegalGuardian> LegalGuardian { get; set; }

        public DbSet<Assent> Assent { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(_config["ConnectionStrings:IdentityContextConnection"]);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Contributor>()
                .Property(c => c.CreateTS)
            .HasDefaultValueSql("GETDATE()");

            builder.Entity<Contributor>()
           .Property(s => s.UpdateTS)
            .HasDefaultValueSql("GETDATE()");

            builder.Entity<ContributorDetails>()
               .Property(c => c.CreateTS)
           .HasDefaultValueSql("GETDATE()");

            builder.Entity<ContributorDetails>()
           .Property(s => s.UpdateTS)
            .HasDefaultValueSql("GETDATE()");

            builder.Entity<ContributorRace>()
              .Property(c => c.CreateTS)
          .HasDefaultValueSql("GETDATE()");

            builder.Entity<ContributorRace>()
           .Property(s => s.UpdateTS)
            .HasDefaultValueSql("GETDATE()");

            builder.Entity<Consent>()
               .Property(c => c.CreateTS)
           .HasDefaultValueSql("GETDATE()");

            builder.Entity<Consent>()
           .Property(s => s.UpdateTS)
            .HasDefaultValueSql("GETDATE()");

            builder.Entity<LegalGuardian>()
              .Property(c => c.CreateTS)
          .HasDefaultValueSql("GETDATE()");

            builder.Entity<LegalGuardian>()
           .Property(s => s.UpdateTS)
            .HasDefaultValueSql("GETDATE()");

            builder.Entity<Assent>()
              .Property(c => c.CreateTS)
          .HasDefaultValueSql("GETDATE()");

            builder.Entity<Assent>()
           .Property(s => s.UpdateTS)
            .HasDefaultValueSql("GETDATE()");

            base.OnModelCreating(builder);
         
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
