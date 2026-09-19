using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Infrastructure.Persistence;

namespace HireFlow.Infrastructure.Repositories;

public sealed class RecruiterRepository(ApplicationDbContext context) : IRecruiterRepository
{
    public async Task InsertAsync(Recruiter recruiter, CancellationToken cancellationToken = default)
        => await context.Recruiters.AddAsync(recruiter, cancellationToken);

    public void Update(Recruiter recruiter)
        => context.Recruiters.Update(recruiter);

    public IQueryable<Recruiter> Query()
        => context.Recruiters.AsQueryable();

    public void Remove(Recruiter recruiter)
        => context.Recruiters.Remove(recruiter);
}
