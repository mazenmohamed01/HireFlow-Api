using HireFlow.Application.DTOs.Jobs;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Jobs.Queries.GetJobById;

public sealed class GetJobByIdQueryHandler(
    IJobRepository jobRepository) : IRequestHandler<GetJobByIdQuery, Result<JobDto>>
{
    public async Task<Result<JobDto>> Handle(GetJobByIdQuery query, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdWithRecruiterAsync(query.JobId, cancellationToken);
        if (job is null)
            return Result.Failure<JobDto>(new Error("Job.NotFound", "Job not found.", ErrorType.NotFound));

        var dto = new JobDto(
            job.Id, job.RecruiterId, job.Recruiter.CompanyName, job.Title, job.Description,
            job.Location, job.JobType.ToString(), job.Status.ToString(), job.CreatedAt, job.UpdatedAt, job.ClosedAt);

        return Result.Success(dto);
    }
}
