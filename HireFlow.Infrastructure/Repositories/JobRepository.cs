using HireFlow.Application.Services.Jobs;
using HireFlow.Domain.Entities;
using HireFlow.Infrastructure.Persistence;

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
}
