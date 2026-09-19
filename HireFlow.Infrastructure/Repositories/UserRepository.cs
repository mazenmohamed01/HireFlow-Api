using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Infrastructure.Persistence;

namespace HireFlow.Infrastructure.Repositories;

public sealed class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task InsertAsync(User user, CancellationToken cancellationToken = default)
        => await context.Users.AddAsync(user, cancellationToken);

    public void Update(User user)
        => context.Users.Update(user);

    public IQueryable<User> Query()
        => context.Users.AsQueryable();

    public void Remove(User user)
        => context.Users.Remove(user);
}
