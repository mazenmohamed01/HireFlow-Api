using HireFlow.Domain.Entities;

namespace HireFlow.Application.Interfaces;

/// <summary>
/// Repository abstraction for <see cref="Job"/> aggregate.
/// SaveChangesAsync is the responsibility of <see cref="Common.IUnitOfWork"/>.
/// </summary>
public interface IJobRepository
{
    Task InsertAsync(Job job, CancellationToken cancellationToken = default);
    void Update(Job job);
    IQueryable<Job> Query();
    void Remove(Job job);
}
