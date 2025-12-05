using NextGen.Domain.Entities;

namespace NextGen.Application.Services;

public interface IAiInsightService
{
    Task<string> BuildInsightForAccountAsync(Account account, CancellationToken cancellationToken = default);
}
