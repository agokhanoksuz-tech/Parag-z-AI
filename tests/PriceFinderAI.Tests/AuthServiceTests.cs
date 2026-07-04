using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PriceFinderAI.Application.Services;
using PriceFinderAI.Core.Models;
using PriceFinderAI.Infrastructure.Data;

namespace PriceFinderAI.Tests;

public class AuthServiceTests
{
    private static AuthService CreateService(out AppDbContext db)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        db = new AppDbContext(options);
        var store = new EfUserAccountStore(db);
        var hasher = new PasswordHasher<User>();
        return new AuthService(store, hasher);
    }

    [Fact]
    public async Task RegisterAsync_CreatesUser_WithHashedPassword()
    {
        var service = CreateService(out var db);

        var (outcome, user) = await service.RegisterAsync("Test@Example.com", "supersecret1");

        Assert.Equal(RegisterOutcome.Success, outcome);
        Assert.NotNull(user);
        Assert.NotEqual("supersecret1", user!.PasswordHash);
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task RegisterAsync_RejectsWeakPassword()
    {
        var service = CreateService(out var db);

        var (outcome, user) = await service.RegisterAsync("test@example.com", "short");

        Assert.Equal(RegisterOutcome.WeakPassword, outcome);
        Assert.Null(user);
        Assert.Equal(0, await db.Users.CountAsync());
    }

    [Fact]
    public async Task RegisterAsync_RejectsDuplicateEmail_CaseInsensitive()
    {
        var service = CreateService(out _);

        await service.RegisterAsync("test@example.com", "supersecret1");
        var (outcome, user) = await service.RegisterAsync("TEST@EXAMPLE.COM", "anotherpass1");

        Assert.Equal(RegisterOutcome.EmailAlreadyRegistered, outcome);
        Assert.Null(user);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_Succeeds_WithCorrectPassword()
    {
        var service = CreateService(out _);
        await service.RegisterAsync("test@example.com", "supersecret1");

        var (outcome, user) = await service.ValidateCredentialsAsync("test@example.com", "supersecret1");

        Assert.Equal(LoginOutcome.Success, outcome);
        Assert.NotNull(user);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_Fails_WithWrongPassword()
    {
        var service = CreateService(out _);
        await service.RegisterAsync("test@example.com", "supersecret1");

        var (outcome, user) = await service.ValidateCredentialsAsync("test@example.com", "wrongpassword");

        Assert.Equal(LoginOutcome.InvalidCredentials, outcome);
        Assert.Null(user);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_ReturnsSameOutcome_ForUnknownUserAndWrongPassword()
    {
        var service = CreateService(out _);
        await service.RegisterAsync("test@example.com", "supersecret1");

        var (unknownUserOutcome, _) = await service.ValidateCredentialsAsync("nobody@example.com", "whatever1");
        var (wrongPasswordOutcome, _) = await service.ValidateCredentialsAsync("test@example.com", "wrongpassword");

        Assert.Equal(LoginOutcome.InvalidCredentials, unknownUserOutcome);
        Assert.Equal(LoginOutcome.InvalidCredentials, wrongPasswordOutcome);
    }
}
