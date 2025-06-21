using Domain.Aggregates;
using Domain.Events;
using Domain.ValueObjects;
using FluentAssertions;
using OneOf;

namespace Domain.Tests.Aggregates;

public class AccountTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Open_WithValidParameters_ShouldCreateAccount()
    {
        // Arrange
        var customerId = CustomerId.Generate();
        var initialBalance = new Money(1000m);

        // Act
        var result = Account.Open(customerId, initialBalance);

        // Assert
        result.IsT0.Should().BeTrue();
        var account = result.AsT0;
        
        account.Id.Should().NotBeNull();
        account.CustomerId.Should().Be(customerId);
        account.Balance.Should().Be(initialBalance);
        account.Status.Should().Be(AccountStatus.Active);
        account.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        
        account.UncommittedEvents.Should().HaveCount(1);
        account.UncommittedEvents.First().Should().BeOfType<AccountOpened>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Open_WithNegativeBalance_ShouldReturnValidationError()
    {
        var result = Account.Open(CustomerId.Generate(), new Money(-100, "USD"));
        result.IsT1.Should().BeTrue();
        result.AsT1.Should().BeOfType<ValidationError>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Deposit_WithValidAmount_ShouldIncreaseBalance()
    {
        // Arrange
        var account = CreateTestAccount(1000m);

        // Act
        var result = account.Deposit(new Money(500m), Option<string>.Some("Salary"));

        // Assert
        result.IsT0.Should().BeTrue();
        account.Balance.Should().Be(new Money(1500m));
        account.LastTransactionAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        
        account.UncommittedEvents.Should().HaveCount(2); // AccountOpened + MoneyDeposited
        var depositEvent = account.UncommittedEvents.Last().Should().BeOfType<MoneyDeposited>().Subject;
        depositEvent.Amount.Should().Be(new Money(500m));
        depositEvent.Description.Should().Be("Salary");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Deposit_WithZeroAmount_ShouldReturnValidationError()
    {
        // Arrange
        var account = CreateTestAccount(1000m);

        // Act
        var result = account.Deposit(new Money(0m));

        // Assert
        result.IsT3.Should().BeTrue();
        var error = result.AsT3;
        error.Message.Should().Be("Deposit amount must be positive");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Deposit_WithNegativeAmount_ShouldReturnValidationError()
    {
        var account = Account.Open(CustomerId.Generate(), new Money(100, "USD")).AsT0;
        var result = account.Deposit(new Money(-50, "USD"));
        result.IsT3.Should().BeTrue();
        result.AsT3.Should().BeOfType<ValidationError>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Deposit_WithFrozenAccount_ShouldReturnAccountFrozenError()
    {
        // Arrange
        var account = CreateTestAccount(1000m);
        account.Freeze(Option<string>.Some("Suspicious activity"));

        // Act
        var result = account.Deposit(new Money(500m));

        // Assert
        result.IsT2.Should().BeTrue();
        var error = result.AsT2;
        error.Message.Should().Contain("Account").And.Contain("Frozen");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Withdraw_WithValidAmount_ShouldDecreaseBalance()
    {
        // Arrange
        var account = CreateTestAccount(1000m);

        // Act
        var result = account.Withdraw(new Money(300m), Option<string>.Some("ATM withdrawal"));

        // Assert
        result.IsT0.Should().BeTrue();
        account.Balance.Should().Be(new Money(700m));
        account.LastTransactionAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        
        account.UncommittedEvents.Should().HaveCount(2);
        var withdrawEvent = account.UncommittedEvents.Last().Should().BeOfType<MoneyWithdrawn>().Subject;
        withdrawEvent.Amount.Should().Be(new Money(300m));
        withdrawEvent.Description.Should().Be("ATM withdrawal");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Withdraw_WithInsufficientFunds_ShouldReturnInsufficientFundsError()
    {
        // Arrange
        var account = CreateTestAccount(1000m);

        // Act
        var result = account.Withdraw(new Money(1500m));

        // Assert
        result.IsT1.Should().BeTrue();
        var error = result.AsT1;
        error.Message.Should().Contain("Insufficient funds");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Transfer_WithValidAmount_ShouldDecreaseBalance()
    {
        // Arrange
        var account = CreateTestAccount(1000m);
        var toAccountId = AccountId.Generate();

        // Act
        var result = account.Transfer(new Money(400m), toAccountId, Option<string>.Some("Rent payment"));

        // Assert
        result.IsT0.Should().BeTrue();
        account.Balance.Should().Be(new Money(600m));
        
        account.UncommittedEvents.Should().HaveCount(2);
        var transferEvent = account.UncommittedEvents.Last().Should().BeOfType<MoneyTransferred>().Subject;
        transferEvent.Amount.Should().Be(new Money(400m));
        transferEvent.ToAccountId.Should().Be(toAccountId);
        transferEvent.Description.Should().Be("Rent payment");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Transfer_WithInsufficientFunds_ShouldReturnInsufficientFundsError()
    {
        // Arrange
        var account = CreateTestAccount(1000m);
        var toAccountId = AccountId.Generate();

        // Act
        var result = account.Transfer(new Money(1200m), toAccountId);

        // Assert
        result.IsT1.Should().BeTrue();
        var error = result.AsT1;
        error.Message.Should().Contain("Insufficient funds for transfer");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Freeze_WithValidReason_ShouldChangeStatusToFrozen()
    {
        // Arrange
        var account = CreateTestAccount(1000m);

        // Act
        var result = account.Freeze(Option<string>.Some("Suspicious activity"));

        // Assert
        result.IsT0.Should().BeTrue();
        account.Status.Should().Be(AccountStatus.Frozen);
        
        account.UncommittedEvents.Should().HaveCount(2);
        var freezeEvent = account.UncommittedEvents.Last().Should().BeOfType<AccountFrozen>().Subject;
        freezeEvent.Reason.Should().Be("Suspicious activity");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Freeze_AlreadyFrozenAccount_ShouldReturnValidationError()
    {
        // Arrange
        var account = CreateTestAccount(1000m);
        account.Freeze(Option<string>.Some("First freeze"));

        // Act
        var result = account.Freeze(Option<string>.Some("Second freeze"));

        // Assert
        result.IsT1.Should().BeTrue();
        var error = result.AsT1;
        error.Message.Should().Be("Account is already frozen");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Unfreeze_FrozenAccount_ShouldChangeStatusToActive()
    {
        // Arrange
        var account = CreateTestAccount(1000m);
        account.Freeze(Option<string>.Some("Suspicious activity"));

        // Act
        var result = account.Unfreeze();

        // Assert
        result.IsT0.Should().BeTrue();
        account.Status.Should().Be(AccountStatus.Active);
        
        account.UncommittedEvents.Should().HaveCount(3);
        account.UncommittedEvents.Last().Should().BeOfType<AccountUnfrozen>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Unfreeze_ActiveAccount_ShouldReturnValidationError()
    {
        // Arrange
        var account = CreateTestAccount(1000m);

        // Act
        var result = account.Unfreeze();

        // Assert
        result.IsT1.Should().BeTrue();
        var error = result.AsT1;
        error.Message.Should().Be("Account is not frozen");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Close_WithValidReason_ShouldChangeStatusToClosed()
    {
        // Arrange
        var account = CreateTestAccount(1000m);

        // Act
        var result = account.Close(Option<string>.Some("Customer request"));

        // Assert
        result.IsT0.Should().BeTrue();
        account.Status.Should().Be(AccountStatus.Closed);
        
        account.UncommittedEvents.Should().HaveCount(2);
        var closeEvent = account.UncommittedEvents.Last().Should().BeOfType<AccountClosed>().Subject;
        closeEvent.Reason.Should().Be("Customer request");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Close_AlreadyClosedAccount_ShouldReturnValidationError()
    {
        // Arrange
        var account = CreateTestAccount(1000m);
        account.Close(Option<string>.Some("First close"));

        // Act
        var result = account.Close(Option<string>.Some("Second close"));

        // Assert
        result.IsT1.Should().BeTrue();
        var error = result.AsT1;
        error.Message.Should().Be("Account is already closed");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ChargeOverdraftFee_ShouldDecreaseBalance()
    {
        // Arrange
        var account = CreateTestAccount(1000m);

        // Act
        account.ChargeOverdraftFee(new Money(25m));

        // Assert
        account.Balance.Should().Be(new Money(975m));
        
        account.UncommittedEvents.Should().HaveCount(2);
        var feeEvent = account.UncommittedEvents.Last().Should().BeOfType<OverdraftFeeCharged>().Subject;
        feeEvent.FeeAmount.Should().Be(new Money(25m));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AccrueInterest_ShouldIncreaseBalance()
    {
        // Arrange
        var account = CreateTestAccount(1000m);

        // Act
        account.AccrueInterest(new Money(5.50m));

        // Assert
        account.Balance.Should().Be(new Money(1005.50m));
        
        account.UncommittedEvents.Should().HaveCount(2);
        var interestEvent = account.UncommittedEvents.Last().Should().BeOfType<InterestAccrued>().Subject;
        interestEvent.InterestAmount.Should().Be(new Money(5.50m));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void LoadFromHistory_ShouldReconstructAccountState()
    {
        // Arrange
        var customerId = CustomerId.Generate();
        var accountId = AccountId.Generate();
        var events = new List<BaseEvent>
        {
            new AccountOpened
            {
                AccountId = accountId,
                CustomerId = customerId,
                InitialBalance = new Money(1000m),
                OpenedAt = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new MoneyDeposited
            {
                AccountId = accountId,
                Amount = new Money(500m),
                Description = "Salary",
                DepositedAt = DateTimeOffset.UtcNow.AddHours(-12)
            },
            new MoneyWithdrawn
            {
                AccountId = accountId,
                Amount = new Money(200m),
                Description = "ATM",
                WithdrawnAt = DateTimeOffset.UtcNow.AddHours(-6)
            }
        };

        var account = new Account();

        // Act
        account.LoadFromHistory(events);

        // Assert
        account.Id.Should().Be(accountId);
        account.CustomerId.Should().Be(customerId);
        account.Balance.Should().Be(new Money(1300m));
        account.Status.Should().Be(AccountStatus.Active);
        account.Version.Should().Be(3);
        account.UncommittedEvents.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Deposit_WithoutDescription_ShouldUseDefaultDescription()
    {
        // Arrange
        var account = CreateTestAccount(1000m);

        // Act
        var result = account.Deposit(new Money(500m));

        // Assert
        result.IsT0.Should().BeTrue();
        var depositEvent = account.UncommittedEvents.Last().Should().BeOfType<MoneyDeposited>().Subject;
        depositEvent.Description.Should().Be("Deposit");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Withdraw_WithoutDescription_ShouldUseDefaultDescription()
    {
        // Arrange
        var account = CreateTestAccount(1000m);

        // Act
        var result = account.Withdraw(new Money(300m));

        // Assert
        result.IsT0.Should().BeTrue();
        var withdrawEvent = account.UncommittedEvents.Last().Should().BeOfType<MoneyWithdrawn>().Subject;
        withdrawEvent.Description.Should().Be("Withdrawal");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Transfer_WithoutDescription_ShouldUseDefaultDescription()
    {
        // Arrange
        var account = CreateTestAccount(1000m);
        var toAccountId = AccountId.Generate();

        // Act
        var result = account.Transfer(new Money(400m), toAccountId);

        // Assert
        result.IsT0.Should().BeTrue();
        var transferEvent = account.UncommittedEvents.Last().Should().BeOfType<MoneyTransferred>().Subject;
        transferEvent.Description.Should().Be("Transfer");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Freeze_WithoutReason_ShouldUseDefaultReason()
    {
        // Arrange
        var account = CreateTestAccount(1000m);

        // Act
        var result = account.Freeze();

        // Assert
        result.IsT0.Should().BeTrue();
        var freezeEvent = account.UncommittedEvents.Last().Should().BeOfType<AccountFrozen>().Subject;
        freezeEvent.Reason.Should().Be("No reason provided");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Close_WithoutReason_ShouldUseDefaultReason()
    {
        // Arrange
        var account = CreateTestAccount(1000m);

        // Act
        var result = account.Close();

        // Assert
        result.IsT0.Should().BeTrue();
        var closeEvent = account.UncommittedEvents.Last().Should().BeOfType<AccountClosed>().Subject;
        closeEvent.Reason.Should().Be("No reason provided");
    }

    private static Account CreateTestAccount(decimal initialBalance)
    {
        var customerId = CustomerId.Generate();
        var result = Account.Open(customerId, new Money(initialBalance));
        return result.AsT0;
    }
}
