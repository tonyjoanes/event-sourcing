using Application.Commands;
using Domain.Aggregates;
using Domain.ValueObjects;
using Infrastructure.ReadModels.Projections;
using Infrastructure.Repositories;

namespace Application.Handlers;

public class TransferCommandHandler
{
    private readonly AccountRepository _accountRepository;
    private readonly EventDispatcher _eventDispatcher;

    public TransferCommandHandler(
        AccountRepository accountRepository,
        EventDispatcher eventDispatcher
    )
    {
        _accountRepository = accountRepository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<TransferCommandResult> HandleAsync(TransferCommand command)
    {
        try
        {
            // Load source account from event store
            var fromAccount = await _accountRepository.GetByIdAsync(command.FromAccountId);
            if (fromAccount == null)
            {
                return new CommandFailure($"Source account {command.FromAccountId} not found");
            }

            // Load destination account from event store
            var toAccount = await _accountRepository.GetByIdAsync(command.ToAccountId);
            if (toAccount == null)
            {
                return new CommandFailure($"Destination account {command.ToAccountId} not found");
            }

            // Perform transfer using domain logic
            var descriptionOption = command.Description is not null
                ? Option<string>.Some(command.Description)
                : Option<string>.None;
            var transferResult = fromAccount.Transfer(
                command.Amount,
                command.ToAccountId,
                descriptionOption
            );

            if (transferResult.IsT3) // ValidationError
            {
                return new CommandFailure(transferResult.AsT3.Message);
            }

            if (transferResult.IsT2) // AccountFrozen
            {
                return new CommandFailure("Cannot transfer from frozen account");
            }

            if (transferResult.IsT1) // InsufficientFunds
            {
                return new CommandFailure("Insufficient funds for transfer");
            }

            // Collect events before saving (since SaveAsync clears them)
            var fromAccountEvents = fromAccount.UncommittedEvents.ToList();

            // Save source account to event store
            await _accountRepository.SaveAsync(fromAccount);

            // Publish events to update read models
            foreach (var @event in fromAccountEvents)
            {
                await _eventDispatcher.DispatchAsync(@event);
            }

            // Return success result
            var result = new TransferResult(
                command.FromAccountId,
                command.ToAccountId,
                command.Amount
            );

            return new CommandSuccess<TransferResult>(result);
        }
        catch (Exception ex)
        {
            return new CommandFailure($"Failed to transfer: {ex.Message}");
        }
    }
}
