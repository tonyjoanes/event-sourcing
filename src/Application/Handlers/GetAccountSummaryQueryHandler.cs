using Application.Queries;
using Domain.ValueObjects;
using Infrastructure.ReadModels;
using Infrastructure.ReadModels.Projections;

namespace Application.Handlers;

public class GetAccountSummaryQueryHandler
{
    private readonly IReadModelStore _readModelStore;

    public GetAccountSummaryQueryHandler(IReadModelStore readModelStore)
    {
        _readModelStore = readModelStore;
    }

    public async Task<GetAccountSummaryQueryResult> HandleAsync(GetAccountSummaryQuery query)
    {
        try
        {
            var accountSummary = await _readModelStore.GetAccountSummaryAsync(
                query.AccountId.Value
            );

            if (accountSummary == null)
            {
                return new QueryFailure($"Account {query.AccountId} not found");
            }

            var result = new AccountSummaryResult(
                new AccountId(accountSummary.Id),
                new CustomerId(accountSummary.CustomerId),
                new Money(accountSummary.Balance, accountSummary.Currency),
                Enum.Parse<AccountStatus>(accountSummary.Status),
                accountSummary.OpenedAt,
                accountSummary.LastTransactionAt
            );

            return new QuerySuccess<AccountSummaryResult>(result);
        }
        catch (Exception ex)
        {
            return new QueryFailure($"Failed to get account summary: {ex.Message}");
        }
    }
}
