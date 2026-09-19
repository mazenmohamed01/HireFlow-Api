using HireFlow.Domain.Entities;

namespace HireFlow.Application.Interfaces;

public interface IJobApplicationRepository
{
    Task InsertAsync(JobApplication application, CancellationToken cancellationToken = default);
    void Update(JobApplication application);
    IQueryable<JobApplication> Query();
    void Remove(JobApplication application);

    Task<JobApplication?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<JobApplication?> GetByIdWithJobAsync(int id, CancellationToken cancellationToken = default);
    
    Task<(List<JobApplication> Items, int TotalCount)> GetCandidateApplicationsAsync(
        int candidateId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
        
    Task<(List<JobApplication> Items, int TotalCount)> GetJobApplicationsAsync(
        int jobId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
}
