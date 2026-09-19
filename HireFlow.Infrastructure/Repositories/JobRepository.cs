using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Domain.Enums;
using HireFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IJobRepository"/>.
/// </summary>
public sealed class JobRepository(ApplicationDbContext context) : IJobRepository
{
    public async Task InsertAsync(Job job, CancellationToken cancellationToken = default)
        => await context.Jobs.AddAsync(job, cancellationToken);

    public void Update(Job job)
        => context.Jobs.Update(job);

    public IQueryable<Job> Query()
        => context.Jobs.AsQueryable();

    public void Remove(Job job)
        => context.Jobs.Remove(job);

    public async Task<Job?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Jobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<Job?> GetByIdWithRecruiterAsync(int id, CancellationToken cancellationToken = default)
        => await context.Jobs
            .Include(j => j.Recruiter)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<(List<Job> Items, int TotalCount)> GetPagedOpenJobsAsync(
        string? search, string? jobType, string? location,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Jobs
            .Include(j => j.Recruiter)
            .Where(j => j.Status == JobStatus.Open);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(j => j.Title.Contains(search) || j.Description.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(jobType) && Enum.TryParse<JobType>(jobType, true, out var parsedType))
        {
            query = query.Where(j => j.JobType == parsedType);
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(j => j.Location != null && j.Location.Contains(location));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<(Job Job, int ApplicationsCount)> Items, int TotalCount)> GetPagedMyJobsAsync(
        int recruiterId, string? status,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Jobs.Where(j => j.RecruiterId == recruiterId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<JobStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(j => j.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new { Job = j, AppsCount = context.JobApplications.Count(a => a.JobId == j.Id) })
            .ToListAsync(cancellationToken);

        return (items.Select(x => (x.Job, x.AppsCount)).ToList(), totalCount);
    }
}
