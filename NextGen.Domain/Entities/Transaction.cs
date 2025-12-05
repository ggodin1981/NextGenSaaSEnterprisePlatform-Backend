namespace NextGen.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string Type { get; set; } = "Credit";
    public string Description { get; set; } = string.Empty;

    public Account? Account { get; set; }
}
