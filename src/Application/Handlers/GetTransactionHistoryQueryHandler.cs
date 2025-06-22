using Application.Queries;
using Infrastructure.ReadModels;

namespace Application.Handlers;

public class GetTransactionHistoryQueryHandler
{
    private readonly IReadModelStore _readModelStore;

    public GetTransactionHistoryQueryHandler(IReadModelStore readModelStore)
    {
        _readModelStore = readModelStore;
    }

    public async Task<GetTransactionHistoryQueryResult> HandleAsync(
        GetTransactionHistoryQuery query
    )
    {
        try
        {
            var transactions = await _readModelStore.GetTransactionHistoryAsync(
                query.AccountId.Value,
                query.Limit ?? 50,
                query.FromDate,
                query.ToDate
            );

            var result = new TransactionHistoryResult(query.AccountId, transactions);

            return new QuerySuccess<TransactionHistoryResult>(result);
        }
        catch (Exception ex)
        {
            return new QueryFailure($"Failed to get transaction history: {ex.Message}");
        }
    }
}
