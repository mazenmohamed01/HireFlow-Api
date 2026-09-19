using HireFlow.Application.Interfaces;
using HireFlow.Domain.Entities;
using HireFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await context.Users
            .Include(u => u.Candidate)
            .Include(u => u.Recruiter)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByIdWithProfilesAsync(int id, CancellationToken cancellationToken = default)
        => await context.Users
            .Include(u => u.Candidate)
            .Include(u => u.Recruiter)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
}
