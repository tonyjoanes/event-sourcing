using Application.Commands;
using Application.Queries;
using Application.Services;
using Domain.Aggregates;
using Domain.ValueObjects;
using Infrastructure.EventStore;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    [HttpPost("open")]
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

    [HttpPost("deposit")]
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

    [HttpPost("withdraw")]
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

    [HttpPost("transfer")]
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
    [HttpGet("{accountId}/summary")]
    public async Task<IActionResult> GetAccountSummary(string accountId)
    {
        var result = await _queryService.GetAccountSummaryAsync(new AccountId(accountId));

        return result.Match<IActionResult>(
            success => Ok(success.Data),
            failure => NotFound(new { Error = failure.Reason })
        );
    }

    [HttpGet("{accountId}/transactions")]
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

    [HttpGet("{accountId}/balance-at")]
    public async Task<IActionResult> GetBalanceAt(string accountId, [FromQuery] DateTimeOffset date)
    {
        var result = await _queryService.GetBalanceAtAsync(new AccountId(accountId), date);

        return result.Match<IActionResult>(
            success => Ok(new { Balance = success.Data.Balance.Amount, Date = date }),
            failure => NotFound(new { Error = failure.Reason })
        );
    }

    // View all events for an account (useful for debugging and understanding event sourcing)
    [HttpGet("{accountId}/events")]
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

    // Time Travel: Get complete account state at a specific point in time
    [HttpGet("{accountId}/state-at")]
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

    // Time Travel: Get account timeline showing all changes over time
    [HttpGet("{accountId}/timeline")]
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

    // Time Travel: What-if scenario - show balance if certain transactions didn't happen
    [HttpGet("{accountId}/what-if")]
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

    // Time Travel: Audit trail with detailed transaction history
    [HttpGet("{accountId}/audit-trail")]
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

    // Health check endpoint
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTimeOffset.UtcNow });
    }
}

// Request DTOs
public record OpenAccountRequest(string CustomerId, decimal InitialBalance, string? Description);

public record DepositRequest(string AccountId, decimal Amount, string? Description);

public record WithdrawRequest(string AccountId, decimal Amount, string? Description);

public record TransferRequest(
    string FromAccountId,
    string ToAccountId,
    decimal Amount,
    string? Description
);
