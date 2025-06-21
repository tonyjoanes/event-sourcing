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
- [x] TransactionId, TransactionType, InterestRate, AccountLimits value objects
- [x] Domain services: InterestCalculator, ComplianceChecker

### Event Store Infrastructure
- [x] IEventStore interface - Define event store contract
- [x] RavenDbEventStore implementation - Store events in RavenDB
- [x] Event serialization/deserialization - JSON serialization for events
- [x] AccountRepository - Repository pattern for Account aggregate
- [x] Event versioning and concurrency - Optimistic concurrency control
- [x] DI registration for event store and repositories

### Read Models & Projections
- [x] IReadModelStore interface - Define read model contract
- [x] SqliteReadModelStore implementation - SQLite for read models
- [x] AccountSummaryProjection - Current account state
- [x] TransactionHistoryProjection - Transaction list
- [x] Event handlers for projections - Update read models from events
- [x] EventDispatcher - Route events to appropriate handlers
- [x] DatabaseInitializer - Create tables and indexes
- [x] TransactionEnrichmentService - Enrich transaction data
- [x] DI registration for read models infrastructure

### Testing
- [x] Comprehensive unit tests for Account aggregate (125 tests passing)
- [x] Value object tests (Money, TransactionId, InterestRate, AccountLimits)
- [x] Domain services tests (InterestCalculator, ComplianceChecker)
- [x] Event sourcing reconstruction tests
- [x] Business rule validation tests
- [x] Functional programming pattern tests

### Infrastructure Setup
- [x] .NET 8 solution with multiple projects
- [x] Project references and dependencies
- [x] Docker Compose for RavenDB
- [x] GitHub Actions CI/CD pipeline
- [x] Code coverage and security scanning
- [x] DI configuration for WebApi and ConsoleApp

## 🎯 Next Priority Tasks

### 1. Application Layer - CQRS (High Priority)
- [ ] **Commands** - OpenAccountCommand, DepositCommand, WithdrawCommand, TransferCommand
- [ ] **Queries** - GetAccountSummaryQuery, GetTransactionHistoryQuery, GetBalanceAtQuery
- [ ] **Command handlers** - Handle commands and apply to aggregates
- [ ] **Query handlers** - Handle queries and return read models
- [ ] **Event publishing** - Wire up event publishing from aggregates to EventDispatcher
- [ ] **AccountService** - Application service for account operations

### 2. Web API Endpoints (High Priority)
- [ ] **AccountController** - REST endpoints for account operations
- [ ] **Command endpoints** - POST endpoints for commands
- [ ] **Query endpoints** - GET endpoints for queries
- [ ] **Time travel endpoints** - Historical data queries
- [ ] **OpenAPI documentation** - Swagger/OpenAPI spec
- [ ] **Error handling middleware** - Consistent error responses

### 3. Console Application Features (Medium Priority)
- [ ] **Interactive command interface** - Parse user commands
- [ ] **Demo scenarios runner** - Predefined banking scenarios
- [ ] **Event store browser** - View stored events
- [ ] **Time travel demonstrations** - Show historical state
- [ ] **Colored output** - Better user experience
- [ ] **Command validation** - Input validation and error handling

### 4. Integration & Testing (Medium Priority)
- [ ] **Integration tests** - Test event store and read models together
- [ ] **End-to-end tests** - Complete workflow testing
- [ ] **Event publishing tests** - Test event flow from aggregates to projections
- [ ] **Performance tests** - Load testing for event store and read models

### 5. Advanced Features (Lower Priority)
- [ ] **Snapshotting** - Performance optimization for large event streams
- [ ] **Event schema evolution** - Handle event version changes
- [ ] **Saga pattern** - Complex business processes (transfers between accounts)
- [ ] **Real-time updates** - SignalR for live updates
- [ ] **Analytics projections** - Customer behavior analysis
- [ ] **Compliance reporting** - Audit trail and regulatory reports

## 🧪 Testing Strategy

- **Unit Tests**: Test aggregates and domain logic ✅
- **Integration Tests**: Test event store and read models together
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
- **Command Side**: Optimized for writes, consistency, business rules ✅
- **Query Side**: Optimized for reads, performance, reporting ✅
- **Eventual Consistency**: Read models updated asynchronously ✅
- **Scalability**: Commands and queries can scale independently ✅

## 🔧 Development Tips

1. **Start with Domain**: Focus on business logic first ✅
2. **Event-First Design**: Design events before aggregates ✅
3. **Test-Driven**: Write tests for business rules ✅
4. **CQRS Separation**: Separate read and write concerns ✅
5. **Event-Driven**: Use events to drive read model updates ✅
6. **Small Steps**: Implement one feature at a time

## 🚀 Current Status

**✅ COMPLETED:**
- Complete domain layer with comprehensive business logic
- Full event store infrastructure with RavenDB
- Complete read models and projections system
- 125 passing unit tests
- DI configuration for all components

**🎯 NEXT UP:**
- Application layer (CQRS commands and queries)
- Web API endpoints
- Event publishing from aggregates to projections

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

*Last updated: After implementing complete CQRS read models and projections infrastructure* 