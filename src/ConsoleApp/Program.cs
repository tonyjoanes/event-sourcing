using Application.Handlers;
using Application.Services;
using Domain.ValueObjects;
using Infrastructure.EventStore;
using Infrastructure.ReadModels;
using Infrastructure.ReadModels.Projections;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raven.Client.Documents;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(
        (context, services) =>
        {
            // RavenDB configuration
            services.AddSingleton<IDocumentStore>(sp =>
            {
                var store = new DocumentStore
                {
                    Urls = new[] { "http://localhost:38888" },
                    Database = "EventSourcingBankingDemo",
                };
                store.Initialize();

                // Ensure database exists
                try
                {
                    store.Maintenance.Server.Send(
                        new Raven.Client.ServerWide.Operations.CreateDatabaseOperation(
                            new Raven.Client.ServerWide.DatabaseRecord("EventSourcingBankingDemo")
                        )
                    );
                }
                catch (Raven.Client.Exceptions.Database.DatabaseDisabledException)
                {
                    // Database already exists, ignore
                }

                return store;
            });

            // Event store and repository
            services.AddScoped<IEventStore, RavenDbEventStore>();
            services.AddScoped<AccountRepository>();

            // Read models configuration
            var connectionString =
                context.Configuration.GetConnectionString("ReadModels")
                ?? "Data Source=readmodels.db";
            services.AddSingleton<IReadModelStore>(sp => new SqliteReadModelStore(
                connectionString
            ));
            services.AddScoped<AccountSummaryProjectionHandler>();
            services.AddScoped<TransactionHistoryProjectionHandler>();
            services.AddScoped<TransactionEnrichmentService>();
            services.AddScoped<EventDispatcher>();
            services.AddScoped<DatabaseInitializer>(sp => new DatabaseInitializer(
                connectionString
            ));

            // Application Layer
            services.AddScoped<OpenAccountCommandHandler>();
            services.AddScoped<DepositCommandHandler>();
            services.AddScoped<WithdrawCommandHandler>();
            services.AddScoped<TransferCommandHandler>();
            services.AddScoped<AccountService>();
        }
    )
    .Build();

// Initialize read models database
using (var scope = host.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await dbInitializer.InitializeAsync();

    // Give RavenDB a moment to be fully ready
    Console.WriteLine("Waiting for RavenDB to be fully ready...");
    await Task.Delay(2000);
}

Console.WriteLine("Event Sourcing Banking Demo - Console Application");
Console.WriteLine("Read models infrastructure initialized successfully!\n");

using (var scope = host.Services.CreateScope())
{
    var accountService = scope.ServiceProvider.GetRequiredService<AccountService>();

    while (true)
    {
        Console.WriteLine("\nEnter command (open, deposit, withdraw, transfer, exit):");
        var input = Console.ReadLine()?.Trim().ToLower();
        if (input == "exit")
            break;

        try
        {
            switch (input)
            {
                case "open":
                    Console.Write("Customer ID: ");
                    var customerId = new CustomerId(Console.ReadLine() ?? "");
                    Console.Write("Initial Balance: ");
                    var initialBalance = new Money(decimal.Parse(Console.ReadLine() ?? "0"));
                    var openResult = await accountService.OpenAccountAsync(
                        customerId,
                        initialBalance
                    );
                    openResult.Switch(
                        success =>
                            Console.WriteLine(
                                $"Account opened: {success.Data.AccountId}, Customer: {success.Data.CustomerId}, Balance: {success.Data.InitialBalance}"
                            ),
                        failure => Console.WriteLine($"Error: {failure.Reason}")
                    );
                    break;
                case "deposit":
                    Console.Write("Account ID: ");
                    var depAccountId = new AccountId(Console.ReadLine() ?? "");
                    Console.Write("Amount: ");
                    var depAmount = new Money(decimal.Parse(Console.ReadLine() ?? "0"));
                    Console.Write("Description (optional): ");
                    var depDesc = Console.ReadLine();
                    var depResult = await accountService.DepositAsync(
                        depAccountId,
                        depAmount,
                        depDesc
                    );
                    depResult.Switch(
                        success =>
                            Console.WriteLine(
                                $"Deposited {success.Data.Amount} to {success.Data.AccountId}. New balance: {success.Data.NewBalance}"
                            ),
                        failure => Console.WriteLine($"Error: {failure.Reason}")
                    );
                    break;
                case "withdraw":
                    Console.Write("Account ID: ");
                    var wAccountId = new AccountId(Console.ReadLine() ?? "");
                    Console.Write("Amount: ");
                    var wAmount = new Money(decimal.Parse(Console.ReadLine() ?? "0"));
                    Console.Write("Description (optional): ");
                    var wDesc = Console.ReadLine();
                    var wResult = await accountService.WithdrawAsync(wAccountId, wAmount, wDesc);
                    wResult.Switch(
                        success =>
                            Console.WriteLine(
                                $"Withdrew {success.Data.Amount} from {success.Data.AccountId}. New balance: {success.Data.NewBalance}"
                            ),
                        failure => Console.WriteLine($"Error: {failure.Reason}")
                    );
                    break;
                case "transfer":
                    Console.Write("From Account ID: ");
                    var fromId = new AccountId(Console.ReadLine() ?? "");
                    Console.Write("To Account ID: ");
                    var toId = new AccountId(Console.ReadLine() ?? "");
                    Console.Write("Amount: ");
                    var tAmount = new Money(decimal.Parse(Console.ReadLine() ?? "0"));
                    Console.Write("Description (optional): ");
                    var tDesc = Console.ReadLine();
                    var tResult = await accountService.TransferAsync(fromId, toId, tAmount, tDesc);
                    tResult.Switch(
                        success =>
                            Console.WriteLine(
                                $"Transferred {success.Data.Amount} from {success.Data.FromAccountId} to {success.Data.ToAccountId}"
                            ),
                        failure => Console.WriteLine($"Error: {failure.Reason}")
                    );
                    break;
                default:
                    Console.WriteLine("Unknown command. Try again.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
}

Console.WriteLine("Goodbye!");
