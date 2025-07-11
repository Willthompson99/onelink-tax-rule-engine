# Oklahoma Tax Engine

A comprehensive tax management system built with ASP.NET Core 8.0, demonstrating enterprise-level architecture and modern development practices aligned with the Oklahoma Tax Commission's technology requirements.

## Quick Start

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/OklahomaTaxEngine.git
   cd OklahomaTaxEngine
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database connection**
   - Edit `appsettings.json` with your SQL Server connection string

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - Web Interface: https://localhost:5001
   - API Documentation: https://localhost:5001/api-docs

## Project Overview

This tax engine demonstrates proficiency in:
- **Web-based application development** from conception to deployment
- **Complex business logic** implementation for tax calculations
- **Modern .NET/C#** development practices
- **SQL Server** database design and optimization
- **RESTful API** development with comprehensive documentation
- **Responsive web interfaces** with real-time functionality

## Technology Stack

- **Backend**: ASP.NET Core 8.0, C# 12
- **Database**: SQL Server with Entity Framework Core 8.0
- **Frontend**: HTML5, Bootstrap 5.3, JavaScript (Vanilla)
- **API Documentation**: Swagger/OpenAPI
- **Architecture**: Clean Architecture with Repository Pattern
- **Caching**: In-Memory Cache for performance optimization

## Features

### Core Functionality
- Multi-tax type support (Income, Sales, Property, Corporate)
- Dynamic rule engine with temporal validity
- Progressive tax bracket calculations
- Real-time tax computation
- Transaction management with audit trail
- Taxpayer registration and management

### Technical Features
- RESTful API with comprehensive endpoints
- Swagger documentation for API testing
- Input validation and error handling
- Pagination support for large datasets
- Soft delete implementation
- Caching for frequently accessed data

### User Interface
- Modern, responsive dashboard
- Interactive tax calculator
- Real-time data updates
- Intuitive navigation
- Transaction status management

## Database Schema

### Tables
1. **TaxPayers** - Stores taxpayer information
2. **TaxRules** - Configurable tax rules with effective dates
3. **TaxTransactions** - Transaction records with audit trail

### Key Features
- Proper indexing for performance
- Foreign key relationships
- Temporal data support
- Audit columns (CreatedAt, UpdatedAt)

## 🔌 API Endpoints

### Taxpayers
- `GET /api/taxpayers` - List all taxpayers
- `GET /api/taxpayers/{id}` - Get specific taxpayer
- `GET /api/taxpayers/bytaxid/{taxId}` - Get by tax ID
- `POST /api/taxpayers` - Register new taxpayer
- `PUT /api/taxpayers/{id}` - Update taxpayer
- `DELETE /api/taxpayers/{id}` - Deactivate taxpayer

### Tax Rules
- `GET /api/taxrules` - List all rules
- `GET /api/taxrules/active` - Get active rules by type
- `POST /api/taxrules` - Create new rule
- `PUT /api/taxrules/{id}` - Update rule
- `DELETE /api/taxrules/{id}` - Deactivate rule

### Transactions
- `GET /api/transactions` - List all transactions
- `GET /api/transactions/taxpayer/{taxId}` - Get taxpayer transactions
- `POST /api/transactions/calculate` - Calculate tax
- `POST /api/transactions` - Create transaction
- `PUT /api/transactions/{id}/pay` - Mark as paid

## Tax Calculation Logic

### Income Tax (Progressive Brackets)
- $0 - $1,000: 0.5%
- $1,000 - $2,500: 1%
- $2,500 - $3,750: 2%
- $3,750 - $4,900: 3%
- $4,900 - $7,200: 4%
- $7,200+: 5%

### Other Tax Types
- Sales Tax: 4.5% (flat rate)
- Property Tax: 1.2% (flat rate)
- Corporate Tax: 6% (flat rate)

## Architecture Highlights

### Clean Architecture
```
OklahomaTaxEngine/
├── Controllers/     # API endpoints
├── Services/        # Business logic
├── Models/          # Domain entities
├── Data/            # Database context
└── wwwroot/         # Static files
```

### Design Patterns
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Loose coupling
- **Service Layer** - Business logic separation
- **DTO Pattern** - Data transfer objects

## Security Considerations

- Input validation on all endpoints
- SQL injection prevention via Entity Framework
- CORS configuration for API access
- Proper error handling without exposing internals
- Secure connection strings management

## Performance Optimizations

- Efficient database queries with proper indexing
- Memory caching for frequently accessed rules
- Asynchronous operations throughout
- Pagination for large datasets
- Optimized LINQ queries

## Testing

The project structure supports:
- Unit testing with xUnit
- Integration testing for API endpoints
- Service layer testing with mocked repositories

Example test command:
```bash
dotnet test
```

## Future Enhancements

1. **Reporting Module** - Tax reports and analytics
2. **Bulk Processing** - Handle large-scale calculations
3. **API Versioning** - Support multiple API versions
4. **Authentication** - Secure access control
5. **Multi-tenant Support** - Multiple jurisdictions
6. **Export Functionality** - PDF/Excel reports

## Alignment with OTC Requirements

This project demonstrates:
- **Web-based application** from conception to deployment
- **Complex problem solving** with tax calculations
- **.NET/C# expertise** (preferred technology)
- **SQL programming** knowledge
- **Software lifecycle** understanding
- **Business analysis** and requirements translation
- **Team collaboration** ready architecture

## Contact

**Will Thompson**
- Email: will.j.thompson@outlook.com
- LinkedIn: https://www.linkedin.com/in/will-thompson-8
- GitHub: github.com/willthompson99

---

*This project was developed as a demonstration of technical capabilities for the OneLink Application Developer position at the Oklahoma Tax Commission. It showcases the ability to design, develop, and deploy enterprise-level tax management systems using modern technologies and best practices.*