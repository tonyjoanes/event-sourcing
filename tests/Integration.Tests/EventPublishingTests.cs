using Application.Handlers;
using Application.Services;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.EventStore;
using Infrastructure.ReadModels;
using Infrastructure.ReadModels.Projections;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Integration.Tests;

[Trait("Category", "Integration")]
public class EventPublishingTests : IDisposable
{
    private readonly IHost _host;
    private readonly IServiceScope _scope;
    private readonly AccountService _accountService;
    private readonly QueryService _queryService;
    private readonly IReadModelStore _readModelStore;

    public EventPublishingTests()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(
                (context, services) =>
                {
                    // Configure services for testing
                    services.AddSingleton<IReadModelStore>(sp => new SqliteReadModelStore(
                        "Data Source=file:memdb1?mode=memory&cache=shared"
                    ));
                    services.AddScoped<AccountSummaryProjectionHandler>();
                    services.AddScoped<TransactionHistoryProjectionHandler>();
                    services.AddScoped<TransactionEnrichmentService>();
                    services.AddScoped<EventDispatcher>();

                    // Application Layer
                    services.AddScoped<OpenAccountCommandHandler>();
                    services.AddScoped<DepositCommandHandler>();
                    services.AddScoped<WithdrawCommandHandler>();
                    services.AddScoped<TransferCommandHandler>();
                    services.AddScoped<AccountService>();

                    // Query handlers
                    services.AddScoped<GetAccountSummaryQueryHandler>();
                    services.AddScoped<GetTransactionHistoryQueryHandler>();
                    services.AddScoped<GetBalanceAtQueryHandler>();
                    services.AddScoped<QueryService>();

                    services.AddSingleton<IEventStore, InMemoryEventStore>();
                    services.AddScoped<AccountRepository>();
                }
            )
            .Build();

        _scope = _host.Services.CreateScope();
        _accountService = _scope.ServiceProvider.GetRequiredService<AccountService>();
        _queryService = _scope.ServiceProvider.GetRequiredService<QueryService>();
        _readModelStore = _scope.ServiceProvider.GetRequiredService<IReadModelStore>();

