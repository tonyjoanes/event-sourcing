using Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.ReadModels.Projections;

public class EventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public EventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(BaseEvent @event)
    {
        // Get all projection handlers
        var accountSummaryHandler = _serviceProvider.GetRequiredService<AccountSummaryProjectionHandler>();
        var transactionHistoryHandler = _serviceProvider.GetRequiredService<TransactionHistoryProjectionHandler>();

        // Route events to appropriate handlers
        switch (@event)
        {
            case AccountOpened accountOpened:
                await accountSummaryHandler.HandleAsync(accountOpened);
                break;

            case MoneyDeposited moneyDeposited:
                await accountSummaryHandler.HandleAsync(moneyDeposited);
                await transactionHistoryHandler.HandleAsync(moneyDeposited);
                break;

            case MoneyWithdrawn moneyWithdrawn:
                await accountSummaryHandler.HandleAsync(moneyWithdrawn);
                await transactionHistoryHandler.HandleAsync(moneyWithdrawn);
                break;

            case MoneyTransferred moneyTransferred:
                await accountSummaryHandler.HandleAsync(moneyTransferred);
                await transactionHistoryHandler.HandleAsync(moneyTransferred);
                break;

            case AccountFrozen accountFrozen:
                await accountSummaryHandler.HandleAsync(accountFrozen);
                break;

            case AccountUnfrozen accountUnfrozen:
                await accountSummaryHandler.HandleAsync(accountUnfrozen);
                break;

            case AccountClosed accountClosed:
                await accountSummaryHandler.HandleAsync(accountClosed);
                break;

            case OverdraftFeeCharged overdraftFeeCharged:
                await accountSummaryHandler.HandleAsync(overdraftFeeCharged);
                await transactionHistoryHandler.HandleAsync(overdraftFeeCharged);
                break;

            case InterestAccrued interestAccrued:
                await accountSummaryHandler.HandleAsync(interestAccrued);
                await transactionHistoryHandler.HandleAsync(interestAccrued);
                break;

            case AccountLimitsUpdated accountLimitsUpdated:
                await accountSummaryHandler.HandleAsync(accountLimitsUpdated);
                break;

            case ComplianceViolationDetected complianceViolationDetected:
                await transactionHistoryHandler.HandleAsync(complianceViolationDetected);
                break;

            case DailyWithdrawalLimitExceeded dailyLimitExceeded:
                await transactionHistoryHandler.HandleAsync(dailyLimitExceeded);
                break;

            case TransactionReversed transactionReversed:
                await transactionHistoryHandler.HandleAsync(transactionReversed);
                break;

            default:
                // Log unknown event type
                Console.WriteLine($"Unknown event type: {@event.GetType().Name}");
                break;
        }
    }
} 