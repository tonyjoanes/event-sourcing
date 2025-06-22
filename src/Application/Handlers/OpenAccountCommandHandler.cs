using Application.Commands;
using Domain.Aggregates;
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

            // Collect events before saving (since SaveAsync clears them)
            var eventsToDispatch = account.UncommittedEvents.ToList();

            // Save to event store
            await _accountRepository.SaveAsync(account);

            // Publish events to update read models
            Console.WriteLine(
                $"OpenAccountCommandHandler: About to dispatch {eventsToDispatch.Count} events"
            );
            foreach (var @event in eventsToDispatch)
            {
                Console.WriteLine(
                    $"OpenAccountCommandHandler: Dispatching event {@event.GetType().Name} for aggregate {@event.AggregateId}"
                );
                await _eventDispatcher.DispatchAsync(@event);
            }

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
