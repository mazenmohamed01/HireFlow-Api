using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Infrastructure.Repositories;

public sealed class JobApplicationRepository(ApplicationDbContext context) : IJobApplicationRepository
{
    public async Task InsertAsync(JobApplication application, CancellationToken cancellationToken = default)
        => await context.JobApplications.AddAsync(application, cancellationToken);

    public void Update(JobApplication application)
        => context.JobApplications.Update(application);

    public IQueryable<JobApplication> Query()
        => context.JobApplications.AsQueryable();

    public void Remove(JobApplication application)
        => context.JobApplications.Remove(application);

    public async Task<JobApplication?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.JobApplications.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<JobApplication?> GetByIdWithJobAsync(int id, CancellationToken cancellationToken = default)
        => await context.JobApplications
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<(List<JobApplication> Items, int TotalCount)> GetCandidateApplicationsAsync(
        int candidateId, string? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.JobApplications
            .Include(a => a.Job)
            .ThenInclude(j => j.Recruiter)
            .Where(a => a.CandidateId == candidateId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ApplicationStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(a => a.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<JobApplication> Items, int TotalCount)> GetJobApplicationsAsync(
        int jobId, string? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.JobApplications
            .Include(a => a.Candidate)
            .ThenInclude(c => c.User)
            .Where(a => a.JobId == jobId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ApplicationStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(a => a.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
