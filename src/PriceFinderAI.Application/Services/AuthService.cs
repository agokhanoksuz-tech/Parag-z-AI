using Microsoft.AspNetCore.Identity;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Application.Services;

public enum RegisterOutcome
{
    Success,
    EmailAlreadyRegistered,
    WeakPassword
}

public enum LoginOutcome
{
    Success,
    InvalidCredentials
}

public sealed class AuthService(IUserAccountStore store, IPasswordHasher<User> hasher)
{
    public const int MinPasswordLength = 8;

    public async Task<(RegisterOutcome Outcome, User? User)> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (password.Length < MinPasswordLength)
            return (RegisterOutcome.WeakPassword, null);

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = email.Trim(),
            NormalizedEmail = NormalizeEmail(email),
            PasswordHash = string.Empty,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = hasher.HashPassword(user, password);

        var created = await store.TryCreateAsync(user, cancellationToken);
        return created ? (RegisterOutcome.Success, user) : (RegisterOutcome.EmailAlreadyRegistered, null);
    }

    public async Task<(LoginOutcome Outcome, User? User)> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await store.FindByEmailAsync(NormalizeEmail(email), cancellationToken);
        if (user is null)
            return (LoginOutcome.InvalidCredentials, null);

        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed
            ? (LoginOutcome.InvalidCredentials, null)
            : (LoginOutcome.Success, user);
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
