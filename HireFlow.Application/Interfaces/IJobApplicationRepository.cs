using HireFlow.Domain.Entities;

namespace HireFlow.Application.Interfaces;

public interface IJobApplicationRepository
{
    Task InsertAsync(JobApplication application, CancellationToken cancellationToken = default);
    void Update(JobApplication application);
    IQueryable<JobApplication> Query();
    void Remove(JobApplication application);
}
