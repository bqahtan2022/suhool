namespace SuhoolTech.Api.Models;

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, string UserId, string Email, IList<string> Roles);
