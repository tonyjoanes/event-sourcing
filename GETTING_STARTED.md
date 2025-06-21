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

## ✅ Completed Features

### Core Domain (Banking)
- [x] Account aggregate with event sourcing
- [x] Account operations: Open, Deposit, Withdraw, Transfer
- [x] Account freezing/closing
- [x] Overdraft fee and interest accrual
- [x] Comprehensive business rule validation
- [x] Functional programming patterns with OneOf and Option<T>

### Value Objects & Domain Events
- [x] Money value object with currency support
- [x] AccountId and CustomerId value objects
- [x] AccountStatus enum
- [x] Complete set of account events (AccountOpened, MoneyDeposited, etc.)
- [x] Option<T> type for optional values
- [x] BaseEvent with versioning support

### Testing
- [x] Comprehensive unit tests for Account aggregate
- [x] Value object tests (Money, etc.)
- [x] Event sourcing reconstruction tests
- [x] Business rule validation tests
- [x] Functional programming pattern tests

### Infrastructure Setup
- [x] .NET 8 solution with multiple projects
- [x] Project references and dependencies
- [x] Docker Compose for RavenDB
- [x] GitHub Actions CI/CD pipeline
- [x] Code coverage and security scanning

## 🎯 Next Tasks to Implement

### 1. Event Store Infrastructure (High Priority)
- [ ] **IEventStore interface** - Define event store contract
- [ ] **RavenDbEventStore implementation** - Store events in RavenDB
- [ ] **Event serialization/deserialization** - JSON serialization for events
- [ ] **AccountRepository** - Repository pattern for Account aggregate
- [ ] **Event versioning and concurrency** - Optimistic concurrency control
- [ ] **Event metadata** - Timestamps, correlation IDs, causation IDs

### 2. Read Models & Projections (High Priority)
- [ ] **IReadModelStore interface** - Define read model contract
- [ ] **SqliteReadModelStore implementation** - SQLite for read models
- [ ] **AccountSummaryProjection** - Current account state
- [ ] **TransactionHistoryProjection** - Transaction list
- [ ] **Event handlers for projections** - Update read models from events
- [ ] **Projection rebuild functionality** - Rebuild from event stream

### 3. Application Layer - CQRS (High Priority)
- [ ] **Commands** - OpenAccountCommand, DepositCommand, WithdrawCommand, TransferCommand
- [ ] **Queries** - GetAccountSummaryQuery, GetTransactionHistoryQuery, GetBalanceAtQuery
- [ ] **Command handlers** - Handle commands and apply to aggregates
- [ ] **Query handlers** - Handle queries and return read models
- [ ] **Event handlers** - Update projections when events occur
- [ ] **AccountService** - Application service for account operations

### 4. Console Application (Medium Priority)
- [ ] **Interactive command interface** - Parse user commands
- [ ] **Demo scenarios runner** - Predefined banking scenarios
- [ ] **Event store browser** - View stored events
- [ ] **Time travel demonstrations** - Show historical state
- [ ] **Colored output** - Better user experience
- [ ] **Command validation** - Input validation and error handling

### 5. Web API (Medium Priority)
- [ ] **AccountController** - REST endpoints for account operations
- [ ] **Command endpoints** - POST endpoints for commands
- [ ] **Query endpoints** - GET endpoints for queries
- [ ] **Time travel endpoints** - Historical data queries
- [ ] **OpenAPI documentation** - Swagger/OpenAPI spec
- [ ] **Error handling middleware** - Consistent error responses

### 6. Advanced Features (Lower Priority)
- [ ] **Snapshotting** - Performance optimization for large event streams
- [ ] **Event schema evolution** - Handle event version changes
- [ ] **Saga pattern** - Complex business processes (transfers between accounts)
- [ ] **Real-time updates** - SignalR for live updates
- [ ] **Analytics projections** - Customer behavior analysis
- [ ] **Compliance reporting** - Audit trail and regulatory reports

### 7. Testing & Quality (Ongoing)
- [ ] **Integration tests** - Test event store and read models
- [ ] **End-to-end tests** - Complete workflow testing
- [ ] **Performance tests** - Load testing for event store
- [ ] **Property-based tests** - FsCheck for invariant testing
- [ ] **Mutation testing** - Stryker.NET for test quality

### 8. Documentation & Examples (Ongoing)
- [ ] **Event catalog** - Document all domain events
- [ ] **Architecture decisions** - ADR documentation
- [ ] **Blog post examples** - Code snippets for articles
- [ ] **Demo scenarios** - Step-by-step tutorials
- [ ] **Performance benchmarks** - Event store performance data

## 🧪 Testing Strategy

- **Unit Tests**: Test aggregates and domain logic ✅
- **Integration Tests**: Test event store and read models
- **End-to-End Tests**: Test complete workflows
- **Performance Tests**: Load testing and benchmarks

## 📚 Key Concepts Demonstrated

### Event Sourcing Benefits
- **Audit Trail**: Complete history of all account changes ✅
- **Time Travel**: Query account state at any point in time
- **Debugging**: Replay events to understand system behavior
- **Analytics**: Rich data for business intelligence
- **Compliance**: Immutable records for regulatory requirements

### Functional Programming
- **Union Types**: `OneOf<Success, InsufficientFunds, AccountFrozen>` ✅
- **Option Types**: `Option<string>` for optional values ✅
- **Immutability**: Events and value objects are immutable ✅
- **Pure Functions**: Business logic without side effects ✅

### CQRS Implementation
- **Command Side**: Optimized for writes, consistency, business rules
- **Query Side**: Optimized for reads, performance, reporting
- **Eventual Consistency**: Read models updated asynchronously
- **Scalability**: Commands and queries can scale independently

## 🔧 Development Tips

1. **Start with Domain**: Focus on business logic first ✅
2. **Event-First Design**: Design events before aggregates ✅
3. **Test-Driven**: Write tests for business rules ✅
4. **Small Steps**: Implement one feature at a time
5. **Event Sourcing**: Always think in terms of events
6. **CQRS**: Separate read and write concerns

## 📖 Resources

- [Event Sourcing Pattern](https://martinfowler.com/eaaDev/EventSourcing.html)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [OneOf Library](https://github.com/mcintyre321/OneOf)
- [RavenDB Documentation](https://ravendb.net/docs/)
- [Functional Programming in C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/functional-programming-vs-imperative-programming)

## 🎮 Quick Demo Commands (Future)

```bash
# Open account
> open-account --customer "John Doe" --initial-balance 1000

# Make transactions
> deposit --account ACC001 --amount 500 --description "Salary"
> withdraw --account ACC001 --amount 200 --description "ATM Withdrawal"

# Check current state
> balance --account ACC001
> transactions --account ACC001 --last 10

# Time travel
> balance-at --account ACC001 --date "2024-01-15"
```

---

Happy coding! 🎉 

*Last updated: After implementing comprehensive Account aggregate tests and fixing Option<T> handling* 