namespace NextGen.Application.Dtos;

public record TransactionDto(
    Guid Id,
    DateTime Date,
    decimal Amount,
    string Type,
    string Description
);

public record CreateTransactionRequest(
    DateTime Date,
    decimal Amount,
    string Type,
    string Description
);

public record UpdateTransactionRequest(
    DateTime Date,
    decimal Amount,
    string Type,
    string Description
);
