using HireFlow.Application.Common;
using HireFlow.Application.DTOs.Applications;
using HireFlow.Application.Interfaces;
using HireFlow.Domain.Shared;
using MediatR;

namespace HireFlow.Application.Features.Applications.Queries.GetMyApplications;

public sealed class GetMyApplicationsQueryHandler(
    IJobApplicationRepository applicationRepository,
    ICurrentUser currentUser) : IRequestHandler<GetMyApplicationsQuery, Result<PagedResult<CandidateApplicationItemDto>>>
{
    public async Task<Result<PagedResult<CandidateApplicationItemDto>>> Handle(GetMyApplicationsQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.CandidateId is null)
            return Result.Failure<PagedResult<CandidateApplicationItemDto>>(new Error("Auth.Forbidden", "Only candidates can access this.", ErrorType.Forbidden));

        var page = query.Pagination.Page > 0 ? query.Pagination.Page : 1;
        var pageSize = Math.Clamp(query.Pagination.PageSize, 1, 50);

        var (items, totalCount) = await applicationRepository.GetCandidateApplicationsAsync(
            currentUser.CandidateId.Value, query.Status, page, pageSize, cancellationToken);

        var dtos = items.Select(a => new CandidateApplicationItemDto(
            a.Id, a.JobId, a.Job.Title, a.Job.Recruiter.CompanyName,
            a.Job.Status.ToString(), a.Status.ToString(), a.AppliedAt)).ToList();

        return Result.Success(new PagedResult<CandidateApplicationItemDto>(dtos, page, pageSize, totalCount));
    }
}
