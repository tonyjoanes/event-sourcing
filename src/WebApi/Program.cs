using System.Reflection;
using Application.Handlers;
using Application.Services;
using Infrastructure.EventStore;
using Infrastructure.ReadModels;
using Infrastructure.ReadModels.Projections;
using Infrastructure.Repositories;
using Raven.Client.Documents;
using WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new()
        {
            Title = "Event Sourcing Banking Demo API",
            Version = "v1",
            Description =
                "A comprehensive demonstration of event sourcing patterns in banking, featuring time travel queries, audit trails, and what-if analysis.",
            Contact = new()
            {
                Name = "Event Sourcing Demo",
                Url = new Uri("https://github.com/tonyjoanes/event-sourcing"),
            },
        }
    );

    // Include XML comments in Swagger
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // Group endpoints by category
    c.TagActionsBy(api =>
        new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] }
    );
    c.DocInclusionPredicate((name, api) => true);
});

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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Sourcing Banking Demo API v1");
        c.RoutePrefix = "swagger"; // Keep it at /swagger instead of root
        c.DocumentTitle = "Event Sourcing Banking Demo API";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
    });
}

app.UseHttpsRedirection();

// Add exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Add routing and map controllers
app.UseRouting();
app.MapControllers();

app.Run();
