using HireFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HireFlow.Infrastructure.Persistence.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Job)
            .WithMany()
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Candidate)
            .WithMany()
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CvUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.CoverLetter)
            .HasMaxLength(4000);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Indexes

        // Filtered unique (CandidateId, JobId) WHERE Status <> 'Cancelled'
        builder.HasIndex(x => new { x.CandidateId, x.JobId })
            .IsUnique()
            .HasFilter("[Status] <> 'Cancelled'");

        builder.HasIndex(x => new { x.JobId, x.Status });

        builder.HasIndex(x => new { x.CandidateId, x.AppliedAt })
            .IsDescending(false, true); // CandidateId ASC, AppliedAt DESC
    }
}
