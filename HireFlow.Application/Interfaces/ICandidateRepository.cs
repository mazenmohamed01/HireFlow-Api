using HireFlow.Domain.Entities;

namespace HireFlow.Application.Interfaces;

public interface ICandidateRepository
{
    Task InsertAsync(Candidate candidate, CancellationToken cancellationToken = default);
    void Update(Candidate candidate);
    IQueryable<Candidate> Query();
    void Remove(Candidate candidate);
}
