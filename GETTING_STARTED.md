# Getting Started with Event Sourcing Banking Demo

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Docker Desktop (for RavenDB)
- Visual Studio Code, Cursor, or Visual Studio

### Setup Steps

1. **Start RavenDB**
   ```bash
   cd docker
   docker-compose up -d ravendb
   ```
   RavenDB Studio will be available at: http://localhost:8080

2. **Build the Solution**
   ```bash
   dotnet build
   ```

3. **Run Tests**
   ```bash
   dotnet test
   ```

4. **Run Console Application**
   ```bash
   dotnet run --project src/ConsoleApp
   ```

5. **Run Web API**
   ```bash
   dotnet run --project src/WebApi
   ```
   API will be available at: https://localhost:7001
   Swagger UI at: https://localhost:7001/swagger

## 📁 Project Structure

```
EventSourcingBankingDemo/
├── src/
│   ├── Domain/                 # Core business logic
│   │   ├── Aggregates/         # Aggregate roots (Account, etc.)
│   │   ├── Events/             # Domain events
│   │   ├── ValueObjects/       # Money, AccountId, etc.
│   │   └── Services/           # Domain services
│   ├── Infrastructure/         # Data access & external services
│   │   ├── EventStore/         # RavenDB event store
│   │   ├── ReadModels/         # SQLite read models
│   │   └── Repositories/       # Aggregate repositories
│   ├── Application/            # Application services
│   │   ├── Commands/           # Command objects
│   │   ├── Queries/            # Query objects
│   │   ├── Handlers/           # Command/Query handlers
│   │   └── Services/           # Application services
│   ├── ConsoleApp/             # Interactive console demo
│   └── WebApi/                 # REST API
├── tests/                      # Test projects
└── docker/                     # Docker configuration
```

## 🎯 Next Steps

### 1. Implement Account Aggregate
Start by creating the `Account` aggregate in `src/Domain/Aggregates/Account.cs`:

```csharp
public class Account : AggregateRoot
{
    public AccountId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public Money Balance { get; private set; }
    public AccountStatus Status { get; private set; }
    
    // Business methods: Open, Deposit, Withdraw, Transfer
}
```

### 2. Create Account Events
Add events in `src/Domain/Events/AccountEvents.cs`:

```csharp
public record AccountOpened : BaseEvent
{
    public AccountId AccountId { get; init; }
    public CustomerId CustomerId { get; init; }
    public Money InitialBalance { get; init; }
}

public record MoneyDeposited : BaseEvent
{
    public Money Amount { get; init; }
    public string Description { get; init; }
}
```

### 3. Implement Event Store
Create the RavenDB event store implementation in `src/Infrastructure/EventStore/`.

### 4. Add Commands and Queries
Implement CQRS pattern in the Application layer.

## 🧪 Testing Strategy

- **Unit Tests**: Test aggregates and domain logic
- **Integration Tests**: Test event store and read models
- **End-to-End Tests**: Test complete workflows

## 📚 Key Concepts

### Event Sourcing
- Store events instead of current state
- Rebuild state by replaying events
- Complete audit trail
- Time travel capabilities

### CQRS (Command Query Responsibility Segregation)
- Separate read and write models
- Optimize for different use cases
- Eventual consistency

### Functional Programming
- Use OneOf for union types
- Immutable value objects
- Pure functions for business logic

## 🔧 Development Tips

1. **Start with Domain**: Focus on business logic first
2. **Event-First Design**: Design events before aggregates
3. **Test-Driven**: Write tests for business rules
4. **Small Steps**: Implement one feature at a time

## 📖 Resources

- [Event Sourcing Pattern](https://martinfowler.com/eaaDev/EventSourcing.html)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [OneOf Library](https://github.com/mcintyre321/OneOf)
- [RavenDB Documentation](https://ravendb.net/docs/)

---

Happy coding! 🎉 