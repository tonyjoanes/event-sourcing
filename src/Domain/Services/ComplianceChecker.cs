using Domain.ValueObjects;
using OneOf;

namespace Domain.Services;

public class ComplianceChecker
{
    private readonly Money _suspiciousActivityThreshold;
    private readonly Money _largeTransactionThreshold;
    private readonly int _maxDailyTransactions;

    public ComplianceChecker(
        Money suspiciousActivityThreshold,
        Money largeTransactionThreshold,
        int maxDailyTransactions = 50
    )
    {
        _suspiciousActivityThreshold = suspiciousActivityThreshold;
        _largeTransactionThreshold = largeTransactionThreshold;
        _maxDailyTransactions = maxDailyTransactions;
    }

    public static ComplianceChecker Standard(string currency = "USD")
    {
        return new ComplianceChecker(
            suspiciousActivityThreshold: new Money(10000m, currency),
            largeTransactionThreshold: new Money(5000m, currency)
        );
    }

    public OneOf<ComplianceApproved, ComplianceViolation> CheckTransaction(
        Money amount,
        TransactionType transactionType,
        int dailyTransactionCount,
        Money dailyTransactionTotal
    )
    {
        // Check for suspicious activity threshold
        if (amount >= _suspiciousActivityThreshold)
        {
            return new ComplianceViolation(
                ComplianceViolationType.SuspiciousActivity,
                $"Transaction amount {amount} exceeds suspicious activity threshold {_suspiciousActivityThreshold}"
            );
        }

        // Check for large transaction reporting
        if (amount >= _largeTransactionThreshold)
        {
            return new ComplianceViolation(
                ComplianceViolationType.LargeTransaction,
                $"Large transaction {amount} requires additional reporting"
            );
        }

        // Check daily transaction count limit
        if (dailyTransactionCount >= _maxDailyTransactions)
        {
            return new ComplianceViolation(
                ComplianceViolationType.DailyLimitExceeded,
                $"Daily transaction count limit of {_maxDailyTransactions} exceeded"
            );
        }

        // Check for unusual withdrawal patterns
        if (transactionType == TransactionType.Withdrawal && 
            dailyTransactionTotal + amount > _suspiciousActivityThreshold)
        {
            return new ComplianceViolation(
                ComplianceViolationType.UnusualActivity,
                $"Daily withdrawal total {dailyTransactionTotal + amount} exceeds threshold"
            );
        }

        return new ComplianceApproved();
    }

    public OneOf<ComplianceApproved, ComplianceViolation> CheckAccountOpening(
        CustomerId customerId,
        Money initialBalance,
        DateTimeOffset openedAt
    )
    {
        // Check for unusually large initial deposits
        if (initialBalance >= _suspiciousActivityThreshold)
        {
            return new ComplianceViolation(
                ComplianceViolationType.SuspiciousActivity,
                $"Initial deposit {initialBalance} exceeds suspicious activity threshold"
            );
        }

        // Check for multiple account openings in short time
        // This would typically check against a customer service or external system
        // For now, we'll just approve

        return new ComplianceApproved();
    }

    public OneOf<ComplianceApproved, ComplianceViolation> CheckTransfer(
        AccountId fromAccountId,
        AccountId toAccountId,
        Money amount,
        DateTimeOffset transferredAt
    )
    {
        // Check for self-transfers (potential money laundering)
        if (fromAccountId == toAccountId)
        {
            return new ComplianceViolation(
                ComplianceViolationType.SuspiciousActivity,
                "Self-transfer detected"
            );
        }

        // Check for rapid transfers between accounts
        // This would typically check against a transaction history service
        // For now, we'll just approve

        return new ComplianceApproved();
    }

    public bool RequiresEnhancedDueDiligence(Money amount, TransactionType transactionType)
    {
        return amount >= _largeTransactionThreshold || 
               transactionType == TransactionType.Transfer;
    }
}

// Compliance result types
public record ComplianceApproved;

public record ComplianceViolation(ComplianceViolationType Type, string Reason);

public enum ComplianceViolationType
{
    SuspiciousActivity,
    LargeTransaction,
    DailyLimitExceeded,
    UnusualActivity,
    MoneyLaundering,
    TerroristFinancing,
    SanctionsViolation
} 