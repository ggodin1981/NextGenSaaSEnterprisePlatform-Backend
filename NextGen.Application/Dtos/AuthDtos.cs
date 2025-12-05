namespace NextGen.Application.Dtos;

public record LoginRequest(string UserName, string Password);
public record LoginResponse(string Token, string UserName, string Role, Guid TenantId);
