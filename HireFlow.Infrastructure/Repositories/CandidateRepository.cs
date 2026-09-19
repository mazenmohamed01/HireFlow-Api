using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Infrastructure.Persistence;

namespace HireFlow.Infrastructure.Repositories;

public sealed class CandidateRepository(ApplicationDbContext context) : ICandidateRepository
{
    public async Task InsertAsync(Candidate candidate, CancellationToken cancellationToken = default)
        => await context.Candidates.AddAsync(candidate, cancellationToken);

    public void Update(Candidate candidate)
        => context.Candidates.Update(candidate);

    public IQueryable<Candidate> Query()
        => context.Candidates.AsQueryable();

    public void Remove(Candidate candidate)
        => context.Candidates.Remove(candidate);
}
