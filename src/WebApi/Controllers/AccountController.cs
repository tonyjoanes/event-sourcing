using System.ComponentModel.DataAnnotations;
using Application.Services;
using Domain.Aggregates;
using Domain.ValueObjects;
using Infrastructure.EventStore;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

/// <summary>
/// Event Sourcing Banking API - Account Management and Time Travel Operations
/// </summary>
/// <remarks>
/// This controller demonstrates advanced event sourcing patterns including:
/// - Traditional CRUD operations backed by event streams
/// - Time travel queries to view historical account states
/// - Audit trails and compliance reporting
/// - What-if scenario analysis
/// - Complete event stream inspection
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;
    private readonly QueryService _queryService;

    public AccountController(AccountService accountService, QueryService queryService)
    {
        _accountService = accountService;
        _queryService = queryService;
    }

    // Command endpoints

    /// <summary>
    /// Opens a new bank account with an initial balance
    /// </summary>
    /// <param name="request">Account creation details including customer ID and initial balance</param>
    /// <returns>The newly created account ID and confirmation message</returns>
    /// <response code="200">Account successfully created</response>
    /// <response code="400">Invalid request data or business rule violation</response>
    /// <example>
    /// POST /api/account/open
    /// {
    ///   "customerId": "CUST001",
    ///   "initialBalance": 1000.00,
    ///   "description": "Initial deposit"
    /// }
    /// </example>
    [HttpPost("open")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> OpenAccount([FromBody] OpenAccountRequest request)
    {
        var result = await _accountService.OpenAccountAsync(
            new CustomerId(request.CustomerId),
            Money.Create(request.InitialBalance, "USD")
        );

        return result.Match<IActionResult>(
            success =>
                Ok(
                    new
                    {
                        AccountId = success.Data.AccountId.Value,
                        Message = "Account opened successfully",
                    }
                ),
            failure => BadRequest(new { Error = failure.Reason })
        );
    }

    /// <summary>
    /// Deposits money into an existing account
    /// </summary>
    /// <param name="request">Deposit details including account ID, amount, and description</param>
    /// <returns>Updated account balance and confirmation message</returns>
    /// <response code="200">Deposit successful</response>
    /// <response code="400">Invalid request or account not found/frozen</response>
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
    {
        var result = await _accountService.DepositAsync(
            new AccountId(request.AccountId),
            Money.Create(request.Amount, "USD"),
            request.Description
        );

        return result.Match<IActionResult>(
            success =>
                Ok(
                    new
                    {
                        Message = "Deposit successful",
                        NewBalance = success.Data.NewBalance.Amount,
                    }
                ),
            failure => BadRequest(new { Error = failure.Reason })
        );
    }

    /// <summary>
    /// Withdraws money from an account
    /// </summary>
    /// <param name="request">Withdrawal details including account ID, amount, and description</param>
    /// <returns>Updated account balance and confirmation message</returns>
    /// <response code="200">Withdrawal successful</response>
    /// <response code="400">Insufficient funds, invalid request, or account frozen</response>
    [HttpPost("withdraw")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
    {
        var result = await _accountService.WithdrawAsync(
            new AccountId(request.AccountId),
            Money.Create(request.Amount, "USD"),
            request.Description
        );

        return result.Match<IActionResult>(
            success =>
                Ok(
                    new
                    {
                        Message = "Withdrawal successful",
                        NewBalance = success.Data.NewBalance.Amount,
                    }
                ),
            failure => BadRequest(new { Error = failure.Reason })
        );
    }

    /// <summary>
    /// Transfers money between two accounts
    /// </summary>
    /// <param name="request">Transfer details including source account, destination account, and amount</param>
    /// <returns>Transfer confirmation with updated account information</returns>
    /// <response code="200">Transfer successful</response>
    /// <response code="400">Insufficient funds, invalid accounts, or business rule violation</response>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        var result = await _accountService.TransferAsync(
            new AccountId(request.FromAccountId),
            new AccountId(request.ToAccountId),
            Money.Create(request.Amount, "USD"),
            request.Description
        );

        return result.Match<IActionResult>(
            success =>
                Ok(
                    new
                    {
                        Message = "Transfer successful",
                        FromAccountId = success.Data.FromAccountId.Value,
                        ToAccountId = success.Data.ToAccountId.Value,
                        Amount = success.Data.Amount.Amount,
                    }
                ),
            failure => BadRequest(new { Error = failure.Reason })
        );
    }

    // Query endpoints

    /// <summary>
    /// Get current account summary information
    /// </summary>
    /// <param name="accountId">The account identifier</param>
    /// <returns>Current account summary including balance, status, and metadata</returns>
    /// <response code="200">Account summary retrieved successfully</response>
    /// <response code="404">Account not found</response>
    [HttpGet("{accountId}/summary")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetAccountSummary(string accountId)
    {
        var result = await _queryService.GetAccountSummaryAsync(new AccountId(accountId));

        return result.Match<IActionResult>(
            success => Ok(success.Data),
            failure => NotFound(new { Error = failure.Reason })
        );
    }

    /// <summary>
    /// Get transaction history for an account
    /// </summary>
    /// <param name="accountId">The account identifier</param>
    /// <param name="last">Limit to the last N transactions</param>
    /// <param name="from">Start date for transaction history</param>
    /// <param name="to">End date for transaction history</param>
    /// <returns>Filtered transaction history</returns>
    /// <response code="200">Transaction history retrieved successfully</response>
    /// <response code="404">Account not found</response>
    [HttpGet("{accountId}/transactions")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetTransactionHistory(
        string accountId,
        [FromQuery] int? last = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null
    )
    {
        var result = await _queryService.GetTransactionHistoryAsync(
            new AccountId(accountId),
            last,
            from,
            to
        );

        return result.Match<IActionResult>(
            success => Ok(success.Data),
            failure => NotFound(new { Error = failure.Reason })
        );
    }

    /// <summary>
    /// Get account balance at a specific point in time
    /// </summary>
    /// <param name="accountId">The account identifier</param>
    /// <param name="date">The date to query balance for</param>
    /// <returns>Account balance as it existed on the specified date</returns>
    /// <response code="200">Balance retrieved successfully</response>
    /// <response code="404">Account not found or no transactions by that date</response>
    [HttpGet("{accountId}/balance-at")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetBalanceAt(string accountId, [FromQuery] DateTimeOffset date)
    {
        var result = await _queryService.GetBalanceAtAsync(new AccountId(accountId), date);

        return result.Match<IActionResult>(
            success => Ok(new { Balance = success.Data.Balance.Amount, Date = date }),
            failure => NotFound(new { Error = failure.Reason })
        );
    }

    /// <summary>
    /// 🔍 Event Sourcing: View all events for an account
    /// </summary>
    /// <param name="accountId">The account identifier</param>
    /// <returns>Complete event stream showing every change to the account</returns>
    /// <response code="200">Event stream retrieved successfully</response>
    /// <response code="404">Account not found or no events exist</response>
    /// <remarks>
    /// This endpoint demonstrates the core of event sourcing - viewing the raw event stream.
    /// Perfect for debugging, auditing, and understanding exactly what happened to an account.
    ///
    /// Each event contains:
    /// - Event type (AccountOpened, MoneyDeposited, etc.)
    /// - Version number for ordering
    /// - Timestamp of when it occurred
    /// - Complete event data
    /// </remarks>
    [HttpGet("{accountId}/events")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetAccountEvents(string accountId)
    {
        try
        {
            var eventStore = HttpContext.RequestServices.GetRequiredService<IEventStore>();
            var events = await eventStore.GetEventsAsync(new AccountId(accountId));

            return Ok(
                new
                {
                    AccountId = accountId,
                    EventCount = events.Count(),
                    Events = events.Select(e => new
                    {
                        Type = e.GetType().Name,
                        Version = e.Version,
                        Timestamp = e.Timestamp,
                        Data = e,
                    }),
                }
            );
        }
        catch (Exception ex)
        {
            return NotFound(new { Error = $"Failed to retrieve events: {ex.Message}" });
        }
    }

    /// <summary>
    /// ⏰ Time Travel: Get account state at any point in history
    /// </summary>
    /// <param name="accountId">The account identifier</param>
    /// <param name="date">The historical date to query (e.g., 2024-01-15T10:30:00Z)</param>
    /// <returns>Complete account state as it existed at the specified date</returns>
    /// <response code="200">Historical state retrieved successfully</response>
    /// <response code="404">Account not found or no state exists at the specified date</response>
    /// <remarks>
    /// This demonstrates event sourcing's powerful "time travel" capability.
    /// By replaying events up to a specific date, we can reconstruct the exact
    /// account state at any point in history.
    ///
    /// Use cases:
    /// - Compliance reporting: "What was the balance on Dec 31st?"
    /// - Debugging: "When did this issue start?"
    /// - Dispute resolution: "What transactions occurred before the complaint?"
    /// </remarks>
    /// <example>
    /// GET /api/account/ACC123/state-at?date=2024-01-15T10:30:00Z
    /// </example>
    [HttpGet("{accountId}/state-at")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetAccountStateAt(
        string accountId,
        [FromQuery] DateTimeOffset date
    )
    {
        try
        {
            var accountRepository =
                HttpContext.RequestServices.GetRequiredService<AccountRepository>();
            var events = await accountRepository.GetEventsUpToAsync(new AccountId(accountId), date);

            if (!events.Any())
            {
                return NotFound(new { Error = $"No account state found at {date}" });
            }

            // Reconstruct account state at the specified date
            var account = new Account();
            account.LoadFromHistory(events);

            return Ok(
                new
                {
                    AccountId = accountId,
                    AsOfDate = date,
                    State = new
                    {
                        CustomerId = account.CustomerId.Value,
                        Balance = account.Balance.Amount,
                        Status = account.Status.ToString(),
                        CreatedAt = account.CreatedAt,
                        LastTransactionAt = account.LastTransactionAt,
                        EventCount = events.Count(),
                    },
                }
            );
        }
        catch (Exception ex)
        {
            return NotFound(new { Error = $"Failed to get account state at {date}: {ex.Message}" });
        }
    }

    /// <summary>
    /// 📈 Account Timeline: Chronological view of all account changes
    /// </summary>
    /// <param name="accountId">The account identifier</param>
    /// <returns>Complete timeline showing account state after each event</returns>
    /// <response code="200">Timeline generated successfully</response>
    /// <response code="404">Account not found</response>
    /// <remarks>
    /// This endpoint provides a visual timeline of how the account evolved over time.
    /// Each entry shows the account state immediately after a specific event occurred.
    ///
    /// Perfect for:
    /// - Customer service representatives understanding account history
    /// - Debugging account state issues
    /// - Educational demonstrations of event sourcing
    /// - Creating account activity visualizations
    ///
    /// The timeline includes balance progression, status changes, and event details
    /// in chronological order.
    /// </remarks>
    [HttpGet("{accountId}/timeline")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetAccountTimeline(string accountId)
    {
        try
        {
            var eventStore = HttpContext.RequestServices.GetRequiredService<IEventStore>();
            var events = await eventStore.GetEventsAsync(new AccountId(accountId));

            var timeline = new List<object>();
            var account = new Account();

            foreach (var evt in events.OrderBy(e => e.Timestamp))
            {
                account.LoadFromHistory(new[] { evt });
                timeline.Add(
                    new
                    {
                        Timestamp = evt.Timestamp,
                        EventType = evt.GetType().Name,
                        Balance = account.Balance.Amount,
                        Status = account.Status.ToString(),
                        EventData = evt,
                    }
                );
            }

            return Ok(
                new
                {
                    AccountId = accountId,
                    Timeline = timeline,
                    TotalEvents = timeline.Count,
                }
            );
        }
        catch (Exception ex)
        {
            return NotFound(new { Error = $"Failed to get account timeline: {ex.Message}" });
        }
    }

    /// <summary>
    /// 🤔 What-If Analysis: Hypothetical balance calculations
    /// </summary>
    /// <param name="accountId">The account identifier</param>
    /// <param name="excludeFrom">Exclude events from this date onwards</param>
    /// <param name="excludeTo">Exclude events up to this date</param>
    /// <param name="excludeDescription">Exclude events containing this description</param>
    /// <returns>Comparison of actual balance vs hypothetical balance</returns>
    /// <response code="200">What-if analysis completed successfully</response>
    /// <response code="404">Account not found</response>
    /// <remarks>
    /// This endpoint showcases event sourcing's analytical power by answering
    /// "what if" questions about historical transactions.
    ///
    /// Examples:
    /// - "What if those large withdrawals in March hadn't happened?"
    /// - "What would the balance be without overdraft fees?"
    /// - "How much did the customer spend on ATM withdrawals?"
    ///
    /// Perfect for:
    /// - Customer service scenarios
    /// - Financial planning discussions
    /// - Fraud impact analysis
    /// - Fee calculation verification
    /// </remarks>
    /// <example>
    /// GET /api/account/ACC123/what-if?excludeDescription=overdraft
    /// GET /api/account/ACC123/what-if?excludeFrom=2024-03-01&amp;excludeTo=2024-03-31
    /// </example>
    [HttpGet("{accountId}/what-if")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetWhatIfBalance(
        string accountId,
        [FromQuery] DateTimeOffset? excludeFrom = null,
        [FromQuery] DateTimeOffset? excludeTo = null,
        [FromQuery] string? excludeDescription = null
    )
    {
        try
        {
            var eventStore = HttpContext.RequestServices.GetRequiredService<IEventStore>();
            var allEvents = await eventStore.GetEventsAsync(new AccountId(accountId));

            // Filter out events based on criteria
            var filteredEvents = allEvents
                .Where(e =>
                    (excludeFrom == null || e.Timestamp < excludeFrom)
                    && (excludeTo == null || e.Timestamp > excludeTo)
                    && (
                        excludeDescription == null || !e.GetType().Name.Contains(excludeDescription)
                    )
                )
                .ToList();

            // Reconstruct account state with filtered events
            var account = new Account();
            account.LoadFromHistory(filteredEvents);

            var actualAccount = new Account();
            actualAccount.LoadFromHistory(allEvents);

            return Ok(
                new
                {
                    AccountId = accountId,
                    Scenario = new
                    {
                        ExcludeFrom = excludeFrom,
                        ExcludeTo = excludeTo,
                        ExcludeDescription = excludeDescription,
                    },
                    WhatIfBalance = account.Balance.Amount,
                    ActualBalance = actualAccount.Balance.Amount,
                    ExcludedEventCount = allEvents.Count() - filteredEvents.Count,
                    IncludedEventCount = filteredEvents.Count,
                }
            );
        }
        catch (Exception ex)
        {
            return NotFound(new { Error = $"Failed to calculate what-if balance: {ex.Message}" });
        }
    }

    /// <summary>
    /// 📋 Compliance: Complete audit trail for regulatory reporting
    /// </summary>
    /// <param name="accountId">The account identifier</param>
    /// <param name="from">Start date for audit period (optional)</param>
    /// <param name="to">End date for audit period (optional)</param>
    /// <returns>Chronological audit trail with detailed event information</returns>
    /// <response code="200">Audit trail generated successfully</response>
    /// <response code="404">Account not found</response>
    /// <remarks>
    /// This endpoint demonstrates event sourcing's built-in audit capabilities.
    /// Every change is permanently recorded with timestamps, versions, and complete details.
    ///
    /// Perfect for:
    /// - Regulatory compliance (SOX, PCI-DSS, etc.)
    /// - Internal audits and reviews
    /// - Dispute resolution and investigations
    /// - Forensic analysis of account activity
    ///
    /// Features:
    /// - Immutable audit trail (events cannot be modified or deleted)
    /// - Cryptographic ordering via version numbers
    /// - Complete transaction details with timestamps
    /// - Filterable by date range
    /// </remarks>
    /// <example>
    /// GET /api/account/ACC123/audit-trail
    /// GET /api/account/ACC123/audit-trail?from=2024-01-01&amp;to=2024-12-31
    /// </example>
    [HttpGet("{accountId}/audit-trail")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetAuditTrail(
        string accountId,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null
    )
    {
        try
        {
            var eventStore = HttpContext.RequestServices.GetRequiredService<IEventStore>();
            var allEvents = await eventStore.GetEventsAsync(new AccountId(accountId));

            var filteredEvents = allEvents
                .Where(e =>
                    (from == null || e.Timestamp >= from) && (to == null || e.Timestamp <= to)
                )
                .OrderBy(e => e.Timestamp)
                .ToList();

            var auditTrail = filteredEvents
                .Select(e => new
                {
                    Timestamp = e.Timestamp,
                    EventType = e.GetType().Name,
                    Version = e.Version,
                    Details = GetEventDetails(e),
                })
                .ToList();

            return Ok(
                new
                {
                    AccountId = accountId,
                    Period = new { From = from, To = to },
                    AuditTrail = auditTrail,
                    TotalEvents = auditTrail.Count,
                }
            );
        }
        catch (Exception ex)
        {
            return NotFound(new { Error = $"Failed to get audit trail: {ex.Message}" });
        }
    }

    private static object GetEventDetails(Domain.Events.BaseEvent evt)
    {
        return evt switch
        {
            Domain.Events.AccountOpened opened => new
            {
                CustomerId = opened.CustomerId.Value,
                InitialBalance = opened.InitialBalance.Amount,
            },
            Domain.Events.MoneyDeposited deposited => new
            {
                Amount = deposited.Amount.Amount,
                Description = deposited.Description,
            },
            Domain.Events.MoneyWithdrawn withdrawn => new
            {
                Amount = withdrawn.Amount.Amount,
                Description = withdrawn.Description,
            },
            Domain.Events.MoneyTransferred transferred => new
            {
                Amount = transferred.Amount.Amount,
                FromAccountId = transferred.FromAccountId.Value,
                ToAccountId = transferred.ToAccountId.Value,
                Description = transferred.Description,
            },
            _ => new { RawData = evt },
        };
    }

    /// <summary>
    /// Health check endpoint for monitoring
    /// </summary>
    /// <returns>API health status and timestamp</returns>
    /// <response code="200">API is healthy and operational</response>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTimeOffset.UtcNow });
    }
}

// Request DTOs

/// <summary>
/// Request to open a new bank account
/// </summary>
/// <param name="CustomerId">Unique identifier for the customer (e.g., "CUST001")</param>
/// <param name="InitialBalance">Starting balance in USD (must be non-negative)</param>
/// <param name="Description">Optional description for the account opening</param>
public record OpenAccountRequest(
    [Required] string CustomerId,
    [Range(0, double.MaxValue)] decimal InitialBalance,
    string? Description
);

/// <summary>
/// Request to deposit money into an account
/// </summary>
/// <param name="AccountId">Target account identifier</param>
/// <param name="Amount">Amount to deposit in USD (must be positive)</param>
/// <param name="Description">Optional description for the deposit transaction</param>
public record DepositRequest(
    [Required] string AccountId,
    [Range(0.01, double.MaxValue)] decimal Amount,
    string? Description
);

/// <summary>
/// Request to withdraw money from an account
/// </summary>
/// <param name="AccountId">Source account identifier</param>
/// <param name="Amount">Amount to withdraw in USD (must be positive)</param>
/// <param name="Description">Optional description for the withdrawal transaction</param>
public record WithdrawRequest(
    [Required] string AccountId,
    [Range(0.01, double.MaxValue)] decimal Amount,
    string? Description
);

/// <summary>
/// Request to transfer money between accounts
/// </summary>
/// <param name="FromAccountId">Source account identifier</param>
/// <param name="ToAccountId">Destination account identifier</param>
/// <param name="Amount">Amount to transfer in USD (must be positive)</param>
/// <param name="Description">Optional description for the transfer transaction</param>
public record TransferRequest(
    [Required] string FromAccountId,
    [Required] string ToAccountId,
    [Range(0.01, double.MaxValue)] decimal Amount,
    string? Description
);
