using Application.Commands;
using Domain.Aggregates;
using Domain.ValueObjects;
using Infrastructure.ReadModels.Projections;
using Infrastructure.Repositories;

namespace Application.Handlers;

public class DepositCommandHandler
{
    private readonly AccountRepository _accountRepository;
    private readonly EventDispatcher _eventDispatcher;

    public DepositCommandHandler(
        AccountRepository accountRepository,
        EventDispatcher eventDispatcher
    )
    {
        _accountRepository = accountRepository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<DepositCommandResult> HandleAsync(DepositCommand command)
    {
        try
        {
            // Load account from event store
            var account = await _accountRepository.GetByIdAsync(command.AccountId);
            if (account == null)
            {
                return new CommandFailure($"Account {command.AccountId} not found");
            }

            // Perform deposit using domain logic
            var descriptionOption = command.Description is not null
                ? Option<string>.Some(command.Description)
                : Option<string>.None;
            var depositResult = account.Deposit(command.Amount, descriptionOption);

            if (depositResult.IsT3) // ValidationError
            {
                return new CommandFailure(depositResult.AsT3.Message);
            }

            if (depositResult.IsT2) // AccountFrozen
            {
                return new CommandFailure("Cannot deposit to frozen account");
            }

            // Save to event store
            await _accountRepository.SaveAsync(account);

            // Publish events to update read models
            foreach (var @event in account.UncommittedEvents)
            {
                await _eventDispatcher.DispatchAsync(@event);
            }

            account.MarkEventsAsCommitted();

            // Return success result
            var result = new DepositResult(account.Id, command.Amount, account.Balance);

            return new CommandSuccess<DepositResult>(result);
        }
        catch (Exception ex)
        {
            return new CommandFailure($"Failed to deposit: {ex.Message}");
        }
    }
}
