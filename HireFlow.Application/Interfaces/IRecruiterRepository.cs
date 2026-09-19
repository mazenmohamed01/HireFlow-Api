using HireFlow.Domain.Entities;

namespace HireFlow.Application.Interfaces;

public interface IRecruiterRepository
{
    Task InsertAsync(Recruiter recruiter, CancellationToken cancellationToken = default);
    void Update(Recruiter recruiter);
    IQueryable<Recruiter> Query();
    void Remove(Recruiter recruiter);
}
