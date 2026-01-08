# Architecture Deep Dive

This document explains the architectural decisions and patterns used in the System Monitoring Platform. Perfect for interview preparation.

## Table of Contents
1. [Clean Architecture Overview](#clean-architecture-overview)
2. [Layer Responsibilities](#layer-responsibilities)
3. [Design Patterns Used](#design-patterns-used)
4. [SOLID Principles in Action](#solid-principles-in-action)
5. [Data Flow](#data-flow)
6. [Why These Choices Matter](#why-these-choices-matter)

---

## Clean Architecture Overview

### The Dependency Rule

**Core Principle**: Dependencies point inward. Inner layers know nothing about outer layers.

```
┌─────────────────────────────────────────┐
│         API Layer (Outermost)           │  ← HTTP, Controllers
│  ┌───────────────────────────────────┐  │
│  │    Infrastructure Layer           │  │  ← Database, External Services
│  │  ┌─────────────────────────────┐  │  │
│  │  │   Application Layer         │  │  │  ← Business Logic, Services
│  │  │  ┌───────────────────────┐  │  │  │
│  │  │  │   Domain Layer        │  │  │  │  ← Entities, Business Rules
│  │  │  │   (Core)              │  │  │  │
│  │  │  └───────────────────────┘  │  │  │
│  │  └─────────────────────────────┘  │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

### Benefits

1. **Testability**: Test business logic without database or HTTP
2. **Flexibility**: Swap implementations easily
3. **Maintainability**: Changes are isolated
4. **Independence**: Business rules don't depend on frameworks

---

## Layer Responsibilities

### Domain Layer (`Monitoring.Domain`)

**Purpose**: Contains the core business entities and rules.

**What it includes:**
- Entities (`SystemMetric`, `KpiResult`, `Anomaly`)
- Enums (`KpiStatus`, `AnomalySeverity`)
- Domain exceptions (if any)

**What it DOESN'T include:**
- Database access code
- HTTP concerns
- External service calls
- Framework dependencies

**Example - SystemMetric Entity:**
```csharp
public class SystemMetric
{
    public int Id { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? Tags { get; set; }
}
```

**Why this matters**: This entity represents a real-world concept. It has no dependencies, making it easy to understand and test.

---

### Application Layer (`Monitoring.Application`)

**Purpose**: Contains business logic and orchestrates the application flow.

**What it includes:**
- Service interfaces (`IMetricService`, `IKpiService`)
- Service implementations (`MetricService`, `KpiService`)
- DTOs (Data Transfer Objects)
- Repository interfaces

**What it DOESN'T include:**
- Database implementation details
- HTTP request/response handling
- Concrete repository implementations

**Example - KpiService Business Logic:**
```csharp
private async Task CalculateAverageResponseTimeKpi(...)
{
    var responseTimeMetrics = metrics
        .Where(m => m.MetricName.Equals("ResponseTime", StringComparison.OrdinalIgnoreCase))
        .ToList();

    if (!responseTimeMetrics.Any()) return;

    var averageResponseTime = responseTimeMetrics.Average(m => m.Value);
    const double targetResponseTime = 200.0;

    var status = DetermineKpiStatus(averageResponseTime, targetResponseTime, isLowerBetter: true);
    
    // Create KPI result...
}
```

**Why this matters**: Business logic is isolated from infrastructure concerns. You can test this without a database.

---

### Infrastructure Layer (`Monitoring.Infrastructure`)

**Purpose**: Implements data access and external service integrations.

**What it includes:**
- DbContext (Entity Framework)
- Repository implementations
- Database migrations
- External API clients (if any)

**What it DOESN'T include:**
- Business logic
- HTTP controllers
- Application-specific rules

**Example - Repository Pattern:**
```csharp
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly MonitoringDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    // Other data access methods...
}
```

**Why this matters**: Database implementation is hidden behind interfaces. Switching from SQL Server to PostgreSQL only requires changes here.

---

### API Layer (`Monitoring.API`)

**Purpose**: Handles HTTP requests and responses.

**What it includes:**
- Controllers
- Middleware
- Configuration (Program.cs)
- API-specific concerns (CORS, Swagger)

**What it DOESN'T include:**
- Business logic
- Data access code
- Domain entities (uses DTOs instead)

**Example - Controller:**
```csharp
[HttpPost]
public async Task<ActionResult<SystemMetricDto>> CreateMetric([FromBody] CreateMetricDto createMetricDto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    var metric = await _metricService.CreateMetricAsync(createMetricDto);
    return CreatedAtAction(nameof(GetMetricById), new { id = metric.Id }, metric);
}
```

**Why this matters**: Controllers are thin. They only handle HTTP concerns and delegate to services.

---

## Design Patterns Used

### 1. Repository Pattern

**Purpose**: Abstracts data access logic.

**Implementation:**
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    // ...
}
```

**Benefits:**
- Centralized data access logic
- Easy to mock for testing
- Can switch database implementations
- Provides collection-like interface

**Interview Talking Point**: "I used the Repository Pattern to abstract database operations. This allows me to test business logic without a real database and makes it easy to switch data sources."

---

### 2. Unit of Work Pattern

**Purpose**: Manages transactions across multiple repositories.

**Implementation:**
```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<SystemMetric> SystemMetrics { get; }
    IRepository<KpiResult> KpiResults { get; }
    IRepository<Anomaly> Anomalies { get; }
    Task<int> SaveChangesAsync();
}
```

**Benefits:**
- Ensures all changes succeed or fail together
- Single transaction boundary
- Prevents partial updates

**Example Usage:**
```csharp
// Multiple operations in one transaction
await _unitOfWork.SystemMetrics.AddAsync(metric);
await _unitOfWork.KpiResults.AddAsync(kpi);
await _unitOfWork.SaveChangesAsync(); // Commits both or neither
```

**Interview Talking Point**: "The Unit of Work pattern ensures data consistency. All related changes are committed in a single transaction."

---

### 3. Dependency Injection

**Purpose**: Loose coupling between components.

**Configuration (Program.cs):**
```csharp
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMetricService, MetricService>();
builder.Services.AddScoped<IKpiService, KpiService>();
```

**Usage in Controller:**
```csharp
public class MetricsController : ControllerBase
{
    private readonly IMetricService _metricService;

    public MetricsController(IMetricService metricService)
    {
        _metricService = metricService;
    }
}
```

**Benefits:**
- Easy to swap implementations
- Facilitates testing with mocks
- Manages object lifetimes
- Follows Dependency Inversion Principle

---

### 4. DTO Pattern

**Purpose**: Separate internal models from API contracts.

**Why not return entities directly?**
```csharp
// ❌ BAD: Exposing entity
public async Task<SystemMetric> GetMetric(int id)
{
    return await _repository.GetByIdAsync(id);
}

// ✅ GOOD: Using DTO
public async Task<SystemMetricDto> GetMetric(int id)
{
    var metric = await _repository.GetByIdAsync(id);
    return MapToDto(metric);
}
```

**Benefits:**
- API contract independent of database schema
- Can add computed properties
- Prevents over-posting attacks
- Cleaner API responses

---

## SOLID Principles in Action

### Single Responsibility Principle (SRP)

**Each class has one reason to change.**

**Example:**
- `MetricsController`: Only handles HTTP requests for metrics
- `MetricService`: Only contains metric business logic
- `Repository<T>`: Only handles data access

**Bad Example (Violation):**
```csharp
// ❌ Controller doing too much
public class MetricsController
{
    public async Task<IActionResult> CreateMetric(CreateMetricDto dto)
    {
        // Validation
        if (string.IsNullOrEmpty(dto.MetricName)) return BadRequest();
        
        // Business logic
        var metric = new SystemMetric { ... };
        
        // Database access
        _context.Metrics.Add(metric);
        await _context.SaveChangesAsync();
        
        return Ok(metric);
    }
}
```

**Good Example:**
```csharp
// ✅ Separated concerns
public class MetricsController
{
    public async Task<IActionResult> CreateMetric(CreateMetricDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var metric = await _metricService.CreateMetricAsync(dto);
        return CreatedAtAction(nameof(GetMetricById), new { id = metric.Id }, metric);
    }
}
```

---

### Open/Closed Principle (OCP)

**Open for extension, closed for modification.**

**Example - Adding New KPI:**

Instead of modifying `KpiService`, you can extend it:

```csharp
// Current implementation
public class KpiService : IKpiService
{
    public async Task CalculateKpisAsync()
    {
        await CalculateAverageResponseTimeKpi(...);
        await CalculateThroughputKpi(...);
        // Easy to add new KPI calculations here
    }
}

// Future: Could use strategy pattern
public interface IKpiCalculator
{
    Task<KpiResult> CalculateAsync(IEnumerable<SystemMetric> metrics);
}

public class ResponseTimeKpiCalculator : IKpiCalculator { ... }
public class ThroughputKpiCalculator : IKpiCalculator { ... }
```

---

### Liskov Substitution Principle (LSP)

**Subtypes must be substitutable for their base types.**

**Example:**
```csharp
// Any implementation of IRepository<T> can be used
IRepository<SystemMetric> repo = new Repository<SystemMetric>(context);
// OR
IRepository<SystemMetric> repo = new CachedRepository<SystemMetric>(context, cache);
// OR
IRepository<SystemMetric> repo = new MockRepository<SystemMetric>();
```

All implementations honor the contract defined by `IRepository<T>`.

---

### Interface Segregation Principle (ISP)

**Clients shouldn't depend on interfaces they don't use.**

**Example:**

Instead of one large interface:
```csharp
// ❌ BAD: Fat interface
public interface IDataService
{
    Task<SystemMetric> GetMetric(int id);
    Task<KpiResult> GetKpi(int id);
    Task<Anomaly> GetAnomaly(int id);
    Task CalculateKpis();
    Task DetectAnomalies();
    // Too many responsibilities
}
```

We have specific interfaces:
```csharp
// ✅ GOOD: Segregated interfaces
public interface IMetricService { ... }
public interface IKpiService { ... }
public interface IAnomalyService { ... }
```

---

### Dependency Inversion Principle (DIP)

**Depend on abstractions, not concretions.**

**Example:**
```csharp
// ❌ BAD: Depending on concrete class
public class MetricsController
{
    private readonly MetricService _service; // Concrete class
    
    public MetricsController()
    {
        _service = new MetricService(); // Tight coupling
    }
}

// ✅ GOOD: Depending on abstraction
public class MetricsController
{
    private readonly IMetricService _service; // Interface
    
    public MetricsController(IMetricService service) // Injected
    {
        _service = service;
    }
}
```

---

## Data Flow

### Creating a Metric (End-to-End)

```
1. HTTP Request
   ↓
2. MetricsController.CreateMetric()
   - Validates ModelState
   - Calls service
   ↓
3. MetricService.CreateMetricAsync()
   - Creates domain entity
   - Calls repository
   ↓
4. Repository.AddAsync()
   - Adds to DbSet
   ↓
5. UnitOfWork.SaveChangesAsync()
   - Commits transaction
   ↓
6. MetricService maps to DTO
   ↓
7. Controller returns 201 Created
```

**Key Points:**
- Each layer has a specific responsibility
- Data flows through clear boundaries
- Business logic is in the service layer
- Database details are hidden in infrastructure

---

## Why These Choices Matter

### For Interviews

**Question**: "Why did you use Clean Architecture?"

**Answer**: "I chose Clean Architecture because it provides clear separation of concerns and makes the codebase maintainable and testable. The business logic in the Application layer doesn't depend on the database or UI framework, so I can test it in isolation. If requirements change—like switching from SQL Server to MongoDB—I only need to modify the Infrastructure layer. This architecture also makes the code easier for teams to work on, as each layer has well-defined responsibilities."

---

**Question**: "What design patterns did you use and why?"

**Answer**: "I used several patterns:
- **Repository Pattern** to abstract data access, making it easy to test and swap databases
- **Unit of Work** to manage transactions across multiple operations
- **Dependency Injection** for loose coupling and testability
- **DTO Pattern** to separate API contracts from domain models

These patterns follow SOLID principles and industry best practices."

---

**Question**: "How would you add a new feature?"

**Answer**: "Let's say I need to add user authentication. I would:
1. Add a `User` entity in the Domain layer
2. Create `IUserService` interface in Application layer
3. Implement `UserService` with authentication logic
4. Add `UserRepository` in Infrastructure layer
5. Create `AuthController` in API layer
6. Register services in dependency injection

The existing code wouldn't need to change—I'm extending, not modifying."

---

### For Code Quality

1. **Testability**: Can test each layer independently
2. **Maintainability**: Clear structure, easy to navigate
3. **Scalability**: Can add features without breaking existing code
4. **Team Collaboration**: Different developers can work on different layers
5. **Technology Independence**: Can swap frameworks/databases easily

---

## Common Mistakes Avoided

### ❌ Mistake 1: Business Logic in Controllers
```csharp
// BAD
public async Task<IActionResult> CreateMetric(CreateMetricDto dto)
{
    var average = metrics.Average(m => m.Value); // Business logic here
    if (average > 200) { ... }
}
```

### ✅ Solution: Logic in Service Layer
```csharp
// GOOD
public async Task<IActionResult> CreateMetric(CreateMetricDto dto)
{
    var metric = await _metricService.CreateMetricAsync(dto);
    return CreatedAtAction(...);
}
```

---

### ❌ Mistake 2: Direct Database Access in Controllers
```csharp
// BAD
public class MetricsController
{
    private readonly MonitoringDbContext _context;
    
    public async Task<IActionResult> GetMetrics()
    {
        return Ok(await _context.SystemMetrics.ToListAsync());
    }
}
```

### ✅ Solution: Use Services and Repositories
```csharp
// GOOD
public class MetricsController
{
    private readonly IMetricService _service;
    
    public async Task<IActionResult> GetMetrics()
    {
        var metrics = await _service.GetMetricsAsync(query);
        return Ok(metrics);
    }
}
```

---

## Further Learning

- **Clean Architecture** by Robert C. Martin
- **Domain-Driven Design** by Eric Evans
- **Patterns of Enterprise Application Architecture** by Martin Fowler
- **Dependency Injection Principles, Practices, and Patterns** by Steven van Deursen

---

**Remember**: Architecture is about making the right trade-offs for your project's needs. This architecture works well for medium to large applications where maintainability and testability are priorities.
