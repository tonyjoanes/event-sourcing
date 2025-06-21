using Microsoft.Extensions.Configuration;
using Infrastructure.EventStore;
using Infrastructure.ReadModels;
using Infrastructure.ReadModels.Projections;
using Infrastructure.Repositories;
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
                    Urls = new[] { "http://localhost:8080" },
                    Database = "EventSourcingBankingDemo",
                };
                store.Initialize();
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
        }
    )
    .Build();

// Initialize read models database
using (var scope = host.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await dbInitializer.InitializeAsync();
}

Console.WriteLine("Event Sourcing Banking Demo - Console Application");
Console.WriteLine("Read models infrastructure initialized successfully!");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
