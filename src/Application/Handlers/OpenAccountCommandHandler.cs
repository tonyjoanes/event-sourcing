using Application.Commands;
using Domain.Aggregates;
using Domain.ValueObjects;
using Infrastructure.ReadModels.Projections;
using Infrastructure.Repositories;

namespace Application.Handlers;

public class OpenAccountCommandHandler
{
    private readonly AccountRepository _accountRepository;
    private readonly EventDispatcher _eventDispatcher;

    public OpenAccountCommandHandler(
        AccountRepository accountRepository,
        EventDispatcher eventDispatcher
    )
    {
        _accountRepository = accountRepository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<OpenAccountCommandResult> HandleAsync(OpenAccountCommand command)
    {
        try
        {
            // Create account using domain logic
            var accountResult = Account.Open(command.CustomerId, command.InitialBalance);

            if (accountResult.IsT1) // ValidationError
            {
                return new CommandFailure(accountResult.AsT1.Message);
            }

            var account = accountResult.AsT0;

            // Save to event store
            await _accountRepository.SaveAsync(account);

            // Publish events to update read models
            foreach (var @event in account.UncommittedEvents)
            {
                await _eventDispatcher.DispatchAsync(@event);
            }

            account.MarkEventsAsCommitted();

            // Return success result
            var result = new OpenAccountResult(account.Id, account.CustomerId, account.Balance);

            return new CommandSuccess<OpenAccountResult>(result);
        }
        catch (Exception ex)
        {
            return new CommandFailure($"Failed to open account: {ex.Message}");
        }
    }
}
