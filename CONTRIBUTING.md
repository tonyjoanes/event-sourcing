# Contributing to Event Sourcing Banking Demo

Thank you for your interest in contributing to this Event Sourcing Banking Demo! This project serves as an educational resource and reference implementation for event sourcing patterns in .NET.

## 🎯 Project Goals

This project demonstrates:
- **Event Sourcing** fundamentals with real-world banking scenarios
- **CQRS** (Command Query Responsibility Segregation) patterns
- **Time travel** queries and historical analysis
- **Functional programming** patterns in C#
- **Production-ready** code quality and testing practices

## 🚀 Getting Started

### Prerequisites

- **.NET 8 SDK** or later
- **Docker** (for RavenDB)
- **Git**
- **IDE**: Visual Studio, VS Code, or Rider

### Development Setup

1. **Fork and Clone**
   ```bash
   git clone https://github.com/YOUR_USERNAME/event-sourcing.git
   cd event-sourcing
   ```

2. **Install Dependencies**
   ```bash
   dotnet restore
   ```

3. **Start RavenDB**
   ```bash
   docker-compose up -d ravendb
   ```

4. **Run Tests**
   ```bash
   dotnet test
   ```

5. **Start the Application**
   ```bash
   # Console App
   dotnet run --project src/ConsoleApp
   
   # Web API
   dotnet run --project src/WebApi
   ```

## 🔄 Development Workflow

### Branch Strategy

We use **GitHub Flow** with branch protection:

```
main (protected)
├── feature/add-account-limits
├── bugfix/fix-transfer-validation  
├── docs/update-api-documentation
└── refactor/improve-projection-performance
```

### Branch Naming Convention

- **Features**: `feature/description-of-feature`
- **Bug Fixes**: `bugfix/description-of-fix`
- **Documentation**: `docs/description-of-change`
- **Refactoring**: `refactor/description-of-change`
- **Performance**: `perf/description-of-improvement`

### Step-by-Step Workflow

1. **Create Feature Branch**
   ```bash
   git checkout main
   git pull origin main
   git checkout -b feature/your-feature-name
   ```

