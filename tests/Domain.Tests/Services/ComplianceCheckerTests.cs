using Domain.Services;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Services;

public class ComplianceCheckerTests
{
    private readonly ComplianceChecker _checker;

    public ComplianceCheckerTests()
    {
        _checker = ComplianceChecker.Standard();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithValidParameters_ShouldCreateComplianceChecker()
    {
        // Arrange
        var suspiciousThreshold = new Money(10000m, "USD");
        var largeTransactionThreshold = new Money(5000m, "USD");
        var maxDailyTransactions = 50;

        // Act
        var checker = new ComplianceChecker(
            suspiciousThreshold,
            largeTransactionThreshold,
            maxDailyTransactions
        );

        // Assert
        checker.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Standard_ShouldCreateStandardComplianceChecker()
    {
        // Act
        var checker = ComplianceChecker.Standard();

        // Assert
        checker.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Standard_WithCurrency_ShouldCreateStandardComplianceChecker()
    {
        // Act
        var checker = ComplianceChecker.Standard("EUR");

        // Assert
        checker.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CheckTransaction_WithValidAmount_ShouldReturnComplianceApproved()
    {
        // Arrange
        var amount = new Money(1000m, "USD");
        var transactionType = TransactionType.Deposit;
        var dailyTransactionCount = 5;
        var dailyTransactionTotal = new Money(2000m, "USD");

        // Act
        var result = _checker.CheckTransaction(
            amount,
            transactionType,
            dailyTransactionCount,
            dailyTransactionTotal
        );

        // Assert
        result.IsT0.Should().BeTrue();
        result.AsT0.Should().BeOfType<ComplianceApproved>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CheckTransaction_WithSuspiciousActivityAmount_ShouldReturnComplianceViolation()
    {
        // Arrange
        var amount = new Money(15000m, "USD"); // Above 10,000 threshold
        var transactionType = TransactionType.Deposit;
        var dailyTransactionCount = 5;
        var dailyTransactionTotal = new Money(2000m, "USD");

        // Act
        var result = _checker.CheckTransaction(
            amount,
            transactionType,
            dailyTransactionCount,
            dailyTransactionTotal
        );

        // Assert
        result.IsT1.Should().BeTrue();
        var violation = result.AsT1;
        violation.Type.Should().Be(ComplianceViolationType.SuspiciousActivity);
        violation.Reason.Should().Contain("exceeds suspicious activity threshold");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CheckTransaction_WithLargeTransactionAmount_ShouldReturnComplianceViolation()
    {
        // Arrange
        var amount = new Money(7500m, "USD"); // Above 5,000 threshold
        var transactionType = TransactionType.Deposit;
        var dailyTransactionCount = 5;
        var dailyTransactionTotal = new Money(2000m, "USD");

        // Act
        var result = _checker.CheckTransaction(
            amount,
            transactionType,
            dailyTransactionCount,
            dailyTransactionTotal
        );

        // Assert
        result.IsT1.Should().BeTrue();
        var violation = result.AsT1;
        violation.Type.Should().Be(ComplianceViolationType.LargeTransaction);
        violation.Reason.Should().Contain("requires additional reporting");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CheckTransaction_WithExceededDailyLimit_ShouldReturnComplianceViolation()
    {
        // Arrange
        var amount = new Money(1000m, "USD");
        var transactionType = TransactionType.Deposit;
        var dailyTransactionCount = 50; // At the limit
        var dailyTransactionTotal = new Money(2000m, "USD");

        // Act
        var result = _checker.CheckTransaction(
            amount,
            transactionType,
            dailyTransactionCount,
            dailyTransactionTotal
        );

        // Assert
        result.IsT1.Should().BeTrue();
        var violation = result.AsT1;
        violation.Type.Should().Be(ComplianceViolationType.DailyLimitExceeded);
        violation.Reason.Should().Contain("Daily transaction count limit");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CheckTransaction_WithUnusualWithdrawalPattern_ShouldReturnComplianceViolation()
    {
        // Arrange
        var amount = new Money(3000m, "USD"); // Below large transaction threshold
        var transactionType = TransactionType.Withdrawal;
        var dailyTransactionCount = 5;
        var dailyTransactionTotal = new Money(8000m, "USD"); // Total would be 11,000 > 10,000

        // Act
        var result = _checker.CheckTransaction(
            amount,
            transactionType,
            dailyTransactionCount,
            dailyTransactionTotal
        );

        // Assert
        result.IsT1.Should().BeTrue();
        var violation = result.AsT1;
        violation.Type.Should().Be(ComplianceViolationType.UnusualActivity);
        violation.Reason.Should().Contain("Daily withdrawal total");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CheckAccountOpening_WithValidInitialBalance_ShouldReturnComplianceApproved()
    {
        // Arrange
        var customerId = CustomerId.Generate();
        var initialBalance = new Money(5000m, "USD");
        var openedAt = DateTimeOffset.UtcNow;

        // Act
        var result = _checker.CheckAccountOpening(customerId, initialBalance, openedAt);

        // Assert
        result.IsT0.Should().BeTrue();
        result.AsT0.Should().BeOfType<ComplianceApproved>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CheckAccountOpening_WithSuspiciousInitialBalance_ShouldReturnComplianceViolation()
    {
        // Arrange
        var customerId = CustomerId.Generate();
        var initialBalance = new Money(15000m, "USD"); // Above 10,000 threshold
        var openedAt = DateTimeOffset.UtcNow;

        // Act
        var result = _checker.CheckAccountOpening(customerId, initialBalance, openedAt);

        // Assert
        result.IsT1.Should().BeTrue();
        var violation = result.AsT1;
        violation.Type.Should().Be(ComplianceViolationType.SuspiciousActivity);
        violation.Reason.Should().Contain("Initial deposit");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CheckTransfer_WithValidTransfer_ShouldReturnComplianceApproved()
    {
        // Arrange
        var fromAccountId = AccountId.Generate();
        var toAccountId = AccountId.Generate();
        var amount = new Money(1000m, "USD");
        var transferredAt = DateTimeOffset.UtcNow;

        // Act
        var result = _checker.CheckTransfer(fromAccountId, toAccountId, amount, transferredAt);

        // Assert
        result.IsT0.Should().BeTrue();
        result.AsT0.Should().BeOfType<ComplianceApproved>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CheckTransfer_WithSelfTransfer_ShouldReturnComplianceViolation()
    {
        // Arrange
        var accountId = AccountId.Generate();
        var amount = new Money(1000m, "USD");
        var transferredAt = DateTimeOffset.UtcNow;

        // Act
        var result = _checker.CheckTransfer(accountId, accountId, amount, transferredAt);

        // Assert
        result.IsT1.Should().BeTrue();
        var violation = result.AsT1;
        violation.Type.Should().Be(ComplianceViolationType.SuspiciousActivity);
        violation.Reason.Should().Contain("Self-transfer detected");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RequiresEnhancedDueDiligence_WithLargeTransaction_ShouldReturnTrue()
    {
        // Arrange
        var amount = new Money(7500m, "USD"); // Above 5,000 threshold
        var transactionType = TransactionType.Deposit;

        // Act
        var result = _checker.RequiresEnhancedDueDiligence(amount, transactionType);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RequiresEnhancedDueDiligence_WithTransferTransaction_ShouldReturnTrue()
    {
        // Arrange
        var amount = new Money(1000m, "USD");
        var transactionType = TransactionType.Transfer;

        // Act
        var result = _checker.RequiresEnhancedDueDiligence(amount, transactionType);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RequiresEnhancedDueDiligence_WithSmallDeposit_ShouldReturnFalse()
    {
        // Arrange
        var amount = new Money(1000m, "USD");
        var transactionType = TransactionType.Deposit;

        // Act
        var result = _checker.RequiresEnhancedDueDiligence(amount, transactionType);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(TransactionType.Deposit)]
    [InlineData(TransactionType.Withdrawal)]
    [InlineData(TransactionType.OverdraftFee)]
    [InlineData(TransactionType.InterestAccrual)]
    [InlineData(TransactionType.ServiceCharge)]
    [InlineData(TransactionType.Reversal)]
    [InlineData(TransactionType.Adjustment)]
    [Trait("Category", "Unit")]
    public void CheckTransaction_WithDifferentTransactionTypes_ShouldHandleCorrectly(
        TransactionType transactionType
    )
    {
        // Arrange
        var amount = new Money(1000m, "USD");
        var dailyTransactionCount = 5;
        var dailyTransactionTotal = new Money(2000m, "USD");

        // Act
        var result = _checker.CheckTransaction(
            amount,
            transactionType,
            dailyTransactionCount,
            dailyTransactionTotal
        );

        // Assert
        result.Should().NotBeNull();
        // Should either be approved or have a specific violation reason
        if (result.IsT1)
        {
            result.AsT1.Reason.Should().NotBeNullOrEmpty();
        }
    }
}
