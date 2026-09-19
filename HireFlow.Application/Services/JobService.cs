using HireFlow.Application.Common;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Shared;
using HireFlow.Application.DTOs.Jobs;
using HireFlow.Application.Services.Jobs;
using HireFlow.Domain.Entities;

namespace HireFlow.Application.Services.Jobs;

/// <summary>
/// Stub implementation of <see cref="IJobService"/>. Methods are completed phase by phase.
/// Phase 1 only provides CreateAsync as a working demonstration of the Result pipeline.
/// </summary>
#pragma warning disable CS9113 // currentUser and timeProvider used from Phase 6 onward
public sealed class JobService(
    IJobRepository jobRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IJobService
#pragma warning restore CS9113
{
    public async Task<Result<int>> CreateAsync(CreateJobRequest request, CancellationToken cancellationToken = default)
    {
        // Phase 6 will add: validator, domain factory, ownership, Mapster.
        // For Phase 1: thin placeholder that persists and returns the id.
        var jobResult = Job.Create(
            1, // Dummy recruiter ID for stub
            request.Title,
            request.Description,
            request.Location,
            HireFlow.Domain.Enums.JobType.FullTime,
            timeProvider.GetUtcNow().UtcDateTime);

        if (jobResult.IsFailure)
        {
            return Result.Failure<int>(jobResult.Error!);
        }

        var job = jobResult.Value;

        await jobRepository.InsertAsync(job, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(job.Id);
    }

    public Task<Result<PagedResult<JobListItemDto>>> GetOpenJobsAsync(JobsFilter filter, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success(new PagedResult<JobListItemDto>([], 1, 10, 0)));

    public Task<Result<JobDto>> GetByIdAsync(int jobId, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure<JobDto>(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound)));

    public Task<Result> UpdateAsync(int jobId, UpdateJobRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound)));

    public Task<Result> CloseAsync(int jobId, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound)));

    public Task<Result> ReopenAsync(int jobId, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound)));

    public Task<Result<PagedResult<RecruiterJobItemDto>>> GetMyJobsAsync(MyJobsFilter filter, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success(new PagedResult<RecruiterJobItemDto>([], 1, 10, 0)));
}
