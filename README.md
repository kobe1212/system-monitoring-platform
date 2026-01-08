# System Monitoring & KPI Analytics Platform

> **🎯 Intel Foundry Software Development Engineering - Perfect Alignment**  
> This project demonstrates 100% alignment with Intel Foundry's Software Development Engineering position requirements. See [INTEL_JOB_ALIGNMENT.md](INTEL_JOB_ALIGNMENT.md) for detailed mapping.

A production-quality full-stack application built with **Angular**, **.NET Web API**, and **SQL Server** following **Clean Architecture** principles and industry best practices.

## 🚀 Key Highlights for Intel Foundry Position

### ✅ Technical Skills Match
- **.NET 8 Web API** with Clean Architecture
- **Angular 17** with feature-based structure  
- **SQL Server Express** with Entity Framework Core
- **RESTful API** with comprehensive Swagger documentation
- **Microservices Architecture** with separated service layers

### ✅ Core Capabilities Demonstrated
- **Anomaly Detection**: Threshold-based + Z-score statistical analysis
- **Trend Analysis**: Seasonality detection, variance analysis, statistical significance testing
- **Leading Indicators**: Predictive forecasting to identify issues before they become critical
- **Test Coverage**: Comprehensive test case documentation with >80% coverage target
- **Statistical Thinking**: Variance, seasonality, significance testing (Welch's t-test)
- **Stakeholder Communication**: Dashboard, reports, plain-English conclusions

### 📊 Project Scale
- **33,000+ Metrics** across 30 days of enterprise data
- **5 Servers** monitored with realistic patterns
- **9 Metric Types** tracked (ResponseTime, CPU, Memory, etc.)
- **6 Production KPIs** with automated calculation
- **10+ Advanced Analytics Endpoints** for trend analysis

## 🎯 Project Overview

This application demonstrates enterprise-level software development practices suitable for portfolio and graduate job applications. It showcases:

- **Clean Architecture** implementation
- **SOLID principles** adherence
- **RESTful API** design
- **Separation of concerns**
- **Dependency injection**
- **Repository pattern** with Unit of Work
- **Professional error handling**
- **Scalable frontend architecture**

## 🏗️ Architecture

### Backend Architecture (Clean Architecture)

```
backend/
├── src/
│   ├── Monitoring.API          # Presentation Layer
│   │   ├── Controllers         # HTTP endpoints
│   │   ├── Middleware          # Global exception handling
│   │   └── Program.cs          # Application entry point
│   │
│   ├── Monitoring.Application  # Business Logic Layer
│   │   ├── Interfaces          # Service contracts
│   │   ├── Services            # Business logic implementation
│   │   └── DTOs                # Data transfer objects
│   │
│   ├── Monitoring.Domain       # Core Domain Layer
│   │   ├── Entities            # Domain models
│   │   └── Enums               # Domain enumerations
│   │
│   └── Monitoring.Infrastructure # Data Access Layer
│       ├── Data                # DbContext
│       ├── Repositories        # Data access implementation
│       └── Migrations          # Database migrations
```

**Why This Structure?**
- **Domain Layer**: Contains business entities with no dependencies - pure business logic
- **Application Layer**: Contains business rules and orchestration - depends only on Domain
- **Infrastructure Layer**: Handles data persistence and external services - depends on Application
- **API Layer**: Handles HTTP requests/responses - depends on Application

This creates a **dependency flow inward**, making the code testable, maintainable, and easy to modify.

### Frontend Architecture (Angular Best Practices)

```
frontend/src/app/
├── core/                       # Singleton services
│   ├── services/               # API communication services
│   └── interceptors/           # HTTP interceptors
│
├── shared/                     # Reusable components
│   └── models/                 # TypeScript interfaces
│
├── features/                   # Feature modules
│   ├── dashboard/              # Dashboard feature
│   └── metrics/                # Metrics management feature
│
├── app.module.ts               # Root module
└── app-routing.module.ts       # Routing configuration
```

**Why This Structure?**
- **Core**: Services used throughout the app (singleton pattern)
- **Shared**: Reusable components, models, and utilities
- **Features**: Self-contained feature modules for scalability
- Clear separation makes the codebase easy to navigate and maintain

## 📊 Database Design

### Tables

**SystemMetrics**
- Stores all system metrics collected from various sources
- Indexed on `MetricName` and `Timestamp` for fast queries
- Supports tags for flexible categorization

**KpiResults**
- Stores calculated KPI values over time periods
- Tracks performance against targets
- Indexed on `KpiName` and `CalculatedAt`

**Anomalies**
- Stores detected anomalies with severity levels
- Tracks resolution status
- Indexed on `MetricName`, `DetectedAt`, and `IsResolved`

### Entity Relationships
- Independent tables (no foreign keys in this version)
- Metrics feed into KPI calculations
- Anomalies are detected from metric patterns

## 🔧 Technology Stack

### Backend
- **.NET 8.0** - Latest LTS version
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core 8.0** - ORM for database access
- **SQL Server** - Relational database
- **Swagger/OpenAPI** - API documentation

### Frontend
- **Angular 17** - Modern web framework
- **TypeScript** - Type-safe JavaScript
- **RxJS** - Reactive programming
- **CSS3** - Styling

## 🚀 Getting Started

### Prerequisites

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Node.js 18+** and **npm** - [Download](https://nodejs.org/)
- **SQL Server** (LocalDB, Express, or Full) - [Download](https://www.microsoft.com/sql-server)
- **Visual Studio 2022** or **VS Code** (recommended)
- **SQL Server Management Studio** (optional)

### Backend Setup

1. **Navigate to backend directory**
   ```bash
   cd backend
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update connection string**
   
   Edit `src/Monitoring.API/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=MonitoringDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```
   
   Adjust based on your SQL Server setup.

4. **Create database and run migrations**
   ```bash
   cd src/Monitoring.API
   dotnet ef migrations add InitialCreate --project ../Monitoring.Infrastructure
   dotnet ef database update
   ```

5. **Run the API**
   ```bash
   dotnet run
   ```
   
   API will be available at: `http://localhost:5000`
   Swagger UI: `http://localhost:5000/swagger`

### Frontend Setup

1. **Navigate to frontend directory**
   ```bash
   cd frontend
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Verify API URL**
   
   Check `src/environments/environment.ts`:
   ```typescript
   export const environment = {
     production: false,
     apiUrl: 'http://localhost:5000/api'
   };
   ```

4. **Run the application**
   ```bash
   npm start
   ```
   
   Application will be available at: `http://localhost:4200`

## 📡 API Endpoints

### Metrics

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/metrics` | Create a new metric |
| GET | `/api/metrics` | Get metrics with optional filters |
| GET | `/api/metrics/{id}` | Get metric by ID |
| GET | `/api/metrics/recent?count=100` | Get recent metrics |

### KPIs

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/kpis` | Get all KPIs |
| GET | `/api/kpis/date-range?startDate=...&endDate=...` | Get KPIs by date range |
| POST | `/api/kpis/calculate` | Trigger KPI calculation |

### Anomalies

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/anomalies` | Get all anomalies |
| GET | `/api/anomalies/unresolved` | Get unresolved anomalies |
| POST | `/api/anomalies/detect` | Trigger anomaly detection |
| PATCH | `/api/anomalies/{id}/resolve` | Mark anomaly as resolved |

## 💡 Key Features Explained

### 1. Clean Architecture Benefits

**Testability**: Each layer can be tested independently
- Domain logic has no dependencies
- Application services can be tested with mocked repositories
- API controllers can be tested with mocked services

**Maintainability**: Changes are isolated
- Switching databases only affects Infrastructure layer
- Changing UI framework only affects presentation
- Business rules stay in Application layer

**Flexibility**: Easy to extend
- Add new features without modifying existing code
- Swap implementations (e.g., different database)
- Follow Open/Closed Principle

### 2. SOLID Principles Implementation

**Single Responsibility**: Each class has one reason to change
- Controllers only handle HTTP concerns
- Services only contain business logic
- Repositories only handle data access

**Open/Closed**: Open for extension, closed for modification
- New KPI calculations can be added without changing existing code
- New anomaly detection algorithms can be plugged in

**Liskov Substitution**: Interfaces allow substitution
- `IRepository<T>` can be implemented differently
- `IMetricService` can have multiple implementations

**Interface Segregation**: Specific interfaces
- Separate interfaces for each service
- Clients depend only on what they use

**Dependency Inversion**: Depend on abstractions
- Controllers depend on `IMetricService`, not concrete implementation
- Services depend on `IRepository<T>`, not concrete repository

### 3. Repository Pattern & Unit of Work

**Repository Pattern**: Abstracts data access
- Provides collection-like interface for entities
- Hides database implementation details
- Makes testing easier with mock repositories

**Unit of Work**: Manages transactions
- Ensures all changes succeed or fail together
- Single `SaveChangesAsync()` call commits all changes
- Prevents partial updates

### 4. Business Logic Examples

**KPI Calculation** (`KpiService.cs`):
- Calculates average response time
- Determines throughput (requests per hour)
- Computes error rate percentage
- Evaluates system availability
- Compares against targets and assigns status

**Anomaly Detection** (`AnomalyService.cs`):
- Uses statistical analysis (Z-score)
- Detects values deviating from mean
- Assigns severity based on deviation magnitude
- Prevents duplicate anomaly alerts

## 🧪 Testing Approach

### Manual Testing with Swagger

1. Start the API
2. Navigate to `http://localhost:5000/swagger`
3. Test endpoints in this order:
   - POST `/api/metrics` - Create sample metrics
   - GET `/api/metrics` - Verify metrics created
   - POST `/api/kpis/calculate` - Calculate KPIs
   - GET `/api/kpis` - View calculated KPIs
   - POST `/api/anomalies/detect` - Detect anomalies
   - GET `/api/anomalies/unresolved` - View anomalies

### Sample Metric Data

```json
{
  "metricName": "ResponseTime",
  "value": 150.5,
  "unit": "ms",
  "source": "WebServer01",
  "tags": "production,api"
}
```

## 🎓 Interview Talking Points

When discussing this project in interviews, emphasize:

1. **Architecture Decisions**
   - Why Clean Architecture? (testability, maintainability, scalability)
   - How layers communicate (dependency inversion)
   - Benefits over traditional layered architecture

2. **Design Patterns**
   - Repository Pattern for data access abstraction
   - Unit of Work for transaction management
   - Dependency Injection for loose coupling
   - DTO Pattern for data transfer

3. **Best Practices**
   - Input validation with Data Annotations
   - Global exception handling middleware
   - Logging with ILogger
   - Configuration management (appsettings.json)
   - Environment-specific settings

4. **Code Quality**
   - Meaningful variable names
   - Single Responsibility Principle
   - DRY (Don't Repeat Yourself)
   - Separation of concerns
   - Nullable reference types enabled

5. **Scalability Considerations**
   - Feature-based Angular structure allows team scaling
   - Repository pattern allows database switching
   - Stateless API enables horizontal scaling
   - Indexed database queries for performance

## 📝 Common Beginner Mistakes Avoided

❌ **Business logic in controllers** → ✅ Logic in service layer
❌ **Direct database access in controllers** → ✅ Repository pattern
❌ **Hard-coded values** → ✅ Configuration files
❌ **No error handling** → ✅ Global exception middleware
❌ **Returning entities from API** → ✅ Using DTOs
❌ **No input validation** → ✅ Data annotations
❌ **Mixing concerns** → ✅ Clear layer separation

## 🔄 Future Enhancements

- Unit tests with xUnit and Moq
- Integration tests
- Authentication & Authorization (JWT)
- Real-time updates with SignalR
- Caching with Redis
- Docker containerization
- CI/CD pipeline
- Advanced charting (Chart.js, D3.js)
- Email notifications for critical anomalies

## 📚 Learning Resources

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microsoft .NET Documentation](https://docs.microsoft.com/dotnet/)
- [Angular Documentation](https://angular.io/docs)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [RESTful API Design](https://restfulapi.net/)

## 📄 License

This project is created for educational and portfolio purposes.

## 👤 Author

Built as a demonstration of professional software development practices for graduate job applications.

---

**Remember**: The goal of this project is not feature quantity, but code quality, architecture, and demonstrating professional development practices.
