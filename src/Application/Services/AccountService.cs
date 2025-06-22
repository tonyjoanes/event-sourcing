using Application.Commands;
using Application.Handlers;
using Domain.ValueObjects;

namespace Application.Services;

public class AccountService
{
    private readonly OpenAccountCommandHandler _openAccountHandler;
    private readonly DepositCommandHandler _depositHandler;
    private readonly WithdrawCommandHandler _withdrawHandler;
    private readonly TransferCommandHandler _transferHandler;

    public AccountService(
        OpenAccountCommandHandler openAccountHandler,
        DepositCommandHandler depositHandler,
        WithdrawCommandHandler withdrawHandler,
        TransferCommandHandler transferHandler
    )
    {
        _openAccountHandler = openAccountHandler;
        _depositHandler = depositHandler;
        _withdrawHandler = withdrawHandler;
        _transferHandler = transferHandler;
    }

    public async Task<OpenAccountCommandResult> OpenAccountAsync(
        CustomerId customerId,
        Money initialBalance
    )
    {
        var command = new OpenAccountCommand
        {
            CustomerId = customerId,
            InitialBalance = initialBalance,
        };

        return await _openAccountHandler.HandleAsync(command);
    }

    public async Task<DepositCommandResult> DepositAsync(
        AccountId accountId,
        Money amount,
        string? description = null
    )
    {
        var command = new DepositCommand
        {
            AccountId = accountId,
            Amount = amount,
            Description = description,
        };

        return await _depositHandler.HandleAsync(command);
    }

    public async Task<WithdrawCommandResult> WithdrawAsync(
        AccountId accountId,
        Money amount,
        string? description = null
    )
    {
        var command = new WithdrawCommand
        {
            AccountId = accountId,
            Amount = amount,
            Description = description,
        };

        return await _withdrawHandler.HandleAsync(command);
    }

    public async Task<TransferCommandResult> TransferAsync(
        AccountId fromAccountId,
        AccountId toAccountId,
        Money amount,
        string? description = null
    )
    {
        var command = new TransferCommand
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount,
            Description = description,
        };

        return await _transferHandler.HandleAsync(command);
    }
}
