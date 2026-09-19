using HireFlow.Domain.Entities;

namespace HireFlow.Application.Interfaces;

public interface IUserRepository
{
    Task InsertAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
    IQueryable<User> Query();
    void Remove(User user);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithProfilesAsync(int id, CancellationToken cancellationToken = default);
}
