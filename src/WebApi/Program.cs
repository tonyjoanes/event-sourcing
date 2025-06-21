using Infrastructure.EventStore;
using Infrastructure.ReadModels;
using Infrastructure.ReadModels.Projections;
using Infrastructure.Repositories;
using Raven.Client.Documents;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// RavenDB configuration (for development/demo)
builder.Services.AddSingleton<IDocumentStore>(sp =>
{
    var store = new DocumentStore
    {
        Urls = new[] { "http://localhost:8080" },
        Database = "EventSourcingBankingDemo"
    };
    store.Initialize();
    return store;
});

// Event store and repository
builder.Services.AddScoped<IEventStore, RavenDbEventStore>();
builder.Services.AddScoped<AccountRepository>();

// Read models configuration
var connectionString = builder.Configuration.GetConnectionString("ReadModels") ?? "Data Source=readmodels.db";
builder.Services.AddSingleton<IReadModelStore>(sp => new SqliteReadModelStore(connectionString));
builder.Services.AddScoped<AccountSummaryProjectionHandler>();
builder.Services.AddScoped<TransactionHistoryProjectionHandler>();
builder.Services.AddScoped<TransactionEnrichmentService>();
builder.Services.AddScoped<EventDispatcher>();
builder.Services.AddScoped<DatabaseInitializer>(sp => new DatabaseInitializer(connectionString));

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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
