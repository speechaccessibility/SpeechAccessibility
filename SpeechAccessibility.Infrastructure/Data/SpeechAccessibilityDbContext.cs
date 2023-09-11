using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Infrastructure.Data
{
    public class SpeechAccessibilityDbContext : DbContext
    {
        public SpeechAccessibilityDbContext(DbContextOptions<SpeechAccessibilityDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<SubRole> SubRole { get; set; }
        public DbSet<UserSubRole> UserSubRole { get; set; }
        public DbSet<Block> Block { get; set; }
        public DbSet<BlockOfDigitalCommand> BlockOfDigitalCommand { get; set; }
        public DbSet<BlockOfPrompts> BlockOfPrompts { get; set; }
        public DbSet<BlockMasterOfPrompts> BlockMasterOfPrompts { get; set; }
        public DbSet<BlockMaster> BlockMaster { get; set; }
        public DbSet<BlockOfDigitalCommandPrompts> BlockOfDigitalCommandPrompts { get; set; }
        public DbSet<Category> Category { get; set; }
      
        public DbSet<ContributorAssignedAnnotator> ContributorAssignedAnnotator { get; set; }
        public DbSet<ContributorAssignedBlock> ContributorAssignedBlock { get; set; }
        public DbSet<ContributorCompensation> ContributorCompensation { get; set; }
        public DbSet<ContributorFollowUp> ContributorFollowUp { get; set; }
        public  DbSet<EmailLogging> EmailLogging { get; set; }
        public DbSet<Recording> Recording { get; set; }
        public DbSet<RecordingRating> RecordingRating { get; set; }
        public DbSet<Prompt> Prompt { get; set; }
        public DbSet<RecordingStatus> RecordingStatus { get; set; }
        public DbSet<SubCategory> SubCategory { get; set; }
        public DbSet<Dimension> Dimension { get; set; }
        public DbSet<DimensionCategory> DimensionCategory { get; set; }
        public DbSet<List> List { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>(ConfigureUser);
            builder.Entity<Role>(ConfigureRole);
            builder.Entity<SubRole>(ConfigureSubRole);
            builder.Entity<UserSubRole>(ConfigureUserSubRole);
            builder.Entity<Block>(ConfigureBlock);
            builder.Entity<BlockOfDigitalCommand>(ConfigureBlockOfDigitalCommand);
            builder.Entity<BlockOfDigitalCommandPrompts>(ConfigureBlockOfDigitalCommandPrompts);
            builder.Entity<BlockMaster>(ConfigureBlockMaster);
            builder.Entity<BlockMasterOfPrompts>(ConfigureBlockMasterOfPrompts);
            builder.Entity<BlockOfPrompts>(ConfigureBlockOfPrompts);
            builder.Entity<Category>(ConfigureCategory);
            builder.Entity<ContributorAssignedAnnotator>(ConfigureContributorAssignedAnnotator);
            builder.Entity<ContributorAssignedBlock>(ConfigureContributorAssignedBlock);
            builder.Entity<ContributorCompensation>(ConfigureContributorCompensation);
            builder.Entity<ContributorFollowUp>(ConfigureContributorFollowUp);
            builder.Entity<EmailLogging>(ConfigureEmailLogging);
            builder.Entity<Recording>(ConfigureRecording);
            builder.Entity<RecordingRating>(ConfigureRecordingRating);
            builder.Entity<Prompt>(ConfigurePrompt);
            builder.Entity<RecordingStatus>(ConfigureRecordingStatus);
            builder.Entity<SubCategory>(ConfigureSubCategory);
            builder.Entity<Dimension>(ConfigureDimension);
            builder.Entity<DimensionCategory>(ConfigureDimensionCategory);

        }
        

        private void ConfigureBlockMasterOfPrompts(EntityTypeBuilder<BlockMasterOfPrompts> entity)
        {
            entity.Property(e => e.Active)
                .IsRequired()
                .HasMaxLength(3)
                .IsFixedLength()
                .HasDefaultValueSql("('Yes')");

            entity.Property(e => e.CreateTS)
                .HasColumnName("CreateTS")
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.BlockMaster)
                .WithMany(p => p.BlockMasterOfPrompts)
                .HasForeignKey(d => d.BlockMasterId);

            entity.HasOne(d => d.Category)
                .WithMany(p => p.BlockMasterOfPrompts)
                .HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.Prompt)
                .WithMany(p => p.BlockMasterOfPrompts)
                .HasForeignKey(d => d.PromptId);
        }

        private void ConfigureBlockMaster(EntityTypeBuilder<BlockMaster> entity)
        {
            entity.Property(e => e.Active)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasDefaultValueSql("('Yes')");

            entity.Property(e => e.CreateTS)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Description).HasMaxLength(500);
        }

        private void ConfigureBlockOfDigitalCommandPrompts(EntityTypeBuilder<BlockOfDigitalCommandPrompts> entity)
        {
            entity.Property(e => e.Active)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasDefaultValueSql("('Yes')");

            entity.Property(e => e.CreateTS)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.BlockOfDigitalCommand)
                .WithMany(p => p.BlockOfDigitalCommandPrompts)
                .HasForeignKey(d => d.BlockOfDigitalCommandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BlockOfDigitalCommand_BlockOfDigitalCommand");

            entity.HasOne(d => d.Category)
                .WithMany(p => p.BlockOfDigitalCommandPrompts)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_BlockOfDigitalCommandPrompts_Category");

            entity.HasOne(d => d.Prompt)
                .WithMany(p => p.BlockOfDigitalCommandPrompts)
                .HasForeignKey(d => d.PromptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BlockOfDigitalCommandPrompts_Prompt");
        }

        private void ConfigureBlockOfDigitalCommand(EntityTypeBuilder<BlockOfDigitalCommand> entity)
        {
            entity.Property(e => e.Active)
                .IsRequired()
                .HasMaxLength(3)
                .IsFixedLength()
                .HasDefaultValueSql("('Yes')");

            entity.Property(e => e.CreateTS)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.UpdateTS)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.List)
                .WithMany(p => p.BlockOfDigitalCommand)
                .HasForeignKey(d => d.ListId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BlockOfDigitalCommand_List");

        }


        private void ConfigureBlock(EntityTypeBuilder<Block> entity)
        {
            entity.Property(e => e.CreateTS)
                .HasColumnType("datetime");

            entity.Property(e => e.UpdateTS)
                .HasColumnType("datetime");
        }

        private void ConfigureBlockOfPrompts(EntityTypeBuilder<BlockOfPrompts> entity)
        {
            entity.Property(e => e.Active)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasDefaultValueSql("('Yes')");

            entity.Property(e => e.CreateTS)
                .HasColumnName("CreateTS")
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.UpdateBy).HasMaxLength(50);

            entity.HasOne(d => d.Block)
                .WithMany(p => p.BlockOfPrompts)
                .HasForeignKey(d => d.BlockId);

            entity.HasOne(d => d.Prompt)
                .WithMany(p => p.BlockOfPrompts)
                .HasForeignKey(d => d.PromptId);

            entity.HasOne(d => d.Categorty)
                .WithMany(p => p.BlockOfPrompts)
                .HasForeignKey(d => d.CategoryId);
        }

        private void ConfigureCategory(EntityTypeBuilder<Category> entity)
        {
            entity.Property(e => e.Active)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasDefaultValueSql("('Yes')");

            entity.Property(e => e.CreateTS)
                .HasColumnName("CreateTS")
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Description).HasMaxLength(500);

            entity.Property(e => e.UpdateTS)
                .HasColumnName("UpdateTS")
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");
        }
        private void ConfigureContributorAssignedAnnotator(EntityTypeBuilder<ContributorAssignedAnnotator> entity)
        {
            entity.HasOne(d => d.User)
                .WithMany(p => p.ContributorAssignedAnnotator)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }


        private void ConfigureContributorAssignedBlock(EntityTypeBuilder<ContributorAssignedBlock> entity)
        {
            entity.Property(e => e.Active)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasDefaultValueSql("('Yes')");

            entity.Property(e => e.UpdateBy).HasMaxLength(50);

            entity.Property(e => e.UpdateTS)
                .HasColumnName("UpdateTS")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Block)
                .WithMany(p => p.ContributorAssignedBlock)
                .HasForeignKey(d => d.BlockId);
        }

        private void ConfigureContributorFollowUp(EntityTypeBuilder<ContributorFollowUp> entity)
        {
            entity.Property(e => e.EmailContent).HasColumnType("text");

            entity.Property(e => e.SendTS)
                .HasColumnType("datetime");
        }
        private void ConfigureContributorCompensation(EntityTypeBuilder<ContributorCompensation> entity)
        {
            entity.HasIndex(e => e.ContributorId)
                .HasName("ContributorCompensation_ContributorId_unique")
                .IsUnique();

            entity.Property(e => e.SendFirstCard).HasColumnType("datetime");

            entity.Property(e => e.SendSecondCard).HasColumnType("datetime");

            entity.Property(e => e.SendThirdCard).HasColumnType("datetime");
        }

        private void ConfigureEmailLogging(EntityTypeBuilder<EmailLogging> entity)
        {
            entity.Property(e => e.Message).HasColumnType("text");
            entity.Property(e=>e.Error).HasColumnType("text");
            entity.Property(e => e.SendBy).HasMaxLength(50);

            entity.Property(e => e.SendTS)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Subject).HasMaxLength(200);
        }


        private void ConfigureRecordingRating(EntityTypeBuilder<RecordingRating> entity)
        {

            entity.Property(e => e.Active)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasDefaultValueSql("('Yes')");

            entity.Property(e => e.Comment).HasMaxLength(500);

            entity.HasOne(d => d.Dimension)
                .WithMany(p => p.RecordingRating)
                .HasForeignKey(d => d.DimensionId);

            entity.HasOne(d => d.Recording)
                .WithMany(p => p.RecordingRating)
                .HasForeignKey(d => d.RecordingId);
        }

        private void ConfigureRecordingStatus(EntityTypeBuilder<RecordingStatus> entity)
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(50);
        }

        private void ConfigurePrompt(EntityTypeBuilder<Prompt> entity)
        {
            entity.Property(e => e.Active)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasDefaultValueSql("('Yes')");

            entity.Property(e => e.SeverityLevels).HasDefaultValueSql("((7))");

            entity.Property(e => e.CreateTS)
                .HasColumnName("CreateTS")
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");
            
            entity.Property(e => e.UpdateTS)
                .HasColumnName("UpdateTS")
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Category)
                .WithMany(p => p.Prompt)
                .HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.SubCategory)
                .WithMany(p => p.Prompt)
                .HasForeignKey(d => d.SubCategoryId);

        }

        
        private void ConfigureRecording(EntityTypeBuilder<Recording> entity)
        {
            entity.Property(e => e.CreateTS)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.FileName).HasMaxLength(500);

            entity.Property(e => e.UpdateTS)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Block)
                .WithMany(p => p.Recording)
                .HasForeignKey(d => d.BlockId);

            entity.HasOne(d => d.OriginalPrompt)
                .WithMany(p => p.Recording)
                .HasForeignKey(d => d.OriginalPromptId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Status)
                .WithMany(p => p.Recording)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

        }

      

        private void ConfigureRole(EntityTypeBuilder<Role> entity)
        {
            entity.Property(e => e.ADGroupName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Description).HasMaxLength(500);

            entity.Property(e => e.InUsed)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasDefaultValueSql("('Yes')");

            entity.Property(e => e.Name).HasMaxLength(50);

            entity.Property(e => e.UpdateTS)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");
        }

        private void ConfigureSubRole(EntityTypeBuilder<SubRole> entity)
        {
            entity.Property(e => e.InUsed)
                .IsRequired()
                .HasMaxLength(3)
                .IsFixedLength()
                .HasDefaultValueSql("('Yes')");

            entity.Property(e => e.UpdateTS)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Etiology)
                .WithMany(p => p.SubRole)
                .HasForeignKey(d => d.EtiologyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SubRole_Etiology");

            entity.HasOne(d => d.Role)
                .WithMany(p => p.SubRole)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SubRole_Role");

           
        }

        private void ConfigureUserSubRole(EntityTypeBuilder<UserSubRole> entity)
        {
            entity.Property(e => e.UpdateTS)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.SubRole)
                .WithMany(p => p.UserSubRole)
                .HasForeignKey(d => d.SubRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserSubRole_SubRole");

            entity.HasOne(d => d.User)
                .WithMany(p => p.UserSubRole)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserSubRole_User");

          

        }

        private void ConfigureUser(EntityTypeBuilder<User> entity)
        {
            entity.Property(e => e.Active)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasDefaultValueSql("('Yes')");

            entity.Property(e => e.FirstName).HasMaxLength(50);

            entity.Property(e => e.LastName).HasMaxLength(50);

            entity.Property(e => e.NetId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.UpdateBy).HasMaxLength(50);

            entity.Property(e => e.UpdateTS)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Role)
                .WithMany(p => p.User)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");
        }


        private void ConfigureSubCategory(EntityTypeBuilder<SubCategory> entity)
        {
            entity.Property(e => e.CreateTS)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.UpdateTS)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Category)
                .WithMany(p => p.SubCategory)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SubCategory_Category");
        }

        private void ConfigureDimensionCategory(EntityTypeBuilder<DimensionCategory> entity)
        {
            entity.Property(e => e.Active)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasDefaultValueSql("('Yes')");

            entity.Property(e => e.CreateTS)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);
        }

        private void ConfigureDimension(EntityTypeBuilder<Dimension> entity)
        {
            entity.Property(e => e.Active)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasDefaultValueSql("('Yes')");

            entity.Property(e => e.CreateTS)
                .HasColumnName("CreateTS")
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.UpdateTS)
                .HasColumnName("UpdateTS")
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.DimensionCategory)
                .WithMany(p => p.Dimension)
                .HasForeignKey(d => d.DimensionCategoryId);
        }


    }
}
