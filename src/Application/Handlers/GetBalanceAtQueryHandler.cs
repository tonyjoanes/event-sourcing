using Application.Queries;
using Domain.Aggregates;
using Domain.ValueObjects;
using Infrastructure.Repositories;

namespace Application.Handlers;

public class GetBalanceAtQueryHandler
{
    private readonly AccountRepository _accountRepository;

    public GetBalanceAtQueryHandler(AccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<GetBalanceAtQueryResult> HandleAsync(GetBalanceAtQuery query)
    {
        try
        {
            // Get all events for the account up to the specified date
            var events = await _accountRepository.GetEventsUpToAsync(query.AccountId, query.AtDate);

            if (!events.Any())
            {
                return new QueryFailure($"No events found for account {query.AccountId}");
            }

            // Reconstruct account state at the specified date
            var account = new Account();
            account.LoadFromHistory(events);

            var result = new BalanceAtResult(query.AccountId, account.Balance, query.AtDate);

            return new QuerySuccess<BalanceAtResult>(result);
        }
        catch (Exception ex)
        {
            return new QueryFailure($"Failed to get balance at {query.AtDate}: {ex.Message}");
        }
    }
}
