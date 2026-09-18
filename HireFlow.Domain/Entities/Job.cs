
using HireFlow.Domain.Shared;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Errors;

namespace HireFlow.Domain.Entities;

public class Job
{
    public int Id { get; private set; }
    public int RecruiterId { get; private set; }
    public Recruiter Recruiter { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public JobType JobType { get; private set; }
    public JobStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    // EF Core requires a parameterless constructor
    private Job() { }

    public static Result<Job> Create(
        int recruiterId,
        string title,
        string description,
        string? location,
        JobType jobType,
        DateTime now)
    {
        var job = new Job
        {
            RecruiterId = recruiterId,
            Title = title,
            Description = description,
            Location = location,
            JobType = jobType,
            Status = JobStatus.Open,
            CreatedAt = now
        };

        return Result.Success(job);
    }

    public Result Update(
        string title,
        string description,
        string? location,
        JobType jobType,
        DateTime now)
    {
        if (Status != JobStatus.Open)
        {
            return Result.Failure(DomainErrors.Job.NotEditable);
        }

        Title = title;
        Description = description;
        Location = location;
        JobType = jobType;
        UpdatedAt = now;

        return Result.Success();
    }

    public Result Close(DateTime now)
    {
        if (Status == JobStatus.Closed)
        {
            return Result.Failure(DomainErrors.Job.AlreadyClosed);
        }

        Status = JobStatus.Closed;
        ClosedAt = now;

        return Result.Success();
    }

    public Result Reopen(DateTime now)
    {
        if (Status == JobStatus.Open)
        {
            return Result.Failure(DomainErrors.Job.AlreadyOpen);
        }

        Status = JobStatus.Open;
        ClosedAt = null;

        return Result.Success();
    }

    public bool IsOpen => Status == JobStatus.Open;
}
