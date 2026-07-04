using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Interfaces;

public interface IUserAccountStore
{
    Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<User?> FindByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns false (without throwing) when a user with the same normalized email already exists.
    /// </summary>
    Task<bool> TryCreateAsync(User user, CancellationToken cancellationToken = default);
}
