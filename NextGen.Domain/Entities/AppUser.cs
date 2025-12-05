namespace NextGen.Domain.Entities;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public Guid TenantId { get; set; }
}
