namespace NextGen.Domain.Entities;

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }

    public Tenant? Tenant { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
