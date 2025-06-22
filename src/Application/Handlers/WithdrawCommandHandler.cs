using Application.Commands;
using Domain.Aggregates;
using Domain.ValueObjects;
using Infrastructure.ReadModels.Projections;
using Infrastructure.Repositories;

namespace Application.Handlers;

public class WithdrawCommandHandler
{
    private readonly AccountRepository _accountRepository;
    private readonly EventDispatcher _eventDispatcher;

    public WithdrawCommandHandler(
        AccountRepository accountRepository,
        EventDispatcher eventDispatcher
    )
    {
        _accountRepository = accountRepository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<WithdrawCommandResult> HandleAsync(WithdrawCommand command)
    {
        try
        {
            // Load account from event store
            var account = await _accountRepository.GetByIdAsync(command.AccountId);
            if (account == null)
            {
                return new CommandFailure($"Account {command.AccountId} not found");
            }

            // Perform withdrawal using domain logic
            var descriptionOption = command.Description is not null
                ? Option<string>.Some(command.Description)
                : Option<string>.None;
            var withdrawResult = account.Withdraw(command.Amount, descriptionOption);

            if (withdrawResult.IsT3) // ValidationError
            {
                return new CommandFailure(withdrawResult.AsT3.Message);
            }

            if (withdrawResult.IsT2) // AccountFrozen
            {
                return new CommandFailure("Cannot withdraw from frozen account");
            }

            if (withdrawResult.IsT1) // InsufficientFunds
            {
                return new CommandFailure("Insufficient funds for withdrawal");
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
            var result = new WithdrawResult(account.Id, command.Amount, account.Balance);

            return new CommandSuccess<WithdrawResult>(result);
        }
        catch (Exception ex)
        {
            return new CommandFailure($"Failed to withdraw: {ex.Message}");
        }
    }
}
