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
    
    Task<Job?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Job?> GetByIdWithRecruiterAsync(int id, CancellationToken cancellationToken = default);
    
    Task<(List<Job> Items, int TotalCount)> GetPagedOpenJobsAsync(
        string? search, string? jobType, string? location, 
        int page, int pageSize, CancellationToken cancellationToken = default);
        
    Task<(List<(Job Job, int ApplicationsCount)> Items, int TotalCount)> GetPagedMyJobsAsync(
        int recruiterId, string? status, 
        int page, int pageSize, CancellationToken cancellationToken = default);

    Task<List<Job>> GetExpiredOpenJobsAsync(int daysOld, CancellationToken cancellationToken = default);
}