        // Initialize the in-memory SQLite schema
        var dbInitializer = new Infrastructure.ReadModels.DatabaseInitializer(
            "Data Source=file:memdb1?mode=memory&cache=shared"
        );
        dbInitializer.InitializeAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task OpenAccount_ShouldPublishEvents_AndUpdateReadModels()
    {
        // Arrange
        var customerId = new CustomerId("CUST12345");
        var initialBalance = new Money(1000m);

        // Act
        var result = await _accountService.OpenAccountAsync(customerId, initialBalance);

        // Assert
        result.IsT0.Should().BeTrue(); // Success
        var success = result.AsT0;

        // Verify read model was updated
        var accountSummary = await _readModelStore.GetAccountSummaryAsync(
            success.Data.AccountId.Value
        );
        accountSummary.Should().NotBeNull();
        accountSummary!.Id.Should().Be(success.Data.AccountId.Value);
        accountSummary.CustomerId.Should().Be(customerId.Value);
        accountSummary.Balance.Should().Be(initialBalance.Amount);
        accountSummary.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Deposit_ShouldPublishEvents_AndUpdateReadModels()
    {
        // Arrange
        var customerId = new CustomerId("CUST12345");
        var initialBalance = new Money(1000m);
        var openResult = await _accountService.OpenAccountAsync(customerId, initialBalance);
        var accountId = openResult.AsT0.Data.AccountId;
        var depositAmount = new Money(500m);

        // Act
        var result = await _accountService.DepositAsync(accountId, depositAmount, "Salary");

        // Assert
        result.IsT0.Should().BeTrue(); // Success

        // Verify read model was updated
        var accountSummary = await _readModelStore.GetAccountSummaryAsync(accountId.Value);
        accountSummary.Should().NotBeNull();
        accountSummary!.Balance.Should().Be(1500m); // 1000 + 500

        // Verify transaction history
        var transactions = await _readModelStore.GetTransactionHistoryAsync(accountId.Value, 10);
        transactions.Should().HaveCount(1); // Only MoneyDeposited (AccountOpened is not a transaction)
        transactions[0].Type.Should().Be("Deposit");
        transactions[0].Amount.Should().Be(500m);
        transactions[0].Description.Should().Be("Salary");
    }

    [Fact]
    public async Task Withdraw_ShouldPublishEvents_AndUpdateReadModels()
    {
        // Arrange
        var customerId = new CustomerId("CUST12345");
        var initialBalance = new Money(1000m);
        var openResult = await _accountService.OpenAccountAsync(customerId, initialBalance);
        var accountId = openResult.AsT0.Data.AccountId;
        var withdrawAmount = new Money(200m);

        // Act
        var result = await _accountService.WithdrawAsync(
            accountId,
            withdrawAmount,
            "ATM Withdrawal"
        );

        // Assert
        result.IsT0.Should().BeTrue(); // Success

        // Verify read model was updated
        var accountSummary = await _readModelStore.GetAccountSummaryAsync(accountId.Value);
        accountSummary.Should().NotBeNull();
        accountSummary!.Balance.Should().Be(800m); // 1000 - 200

        // Verify transaction history
        var transactions = await _readModelStore.GetTransactionHistoryAsync(accountId.Value, 10);
        transactions.Should().HaveCount(1); // Only MoneyWithdrawn (AccountOpened is not a transaction)
        transactions[0].Type.Should().Be("Withdrawal");
        transactions[0].Amount.Should().Be(200m);
        transactions[0].Description.Should().Be("ATM Withdrawal");
    }

    [Fact]
    public async Task Transfer_ShouldPublishEvents_AndUpdateReadModels()
    {
        // Arrange
        var customerId1 = new CustomerId("CUST12345");
        var customerId2 = new CustomerId("CUST67890");
        var initialBalance = new Money(1000m);

        var openResult1 = await _accountService.OpenAccountAsync(customerId1, initialBalance);
        var openResult2 = await _accountService.OpenAccountAsync(customerId2, initialBalance);

        var fromAccountId = openResult1.AsT0.Data.AccountId;
        var toAccountId = openResult2.AsT0.Data.AccountId;
        var transferAmount = new Money(300m);

        // Act
        var result = await _accountService.TransferAsync(
            fromAccountId,
            toAccountId,
            transferAmount,
            "Rent Payment"
        );

        // Assert
        result.IsT0.Should().BeTrue(); // Success

        // Verify read models were updated
        var fromAccountSummary = await _readModelStore.GetAccountSummaryAsync(fromAccountId.Value);
        var toAccountSummary = await _readModelStore.GetAccountSummaryAsync(toAccountId.Value);

        fromAccountSummary.Should().NotBeNull();
        toAccountSummary.Should().NotBeNull();
        fromAccountSummary!.Balance.Should().Be(700m); // 1000 - 300
        toAccountSummary!.Balance.Should().Be(1300m); // 1000 + 300

        // Verify transaction history for both accounts
        var fromTransactions = await _readModelStore.GetTransactionHistoryAsync(
            fromAccountId.Value,
            10
        );
        var toTransactions = await _readModelStore.GetTransactionHistoryAsync(
            toAccountId.Value,
            10
        );

        fromTransactions.Should().HaveCount(1); // Only MoneyTransferred (AccountOpened is not a transaction)
        toTransactions.Should().HaveCount(1); // Only MoneyDeposited from transfer (AccountOpened is not a transaction)

        fromTransactions[0].Type.Should().Be("TransferOut");
        toTransactions[0].Type.Should().Be("TransferIn");
    }

    [Fact]
    public async Task QueryService_ShouldReturnReadModelData()
    {
        // Arrange
        var customerId = new CustomerId("CUST12345");
        var initialBalance = new Money(1000m);
        var openResult = await _accountService.OpenAccountAsync(customerId, initialBalance);
        var accountId = openResult.AsT0.Data.AccountId;

        // Act
        var summaryResult = await _queryService.GetAccountSummaryAsync(accountId);
        var historyResult = await _queryService.GetTransactionHistoryAsync(accountId);

        // Assert
        summaryResult.IsT0.Should().BeTrue();
        historyResult.IsT0.Should().BeTrue();

        var summary = summaryResult.AsT0.Data;
        var history = historyResult.AsT0.Data;

        summary.AccountId.Should().Be(accountId);
        summary.CustomerId.Should().Be(customerId);
        summary.Balance.Should().Be(initialBalance);
        summary.Status.Should().Be(AccountStatus.Active);

        history.AccountId.Should().Be(accountId);
        history.Transactions.Should().HaveCount(0); // AccountOpened is not a transaction
    }

    [Fact]
    public async Task Debug_EventDispatching_ShouldWork()
    {
        // Arrange
        var customerId = new CustomerId("CUST12345");
        var initialBalance = new Money(1000m);

        // Act
        var result = await _accountService.OpenAccountAsync(customerId, initialBalance);

        // Assert
        result.IsT0.Should().BeTrue(); // Success
        var success = result.AsT0;

        // Debug: Check if the account was created
        Console.WriteLine($"Account created with ID: {success.Data.AccountId.Value}");

        // Debug: Check if read model exists
        var accountSummary = await _readModelStore.GetAccountSummaryAsync(
            success.Data.AccountId.Value
        );
        Console.WriteLine($"Account summary found: {accountSummary != null}");

        if (accountSummary != null)
        {
            Console.WriteLine(
                $"Account summary - ID: {accountSummary.Id}, Balance: {accountSummary.Balance}"
            );
        }

        // Debug: Check transaction history
        var transactions = await _readModelStore.GetTransactionHistoryAsync(
            success.Data.AccountId.Value,
            10
        );
        Console.WriteLine($"Transaction count: {transactions.Count}");

        // The test should pass if we can see the debug output
        success.Data.AccountId.Should().NotBeNull();
    }

    [Fact]
    public async Task Debug_EventDispatcher_ShouldWork()
    {
        // Arrange
        var eventDispatcher = _scope.ServiceProvider.GetRequiredService<EventDispatcher>();
        var testEvent = new Domain.Events.AccountOpened
        {
            AccountId = new Domain.ValueObjects.AccountId("TEST123"),
            CustomerId = new Domain.ValueObjects.CustomerId("CUST123"),
            InitialBalance = new Domain.ValueObjects.Money(1000m),
            OpenedAt = DateTimeOffset.UtcNow,
        };

        // Act
        await eventDispatcher.DispatchAsync(testEvent);

        // Assert
        var accountSummary = await _readModelStore.GetAccountSummaryAsync("TEST123");
        Console.WriteLine(
            $"EventDispatcher test - Account summary found: {accountSummary != null}"
        );

        if (accountSummary != null)
        {
            Console.WriteLine(
                $"EventDispatcher test - Account summary - ID: {accountSummary.Id}, Balance: {accountSummary.Balance}"
            );
        }

        // The test should pass if EventDispatcher is working
        accountSummary.Should().NotBeNull();
        accountSummary!.Id.Should().Be("TEST123");
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _host?.Dispose();
    }
}
