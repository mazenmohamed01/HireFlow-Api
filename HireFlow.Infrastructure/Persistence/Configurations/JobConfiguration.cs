using HireFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireFlow.Infrastructure.Persistence.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Recruiter)
            .WithMany()
            .HasForeignKey(x => x.RecruiterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .IsRequired(); // No max length for description

        builder.Property(x => x.Location)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.JobType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(x => new { x.Status, x.CreatedAt })
            .IsDescending(false, true); // Status ASC, CreatedAt DESC

        builder.HasIndex(x => new { x.RecruiterId, x.Status });
    }
}
