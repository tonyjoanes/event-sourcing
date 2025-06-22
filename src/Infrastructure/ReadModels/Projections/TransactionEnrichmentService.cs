using Infrastructure.ReadModels;

namespace Infrastructure.ReadModels.Projections;

public class TransactionEnrichmentService
{
    private readonly IReadModelStore _readModelStore;

    public TransactionEnrichmentService(IReadModelStore readModelStore)
    {
        _readModelStore = readModelStore;
    }

    public async Task EnrichTransactionAsync(TransactionHistoryProjection transaction)
    {
        if (string.IsNullOrEmpty(transaction.CustomerId))
        {
            var accountSummary = await _readModelStore.GetByIdAsync<AccountSummaryProjection>(
                transaction.AccountId
            );
            if (accountSummary != null)
            {
                transaction.CustomerId = accountSummary.CustomerId;
                await _readModelStore.UpdateAsync(transaction);
            }
        }
    }

    public async Task EnrichTransactionsAsync(
        IEnumerable<TransactionHistoryProjection> transactions
    )
    {
        foreach (var transaction in transactions)
        {
            await EnrichTransactionAsync(transaction);
        }
    }
}
