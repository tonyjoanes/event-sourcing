using Domain.ValueObjects;
using Infrastructure.ReadModels.Projections;
using OneOf;

namespace Application.Queries;

// Query result types
public record QuerySuccess<T>(T Data)
    where T : class;

public record QueryFailure(string Reason);

// Specific query results
public record AccountSummaryResult(
    AccountId AccountId,
    CustomerId CustomerId,
    Money Balance,
    AccountStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastTransactionAt
);

public record TransactionHistoryResult(
    AccountId AccountId,
    List<TransactionHistoryProjection> Transactions
);

public record BalanceAtResult(AccountId AccountId, Money Balance, DateTimeOffset AtDate);

// Query result unions using OneOf
public class GetAccountSummaryQueryResult
    : OneOfBase<QuerySuccess<AccountSummaryResult>, QueryFailure>
{
    public GetAccountSummaryQueryResult(
        OneOf<QuerySuccess<AccountSummaryResult>, QueryFailure> input
    )
        : base(input) { }

    public static implicit operator GetAccountSummaryQueryResult(
        QuerySuccess<AccountSummaryResult> success
    ) => new(success);

    public static implicit operator GetAccountSummaryQueryResult(QueryFailure failure) =>
        new(failure);
}

public class GetTransactionHistoryQueryResult
    : OneOfBase<QuerySuccess<TransactionHistoryResult>, QueryFailure>
{
    public GetTransactionHistoryQueryResult(
        OneOf<QuerySuccess<TransactionHistoryResult>, QueryFailure> input
    )
        : base(input) { }

    public static implicit operator GetTransactionHistoryQueryResult(
        QuerySuccess<TransactionHistoryResult> success
    ) => new(success);

    public static implicit operator GetTransactionHistoryQueryResult(QueryFailure failure) =>
        new(failure);
}

public class GetBalanceAtQueryResult : OneOfBase<QuerySuccess<BalanceAtResult>, QueryFailure>
{
    public GetBalanceAtQueryResult(OneOf<QuerySuccess<BalanceAtResult>, QueryFailure> input)
        : base(input) { }

    public static implicit operator GetBalanceAtQueryResult(
        QuerySuccess<BalanceAtResult> success
    ) => new(success);

    public static implicit operator GetBalanceAtQueryResult(QueryFailure failure) => new(failure);
}
