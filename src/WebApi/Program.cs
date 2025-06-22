using Infrastructure.EventStore;
using Infrastructure.ReadModels;
using Infrastructure.ReadModels.Projections;
using Infrastructure.Repositories;
using Application.Services;
using Application.Handlers;
using WebApi.Middleware;
using Raven.Client.Documents;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add controllers
builder.Services.AddControllers();

// RavenDB configuration (for development/demo)
builder.Services.AddSingleton<IDocumentStore>(sp =>
{
    var store = new DocumentStore
    {
        Urls = new[] { "http://localhost:38888" },
        Database = "EventSourcingBankingDemo",
    };
    store.Initialize();
    return store;
});

// Event store and repository
builder.Services.AddScoped<IEventStore, RavenDbEventStore>();
builder.Services.AddScoped<AccountRepository>();

// Read models configuration
var connectionString =
    builder.Configuration.GetConnectionString("ReadModels") ?? "Data Source=readmodels.db";
builder.Services.AddSingleton<IReadModelStore>(sp => new SqliteReadModelStore(connectionString));
builder.Services.AddScoped<AccountSummaryProjectionHandler>();
builder.Services.AddScoped<TransactionHistoryProjectionHandler>();
builder.Services.AddScoped<TransactionEnrichmentService>();
builder.Services.AddScoped<EventDispatcher>();
builder.Services.AddScoped<DatabaseInitializer>(sp => new DatabaseInitializer(connectionString));

// Application layer services
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<QueryService>();

// Command handlers
builder.Services.AddScoped<OpenAccountCommandHandler>();
builder.Services.AddScoped<DepositCommandHandler>();
builder.Services.AddScoped<WithdrawCommandHandler>();
builder.Services.AddScoped<TransferCommandHandler>();

// Query handlers
builder.Services.AddScoped<GetAccountSummaryQueryHandler>();
builder.Services.AddScoped<GetTransactionHistoryQueryHandler>();
builder.Services.AddScoped<GetBalanceAtQueryHandler>();

var app = builder.Build();

// Initialize read models database
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await dbInitializer.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Add routing and map controllers
app.UseRouting();
app.MapControllers();

app.Run();
