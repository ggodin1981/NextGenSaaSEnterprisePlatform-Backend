using System.Text;
using Microsoft.Extensions.Logging;
using NextGen.Application.Services;
using NextGen.Domain.Abstractions;
using NextGen.Domain.Entities;

namespace NextGen.Application.ServicesImpl;

public class AiInsightService : IAiInsightService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<AiInsightService> _logger;

    public AiInsightService(IUnitOfWork uow, ILogger<AiInsightService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<string> BuildInsightForAccountAsync(Account account, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Building AI insight for account {AccountId}", account.Id);

        var txns = await _uow.Transactions.ListAsync(t => t.AccountId == account.Id);
        var recent = txns
            .OrderByDescending(t => t.Date)
            .Take(20)
            .ToList();

        if (!recent.Any())
            return "No recent transactions found for this account. The balance appears stable with no recent activity.";

        var sb = new StringBuilder();
        sb.AppendLine($"Analyzing last {recent.Count} transactions for account '{account.Name}'.");
        sb.AppendLine($"Current balance: {account.Balance:C}.");

        var totalCredits = recent.Where(t => t.Type.Equals("Credit", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);
        var totalDebits = recent.Where(t => t.Type.Equals("Debit", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);

        sb.AppendLine($"Recent credits total: {totalCredits:C}. Recent debits total: {totalDebits:C}.");

        if (totalDebits > totalCredits)
        {
            sb.AppendLine("Spending is higher than income in the recent period. Consider reviewing recurring expenses.");
        }
        else
        {
            sb.AppendLine("Income is higher than spending in the recent period. The account trend appears healthy.");
        }

        var latest = recent.First();
        sb.AppendLine($"Most recent transaction: {latest.Type} of {latest.Amount:C} on {latest.Date:yyyy-MM-dd}, '{latest.Description}'.");

        return sb.ToString();
    }
}
