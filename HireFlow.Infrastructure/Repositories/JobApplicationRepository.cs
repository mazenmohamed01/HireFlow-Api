using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Infrastructure.Persistence;

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
}
