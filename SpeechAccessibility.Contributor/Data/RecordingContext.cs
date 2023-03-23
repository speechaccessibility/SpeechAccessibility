using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpeechAccessibility.Data.Entities;
using SpeechAccessibility.Models;

namespace SpeechAccessibility
{
    public class RecordingContext : DbContext
    {
        private readonly IConfiguration _config;

        public RecordingContext(IConfiguration config)
        {
            _config = config;
        }

        public DbSet<Recording> Recording { get; set; }
        public DbSet<Prompt> Prompt { get; set; }
        public DbSet<Block> Block { get; set; }
        public DbSet<BlockOfPrompts> BlockOfPrompts { get; set; }
        public DbSet<ContributorAssignedBlock> ContributorAssignedBlock { get; set; }
        public DbSet<RecordingStatus> RecordingStatus { get; set; }
        public DbSet<AssignedDigitalCommandBlock> AssignedDigitalCommandBlock { get; set; }
        public DbSet<List> List { get; set; }

        public DbSet<BlockOfDigitalCommand> BlockOfDigitalCommand { get; set; }
        public DbSet<BlockOfDigitalCommandPrompts> BlockOfDigitalCommandPrompts { get; set; }
        public DbSet<Category> Category { get; set; }

        public DbSet<BlockMasterOfPrompts> BlockMasterOfPrompts {get;set;}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(_config["ConnectionStrings:SpeechAccessibilityContextConnection"]);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {                      
            builder.Entity<Dimensions>()
                .Property(c => c.CreateTS)
            .HasDefaultValueSql("GETDATE()");
            builder.Entity<Prompt>()
                .Property(c => c.CreateTS)
            .HasDefaultValueSql("GETDATE()");
            builder.Entity<Recording>()
                .Property(c => c.CreateTS)
            .HasDefaultValueSql("GETDATE()");
            builder.Entity<Block>()
                .Property(c => c.CreateTS)
            .HasDefaultValueSql("GETDATE()");
            builder.Entity<BlockOfPrompts>()
                .Property(c => c.CreateTS)
            .HasDefaultValueSql("GETDATE()");
            builder.Entity<ContributorAssignedBlock>()
            .Property(c => c.CreateTS)
        .HasDefaultValueSql("GETDATE()");
            builder.Entity<AssignedDigitalCommandBlock>()
            .Property(c => c.CreateTS)
        .HasDefaultValueSql("GETDATE()");
            builder.Entity<Dimensions>()
                .Property(c => c.UpdateTS)
            .HasDefaultValueSql("GETDATE()");
            builder.Entity<Prompt>()
                .Property(c => c.UpdateTS)
            .HasDefaultValueSql("GETDATE()");
            builder.Entity<Recording>()
                .Property(c => c.UpdateTS)
            .HasDefaultValueSql("GETDATE()");
            builder.Entity<Block>()
              .Property(c => c.UpdateTS)
          .HasDefaultValueSql("GETDATE()");
            builder.Entity<ContributorAssignedBlock>()
             .Property(c => c.UpdateTS)
         .HasDefaultValueSql("GETDATE()");
            builder.Entity<AssignedDigitalCommandBlock>()
            .Property(c => c.UpdateTS)
        .HasDefaultValueSql("GETDATE()");

            base.OnModelCreating(builder);

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}