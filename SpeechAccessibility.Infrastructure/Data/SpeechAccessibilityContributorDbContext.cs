using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class SpeechAccessibilityContributorDbContext : DbContext
    {
        public SpeechAccessibilityContributorDbContext(DbContextOptions<SpeechAccessibilityContributorDbContext> options)
            : base(options)
        {
        }

        public  DbSet<Consent> Consent { get; set; }
        public DbSet<Contributor> Contributor { get; set; }
        public DbSet<ContributorDetails> ContributorDetails { get; set; }
        public  DbSet<ContributorRace> ContributorRace { get; set; }
        public  DbSet<ContributorStatus> ContributorStatus { get; set; }
        public  DbSet<RacialGroup> RacialGroup { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<Consent>(ConfigureConsent);
            builder.Entity<Contributor>(ConfigureContributor);
            builder.Entity<ContributorDetails>(ConfigureContributorDetails);
            builder.Entity<ContributorRace>(ConfigureContributorRace);
            builder.Entity<ContributorStatus>(ConfigureContributorStatus);
            builder.Entity<RacialGroup>(ConfigureRacialGroup);


        }

        private void ConfigureContributor(EntityTypeBuilder<Contributor> entity)
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            //entity.Property(e => e.Comments).HasMaxLength(2000);

            //entity.Property(e => e.CreateTS)
            //    .HasDefaultValueSql("(getdate())");

            //entity.Property(e => e.EighteenOrOlderInd)
            //    .IsRequired()
            //    .HasMaxLength(3);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            //entity.Property(e => e.HelperEmail).HasMaxLength(175);

            //entity.Property(e => e.HelperFirstName).HasMaxLength(50);

            //entity.Property(e => e.HelperLastName).HasMaxLength(50);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.MiddleName).HasMaxLength(30);

            //entity.Property(e => e.ParkinsonsInd)
            //    .IsRequired()
            //    .HasMaxLength(3);

          
            //entity.Property(e => e.StateResidence)
            //    .IsRequired()
            //    .HasMaxLength(4);

            //entity.Property(e => e.UnderstandSpeechInd)
            //    .IsRequired()
            //    .HasMaxLength(3);

            //entity.Property(e => e.UpdateTS)
            //    .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ContributorStatus)
                .WithMany(p => p.Contributor)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }

        private void ConfigureContributorDetails(EntityTypeBuilder<ContributorDetails> entity)
        {
            entity.HasIndex(e => e.ContributorId);

            entity.Property(e => e.Age)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.AvoidSpeechWhenTiredRating)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.ConverseInEnglish)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.ConverseInOtherLanguageAge).HasMaxLength(20);

            entity.Property(e => e.CreateTS)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.DifficultyHearingRating)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.FamiliarPeopleAtHomeRating)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.FamiliarPeopleOnPhoneRating)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.FrustratedBySpeechRating)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.Gender)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.HispanicOrLatino)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.ImpactSocialActivitiesRating)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.LongConversationRating)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.NoisyEnvironmentRating)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.RelyOnOthersRating)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.RepeatMyselfRating)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.SpeechChange)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.State)
                .IsRequired()
                .HasMaxLength(4);

            entity.Property(e => e.TravelInCarRating)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.UnfamiliarPeopleAtHomeRating)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.UnfamiliarPeopleOnPhoneRating)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.UpdateTS)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.UpsetOrAngryRating)
                .IsRequired()
                .HasMaxLength(30);

            entity.HasOne(d => d.Contributor)
                .WithMany(p => p.ContributorDetails)
                .HasForeignKey(d => d.ContributorId);
        }

        private void ConfigureConsent(EntityTypeBuilder<Consent> entity)
        {
            entity.HasIndex(e => e.ContributorId);

            entity.Property(e => e.CreateTS)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Name).HasMaxLength(50);

            entity.Property(e => e.UpdateTS)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Version)
                .IsRequired()
                .HasMaxLength(3);

            entity.HasOne(d => d.Contributor)
                .WithMany(p => p.Consent)
                .HasForeignKey(d => d.ContributorId);
        }


        private void ConfigureContributorRace(EntityTypeBuilder<ContributorRace> entity)
        {
            entity.HasIndex(e => e.ContributorDetailsId);

            entity.HasIndex(e => e.RacialGroupId);

            entity.Property(e => e.CreateTS)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.UpdateTS)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ContributorDetails)
                .WithMany(p => p.ContributorRace)
                .HasForeignKey(d => d.ContributorDetailsId);

            entity.HasOne(d => d.RacialGroup)
                .WithMany(p => p.ContributorRace)
                .HasForeignKey(d => d.RacialGroupId);
        }

        private void ConfigureContributorStatus(EntityTypeBuilder<ContributorStatus> entity)
        {
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
        }


        private void ConfigureRacialGroup(EntityTypeBuilder<RacialGroup> entity)
        {
         entity.Property(e => e.RacialGroupName).HasMaxLength(50);

            
        }


    }
}