2. **Make Changes**
   - Write code following our [coding standards](#coding-standards)
   - Add tests for new functionality
   - Update documentation as needed

3. **Test Locally**
   ```bash
   # Run all tests
   dotnet test
   
   # Check code formatting
   dotnet format --verify-no-changes
   
   # Build in release mode
   dotnet build --configuration Release
   ```

4. **Commit Changes**
   ```bash
   git add .
   git commit -m "feat: add account overdraft limits"
   ```

5. **Push and Create PR**
   ```bash
   git push origin feature/your-feature-name
   ```
   Then create a Pull Request using our [PR template](.github/pull_request_template.md)

## 📋 Quality Standards

### ✅ Quality Gates

All PRs must pass these automated checks:

1. **Code Formatting**: `dotnet format --verify-no-changes`
2. **Build**: No compilation errors or warnings
3. **Tests**: All unit and integration tests pass
4. **Coverage**: Minimum 60% code coverage
5. **Security**: No vulnerable dependencies
6. **Documentation**: XML docs for public APIs

### 🧪 Testing Requirements

#### Test Categories
- **Unit Tests**: Fast, isolated tests for business logic
- **Integration Tests**: End-to-end testing with real dependencies
- **Event Sourcing Tests**: Verify event replay and projection behavior

#### Test Structure
```csharp
[Fact]
public async Task DepositMoney_ShouldUpdateBalance_AndCreateEvent()
{
    // Arrange
    var account = CreateTestAccount();
    var depositAmount = Money.Create(100, "USD");
    
    // Act
    var result = account.Deposit(depositAmount, "Test deposit");
    
    // Assert
    result.Should().BeSuccessful();
    account.Balance.Should().Be(Money.Create(1100, "USD"));
    account.UncommittedEvents.Should().ContainSingle()
        .Which.Should().BeOfType<MoneyDeposited>();
}
```

#### Coverage Requirements
- **Minimum Overall**: 60%
- **Domain Logic**: 80%+
- **Critical Paths**: 100%

### 🏗️ Coding Standards

#### Event Sourcing Patterns

1. **Events are Immutable**
   ```csharp
   public record MoneyDeposited : BaseEvent
   {
       public required Money Amount { get; init; }
       public required string Description { get; init; }
   }
   ```

2. **Aggregates Control Invariants**
   ```csharp
   public class Account : AggregateRoot
   {
       public Result<Unit, ValidationError> Withdraw(Money amount, string description)
       {
           if (amount.Amount <= 0)
               return ValidationError.Create("Amount must be positive");
               
           if (Balance.Amount < amount.Amount)
               return ValidationError.Create("Insufficient funds");
               
           Apply(new MoneyWithdrawn(Id, amount, description));
           return Unit.Value;
       }
   }
   ```

3. **Projections are Eventually Consistent**
   ```csharp
   public async Task HandleAsync(MoneyDeposited @event)
   {
       var summary = await _store.GetByIdAsync<AccountSummary>(@event.AccountId);
       summary.Balance += @event.Amount.Amount;
       await _store.UpdateAsync(summary);
   }
   ```

#### C# Conventions

- **Naming**: PascalCase for public members, camelCase for private
- **Async**: All async methods end with `Async`
- **Nullability**: Enable nullable reference types
- **Records**: Use for immutable data (events, value objects)
- **Classes**: Use for mutable entities (aggregates)

#### API Design

- **RESTful**: Follow REST conventions
- **Documentation**: XML comments for all public APIs
- **Validation**: Use data annotations and FluentValidation
- **Error Handling**: Consistent error responses

```csharp
/// <summary>
/// Deposits money into an account
/// </summary>
/// <param name="request">Deposit details</param>
/// <returns>Updated account balance</returns>
[HttpPost("deposit")]
public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
```

## 🔍 Event Sourcing Guidelines

### Event Design

1. **Events Represent Facts**
   - Use past tense: `MoneyDeposited`, not `DepositMoney`
   - Include all necessary data for projections
   - Never modify existing event schemas

2. **Event Versioning**
   ```csharp
   // V1
   public record AccountOpened : BaseEvent
   {
       public CustomerId CustomerId { get; init; }
       public Money InitialBalance { get; init; }
   }
   
   // V2 - Add optional field
   public record AccountOpened : BaseEvent  
   {
       public CustomerId CustomerId { get; init; }
       public Money InitialBalance { get; init; }
       public AccountType? AccountType { get; init; } // New optional
   }
   ```

### Projection Design

1. **Purpose-Built Read Models**
   ```csharp
   public class AccountSummaryProjection
   {
       public string Id { get; set; }
       public decimal Balance { get; set; }
       public string Status { get; set; }
       public DateTimeOffset LastTransactionAt { get; set; }
   }
   ```

2. **Idempotent Handlers**
   ```csharp
   public async Task HandleAsync(MoneyDeposited @event)
   {
       // Check if already processed
       var existing = await _store.GetTransactionAsync(@event.Id);
       if (existing != null) return;
       
       // Process event
       await ProcessDeposit(@event);
   }
   ```

## 📚 Documentation Standards

### Code Documentation
- **XML Comments**: All public APIs
- **Complex Logic**: Inline comments explaining why, not what
- **Event Sourcing**: Document event schemas and projection logic

### API Documentation
- **Swagger**: Comprehensive API documentation
- **Examples**: Request/response examples
- **Error Codes**: Document all possible error responses

### Architecture Documentation
- **ADRs**: Architecture Decision Records for major decisions
- **Diagrams**: Keep Mermaid diagrams updated
- **README**: Keep getting started guide current

## 🚨 Pull Request Process

### Before Submitting

1. **Self Review**
   - [ ] Code follows project standards
   - [ ] Tests added/updated
   - [ ] Documentation updated
   - [ ] No debugging code left

2. **Local Testing**
   ```bash
   # Format code
   dotnet format
   
   # Run full test suite
   dotnet test
   
   # Check coverage
   dotnet test --collect:"XPlat Code Coverage"
   
   # Build release
   dotnet build --configuration Release
   ```

### PR Requirements

- **Title**: Clear, descriptive title
- **Description**: Use the PR template
- **Size**: Keep PRs focused and reviewable (< 500 lines)
- **Tests**: Include relevant tests
- **Documentation**: Update docs if needed

### Review Process

1. **Automated Checks**: All CI checks must pass
2. **Code Review**: At least one approving review required
3. **Event Sourcing Review**: Special attention to ES patterns
4. **Documentation Review**: Ensure docs are updated

### Merge Requirements

- ✅ All CI checks pass
- ✅ At least 1 approving review
- ✅ No unresolved conversations
- ✅ Up to date with main branch
- ✅ Squash merge to main

## 🐛 Bug Reports

Use our [bug report template](.github/ISSUE_TEMPLATE/bug_report.md) and include:

- **Clear reproduction steps**
- **Expected vs actual behavior**
- **Environment details**
- **Event sourcing context** (if applicable)
- **Error logs and stack traces**

## ✨ Feature Requests

Use our [feature request template](.github/ISSUE_TEMPLATE/feature_request.md) and consider:

- **Event sourcing impact**
- **API design implications**
- **Testing strategy**
- **Documentation needs**

## 🎓 Learning Resources

### Event Sourcing
- [Martin Fowler - Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Event Store Documentation](https://eventstore.com/docs/)
- [CQRS Journey](https://docs.microsoft.com/en-us/previous-versions/msp-n-p/jj554200(v=pandp.10))

### .NET Patterns
- [.NET Application Architecture Guides](https://dotnet.microsoft.com/learn/dotnet/architecture-guides)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## 🤝 Code of Conduct

- **Be Respectful**: Treat everyone with respect
- **Be Constructive**: Provide helpful feedback
- **Be Patient**: Remember this is a learning project
- **Be Inclusive**: Welcome contributors of all skill levels

## 📞 Getting Help

- **GitHub Discussions**: For questions and discussions
- **Issues**: For bugs and feature requests
- **Code Review**: For implementation guidance

---

Thank you for contributing to the Event Sourcing Banking Demo! Your contributions help make this a better learning resource for the community. 🎉 