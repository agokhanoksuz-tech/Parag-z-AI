namespace PriceFinderAI.Api.Contracts;

public sealed record RegisterRequest(string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthUserDto(string Id, string Email);
