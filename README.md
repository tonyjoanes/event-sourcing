# Event Sourcing Banking Demo

[![Build and Test](https://github.com/tonyjoanes/event-sourcing/actions/workflows/build.yml/badge.svg)](https://github.com/tonyjoanes/event-sourcing/actions/workflows/build.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![codecov](https://codecov.io/gh/tonyjoanes/event-sourcing/branch/main/graph/badge.svg)](https://codecov.io/gh/tonyjoanes/event-sourcing)

A practical demonstration of Event Sourcing patterns using C#, showcasing how to build a robust banking system with complete audit trails, time travel capabilities, and powerful analytics.

## 🎯 Project Goals

This project demonstrates:
- **Event Sourcing** fundamentals with real-world banking scenarios
- **CQRS** (Command Query Responsibility Segregation) pattern
- **Functional programming** patterns using OneOf library
- **Time travel** queries and historical analysis
- **Immutable audit trails** for compliance
- **Event-driven architecture** principles

## 🏗️ Architecture Overview

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Console App   │    │   REST API       │    │  Event Handlers │
│   (Commands)    │───▶│  (HTTP Layer)    │───▶│  (Projections)  │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                │                        │
                                ▼                        ▼
                       ┌──────────────────┐    ┌─────────────────┐
                       │   Event Store    │    │   Read Models   │
                       │   (RavenDB)      │    │   (SQLite)      │
                       └──────────────────┘    └─────────────────┘
```

## 🔧 Technology Stack

- **Language**: C# 12 / .NET 8
- **Event Store**: RavenDB (document database optimized for event storage)
- **Read Models**: SQLite (lightweight relational database)
- **Functional Patterns**: OneOf library for unions and Result types
- **Testing**: xUnit with FluentAssertions
- **API**: ASP.NET Core Minimal APIs
- **Logging**: Serilog with structured logging

## ✨ Key Features

This banking demo showcases a complete event sourcing implementation with:

**Core Banking Operations**
- Account management with full event sourcing
- Deposits, withdrawals, and transfers
- Overdraft handling and business rule enforcement
- Interest calculation and fee processing
- Account lifecycle management (freezing/closing)

**Event Sourcing Infrastructure**
- Robust event store abstraction with RavenDB implementation
- Complete event serialization and versioning support
- Aggregate repository pattern with optimistic concurrency control

**Functional Programming Patterns**
- OneOf unions for type-safe command results
- Result<T, Error> pattern for operation outcomes
- Option<T> for handling optional values
- Immutable event types and pure business logic functions

**CQRS with Read Models**
- Optimized projections for account summaries and transaction history
- Real-time read model updates via event handlers
- Separate query models for analytics and compliance reporting

**Time Travel & Analytics**
- Point-in-time balance queries and historical analysis
- Customer behavior profiling and spending pattern detection
- Comprehensive audit trails for compliance requirements

**Interactive Experiences**
- Rich console application with demo scenarios
- Full REST API with OpenAPI documentation
- Time travel demonstrations and analytics reporting

## 🗂️ Project Structure

```
EventSourcingBankingDemo/
├── src/
│   ├── Domain/
│   │   ├── Aggregates/
│   │   │   ├── Account.cs
│   │   │   └── AggregateRoot.cs
│   │   ├── Events/
│   │   │   ├── AccountEvents.cs
│   │   │   └── BaseEvent.cs
│   │   ├── ValueObjects/
│   │   │   ├── Money.cs
│   │   │   ├── AccountId.cs
│   │   │   └── CustomerId.cs
│   │   └── Services/
│   │       ├── InterestCalculator.cs
│   │       └── ComplianceChecker.cs
│   ├── Infrastructure/
│   │   ├── EventStore/
│   │   │   ├── IEventStore.cs
│   │   │   ├── RavenDbEventStore.cs
│   │   │   └── EventStoreConfiguration.cs
│   │   ├── ReadModels/
│   │   │   ├── IReadModelStore.cs
│   │   │   ├── SqliteReadModelStore.cs
│   │   │   └── Projections/
│   │   │       ├── AccountSummaryProjection.cs
│   │   │       ├── TransactionHistoryProjection.cs
│   │   │       └── AnalyticsProjection.cs
│   │   └── Repositories/
│   │       └── AccountRepository.cs
│   ├── Application/
│   │   ├── Commands/
│   │   │   ├── OpenAccountCommand.cs
│   │   │   ├── DepositCommand.cs
│   │   │   ├── WithdrawCommand.cs
│   │   │   └── TransferCommand.cs
│   │   ├── Queries/
│   │   │   ├── GetAccountSummaryQuery.cs
│   │   │   ├── GetTransactionHistoryQuery.cs
│   │   │   └── GetBalanceAtQuery.cs
│   │   ├── Handlers/
│   │   │   ├── CommandHandlers.cs
│   │   │   ├── QueryHandlers.cs
│   │   │   └── EventHandlers.cs
│   │   └── Services/
│   │       ├── AccountService.cs
│   │       ├── AnalyticsService.cs
│   │       └── ComplianceService.cs
│   ├── ConsoleApp/
│   │   ├── Program.cs
│   │   ├── CommandParser.cs
│   │   ├── DemoScenarios.cs
│   │   └── ConsoleUI.cs
│   └── WebApi/
│       ├── Program.cs
│       ├── Controllers/
│       │   ├── AccountController.cs
│       │   ├── AnalyticsController.cs
│       │   └── ComplianceController.cs
│       └── Middleware/
│           └── ExceptionHandlingMiddleware.cs
├── tests/
│   ├── Domain.Tests/
│   ├── Infrastructure.Tests/
│   ├── Application.Tests/
│   └── Integration.Tests/
├── docs/
│   ├── EventStorming.md
│   ├── EventCatalog.md
│   └── ArchitectureDecisions.md
└── docker/
    ├── docker-compose.yml
    ├── ravendb/
    └── sqlite/
```

## 🎮 Demo Scenarios

### Scenario 1: Basic Banking Operations
```bash
# Open account
> open-account --customer "John Doe" --initial-balance 1000

# Make transactions
> deposit --account ACC001 --amount 500 --description "Salary"
> withdraw --account ACC001 --amount 200 --description "ATM Withdrawal"
> transfer --from ACC001 --to ACC002 --amount 150 --description "Rent Payment"

# Check current state
> balance --account ACC001
> transactions --account ACC001 --last 10
```

### Scenario 2: Time Travel Demonstration
```bash
# Show balance at different points in time
> balance-at --account ACC001 --date "2024-01-15"
> balance-at --account ACC001 --date "2024-06-01"

# Show transaction history for specific period
> transactions --account ACC001 --from "2024-01-01" --to "2024-03-31"

# Explain how we got to current state
> explain-balance --account ACC001
```

### Scenario 3: Analytics & Compliance
```bash
# Customer behavior analysis
> analyze-customer --account ACC001

# Overdraft analysis
> overdraft-analysis --account ACC001

# Compliance report
> compliance-report --account ACC001 --period "2024-Q1"

# Audit trail
> audit-trail --account ACC001 --from "2024-01-01"
```

### Scenario 4: System Debugging
```bash
# Browse event store
> events --account ACC001
> events --account ACC001 --type "MoneyWithdrawn"

# Compare event store vs read model
> compare-state --account ACC001

# Replay events (rebuild read model)
> replay-events --account ACC001
```

## 🏃‍♂️ Getting Started

### Prerequisites
- .NET 8 SDK
- Docker (for RavenDB)
- Visual Studio Code or Cursor IDE

### Setup Instructions

1. **Clone and setup**
   ```bash
   git clone <repository-url>
   cd EventSourcingBankingDemo
   dotnet restore
   ```

2. **Start RavenDB**
   ```bash
   docker-compose up -d ravendb
   ```

3. **Run migrations**
   ```bash
   dotnet run --project src/ConsoleApp -- migrate
   ```

4. **Run console demo**
   ```bash
   dotnet run --project src/ConsoleApp
   ```

5. **Run web API**
   ```bash
   dotnet run --project src/WebApi
   # API available at https://localhost:5001
   # Swagger UI at https://localhost:5001/swagger
   ```

### Quick Demo
```bash
# Run predefined demo scenarios
dotnet run --project src/ConsoleApp -- demo basic-banking
dotnet run --project src/ConsoleApp -- demo time-travel
dotnet run --project src/ConsoleApp -- demo analytics
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test categories
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
dotnet test --filter Category=Performance

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Key Concepts Demonstrated

### Event Sourcing Benefits
- **Audit Trail**: Complete history of all account changes
- **Time Travel**: Query account state at any point in time
- **Debugging**: Replay events to understand system behavior
- **Analytics**: Rich data for business intelligence
- **Compliance**: Immutable records for regulatory requirements

### Functional Programming Patterns
- **Union Types**: `OneOf<Success, InsufficientFunds, AccountFrozen>`
- **Result Types**: `Result<Account, ValidationError>`
- **Option Types**: `Option<Customer>` for optional data
- **Immutability**: Events and value objects are immutable
- **Pure Functions**: Business logic without side effects

### CQRS Implementation
- **Command Side**: Optimized for writes, consistency, business rules
- **Query Side**: Optimized for reads, performance, reporting
- **Eventual Consistency**: Read models updated asynchronously
- **Scalability**: Commands and queries can scale independently

## 🎯 Blog Post Integration

This codebase supports a comprehensive blog post series:

1. **"Event Sourcing Fundamentals"** - Use basic banking operations
2. **"Time Travel with Event Sourcing"** - Demonstrate historical queries
3. **"Functional Patterns in C#"** - Show OneOf and Result usage
4. **"Building Audit Trails"** - Compliance and debugging scenarios
5. **"Event-Driven Analytics"** - Customer behavior analysis
6. **"CQRS in Practice"** - Command/query separation

Each scenario in the console app can be directly used as blog post examples with copy-pasteable code snippets.

## 🚀 Future Enhancements

- [ ] Event schema evolution examples
- [ ] Snapshotting for performance optimization
- [ ] Saga pattern for complex business processes
- [ ] Event sourcing with microservices
- [ ] GraphQL API for flexible querying
- [ ] Real-time event streaming with SignalR
- [ ] Advanced analytics with ML.NET

## 📝 Contributing

This is a demo project for educational purposes. Feel free to:
- Add new banking features
- Implement additional analytics
- Improve the functional programming patterns
- Add more comprehensive tests
- Enhance the console UI experience

## 📖 References

- [Event Sourcing Pattern](https://martinfowler.com/eaaDev/EventSourcing.html)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [OneOf Library](https://github.com/mcintyre321/OneOf)
- [RavenDB Documentation](https://ravendb.net/docs/)
- [Functional Programming in C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/functional-programming-vs-imperative-programming)

---

*This project demonstrates event sourcing concepts for educational purposes and should not be used as-is for production banking systems without proper security, compliance, and regulatory considerations.*