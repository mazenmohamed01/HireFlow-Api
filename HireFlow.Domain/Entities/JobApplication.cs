
using HireFlow.Domain.Shared;
using HireFlow.Domain.Enums;
using HireFlow.Domain.Errors;

namespace HireFlow.Domain.Entities;

public class JobApplication
{
    public int Id { get; private set; }
    public int CandidateId { get; private set; }
    public Candidate Candidate { get; private set; } = null!;
    public int JobId { get; private set; }
    public Job Job { get; private set; } = null!;
    public ApplicationStatus Status { get; private set; }
    public DateTime AppliedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string CvUrl { get; private set; } = string.Empty;
    public string? CoverLetter { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    private JobApplication() { }

    public static Result<JobApplication> Create(
        int candidateId,
        Job job,
        string? requestCvUrl,
        string? profileCvUrl,
        string? coverLetter,
        DateTime now)
    {
        if (!job.IsOpen)
        {
            return Result.Failure<JobApplication>(DomainErrors.Job.NotOpen);
        }

        string cvUrl = !string.IsNullOrWhiteSpace(requestCvUrl)
            ? requestCvUrl
            : profileCvUrl ?? string.Empty;

        if (string.IsNullOrWhiteSpace(cvUrl))
        {
            return Result.Failure<JobApplication>(DomainErrors.Application.CvRequired);
        }

        var application = new JobApplication
        {
            CandidateId = candidateId,
            JobId = job.Id,
            Job = job,
            Status = ApplicationStatus.Applied,
            CvUrl = cvUrl,
            CoverLetter = coverLetter,
            AppliedAt = now
        };

        return Result.Success(application);
    }

    public Result Cancel(DateTime now)
    {
        if (Status != ApplicationStatus.Applied && Status != ApplicationStatus.UnderReview)
        {
            return Result.Failure(DomainErrors.Application.CannotCancel);
        }

        Status = ApplicationStatus.Cancelled;
        CancelledAt = now;
        UpdatedAt = now;

        return Result.Success();
    }

    public Result ChangeStatusByRecruiter(ApplicationStatus target, DateTime now)
    {
        if (target == ApplicationStatus.Applied || target == ApplicationStatus.Cancelled)
        {
            return Result.Failure(DomainErrors.Application.StatusNotAllowed);
        }

        if (IsTerminalStatus(Status))
        {
            return Result.Failure(DomainErrors.Application.InvalidTransition);
        }

        if (!IsValidTransition(Status, target))
        {
            return Result.Failure(DomainErrors.Application.InvalidTransition);
        }

        Status = target;
        UpdatedAt = now;

        return Result.Success();
    }

    private static bool IsTerminalStatus(ApplicationStatus status)
    {
        return status == ApplicationStatus.Accepted ||
               status == ApplicationStatus.Rejected ||
               status == ApplicationStatus.Cancelled;
    }

    private static bool IsValidTransition(ApplicationStatus from, ApplicationStatus to)
    {
        return from switch
        {
            ApplicationStatus.Applied =>
                to == ApplicationStatus.UnderReview ||
                to == ApplicationStatus.Rejected,
            ApplicationStatus.UnderReview =>
                to == ApplicationStatus.Interview ||
                to == ApplicationStatus.Rejected,
            ApplicationStatus.Interview =>
                to == ApplicationStatus.Accepted ||
                to == ApplicationStatus.Rejected,
            _ => false
        };
    }
}
