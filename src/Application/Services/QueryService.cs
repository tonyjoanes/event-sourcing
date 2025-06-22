using Application.Handlers;
using Application.Queries;
using Domain.ValueObjects;

namespace Application.Services;

public class QueryService
{
    private readonly GetAccountSummaryQueryHandler _accountSummaryHandler;
    private readonly GetTransactionHistoryQueryHandler _transactionHistoryHandler;
    private readonly GetBalanceAtQueryHandler _balanceAtHandler;

    public QueryService(
        GetAccountSummaryQueryHandler accountSummaryHandler,
        GetTransactionHistoryQueryHandler transactionHistoryHandler,
        GetBalanceAtQueryHandler balanceAtHandler
    )
    {
        _accountSummaryHandler = accountSummaryHandler;
        _transactionHistoryHandler = transactionHistoryHandler;
        _balanceAtHandler = balanceAtHandler;
    }

    public async Task<GetAccountSummaryQueryResult> GetAccountSummaryAsync(AccountId accountId)
    {
        var query = new GetAccountSummaryQuery { AccountId = accountId };

        return await _accountSummaryHandler.HandleAsync(query);
    }

    public async Task<GetTransactionHistoryQueryResult> GetTransactionHistoryAsync(
        AccountId accountId,
        int? limit = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null
    )
    {
        var query = new GetTransactionHistoryQuery
        {
            AccountId = accountId,
            Limit = limit,
            FromDate = fromDate,
            ToDate = toDate,
        };

        return await _transactionHistoryHandler.HandleAsync(query);
    }

    public async Task<GetBalanceAtQueryResult> GetBalanceAtAsync(
        AccountId accountId,
        DateTimeOffset atDate
    )
    {
        var query = new GetBalanceAtQuery { AccountId = accountId, AtDate = atDate };

        return await _balanceAtHandler.HandleAsync(query);
    }
}
