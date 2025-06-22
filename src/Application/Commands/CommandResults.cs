using Domain.ValueObjects;
using OneOf;

namespace Application.Commands;

// Command result types
public record CommandSuccess<T>(T Data)
    where T : class;

public record CommandFailure(string Reason);

// Specific command results
public record OpenAccountResult(AccountId AccountId, CustomerId CustomerId, Money InitialBalance);

public record DepositResult(AccountId AccountId, Money Amount, Money NewBalance);

public record WithdrawResult(AccountId AccountId, Money Amount, Money NewBalance);

public record TransferResult(AccountId FromAccountId, AccountId ToAccountId, Money Amount);

// Command result unions using OneOf
public class OpenAccountCommandResult : OneOfBase<CommandSuccess<OpenAccountResult>, CommandFailure>
{
    public OpenAccountCommandResult(OneOf<CommandSuccess<OpenAccountResult>, CommandFailure> input)
        : base(input) { }

    public static implicit operator OpenAccountCommandResult(
        CommandSuccess<OpenAccountResult> success
    ) => new(success);

    public static implicit operator OpenAccountCommandResult(CommandFailure failure) =>
        new(failure);
}

public class DepositCommandResult : OneOfBase<CommandSuccess<DepositResult>, CommandFailure>
{
    public DepositCommandResult(OneOf<CommandSuccess<DepositResult>, CommandFailure> input)
        : base(input) { }

    public static implicit operator DepositCommandResult(CommandSuccess<DepositResult> success) =>
        new(success);

    public static implicit operator DepositCommandResult(CommandFailure failure) => new(failure);
}

public class WithdrawCommandResult : OneOfBase<CommandSuccess<WithdrawResult>, CommandFailure>
{
    public WithdrawCommandResult(OneOf<CommandSuccess<WithdrawResult>, CommandFailure> input)
        : base(input) { }

    public static implicit operator WithdrawCommandResult(CommandSuccess<WithdrawResult> success) =>
        new(success);

    public static implicit operator WithdrawCommandResult(CommandFailure failure) => new(failure);
}

public class TransferCommandResult : OneOfBase<CommandSuccess<TransferResult>, CommandFailure>
{
    public TransferCommandResult(OneOf<CommandSuccess<TransferResult>, CommandFailure> input)
        : base(input) { }

    public static implicit operator TransferCommandResult(CommandSuccess<TransferResult> success) =>
        new(success);

    public static implicit operator TransferCommandResult(CommandFailure failure) => new(failure);
}
