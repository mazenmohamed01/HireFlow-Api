namespace HireFlow.Application.Common;

/// <summary>
/// Unit-of-work abstraction. Flush all tracked changes to the database in a single transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
