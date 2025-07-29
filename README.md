# Star Wars API

A comprehensive Star Wars API built with .NET 8 and ASP.NET Core, featuring full CRUD operations for starships, authentication, data seeding from SWAPI, and more.

## 📚 Documentation

- **[🔐 Authentication Guide](docs/AUTHENTICATION.md)** - Complete authentication system with API keys and cookie management
- **[📚 API Reference](docs/API_REFERENCE.md)** - Full endpoint documentation with examples
- **[🐳 Deployment Guide](docs/DEPLOYMENT.md)** - Docker deployment, production setup, and scaling
- **[🏗️ Development Guide](docs/DEVELOPMENT.md)** - Architecture, testing, and contribution guidelines

## 🚀 Quick Start

**Choose your preferred method:**

### Option 1: Docker (Recommended)
```bash
git clone https://github.com/yourusername/star-wars-api.git
cd star-wars-api
docker-compose up -d
```
Access at: http://localhost:8080

### Option 2: Local Development
```bash
git clone https://github.com/yourusername/star-wars-api.git
cd star-wars-api
dotnet restore
dotnet run
```
Access at: https://localhost:7108

## 🌟 Key Features

- **🚀 Full CRUD Operations**: Create, Read, Update, and Delete starships with advanced filtering
- **🔐 Secure Authentication**: Dynamic API key system with automatic cookie management  
- **🌱 Data Seeding**: Automatically populate database from Star Wars API (SWAPI)
- **🐳 Docker Ready**: Complete containerization with Docker Compose
- **📊 Comprehensive Logging**: Structured logging with NLog and multiple targets
- **🧪 Extensive Testing**: Unit, integration, and performance tests with >80% coverage
- **📚 Interactive Documentation**: Swagger/OpenAPI with live testing capabilities

## 🛠️ Technology Stack

- **.NET 8** - Latest .NET framework with performance improvements
- **ASP.NET Core** - High-performance web API framework
- **Entity Framework Core** - Modern ORM with SQL Server
- **ASP.NET Identity** - Robust authentication and user management
- **Docker** - Containerization and orchestration
- **xUnit** - Comprehensive testing framework
- **NLog** - Structured logging with rich context
- **FluentValidation** - Powerful input validation

## 📋 Prerequisites

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** - LocalDB (included with Visual Studio) or full instance
- **Docker** (optional) - For containerized deployment

## 🏃‍♂️ Getting Started

### 1. Clone and Setup
```bash
git clone https://github.com/yourusername/star-wars-api.git
cd star-wars-api
dotnet restore
```

### 2. Configure Database
The application uses SQL Server LocalDB by default. Connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=StarWarsDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Run the Application
```bash
dotnet run
```

**Access Points:**
- **API**: https://localhost:7108
- **Swagger UI**: https://localhost:7108/swagger
- **Health Check**: https://localhost:7108/health

### 4. First Steps
1. **Register**: Create an account at `/api/auth/register`
2. **Get API Key**: Automatically generated and saved as cookie
3. **Explore**: Use Swagger UI or make direct API calls
4. **Seed Data**: Use `/api/seed/starships` to populate with SWAPI data

## 🔐 Authentication Overview

**Two Authentication Methods:**
- **🍪 Cookie Authentication**: Automatic for browsers (set on login/register)
- **🔑 Header Authentication**: Manual for API clients (`X-API-Key` header)

**Quick Authentication Example:**
```bash
# Register new user
curl -X POST "https://localhost:7108/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Luke","lastName":"Skywalker","email":"luke@jedi.com","password":"Password123!"}'

# Use returned token for API calls
curl -X GET "https://localhost:7108/api/starships" \
  -H "X-API-Key: [your-api-key-here]"
```

**→ [Complete Authentication Guide](docs/AUTHENTICATION.md)**

## 🐳 Docker Deployment

**Quick Docker Setup:**
```bash
# Start with Docker Compose (includes SQL Server)
docker-compose up -d

# Access API at http://localhost:8080
# SQL Server available at localhost:1433
```

**Production Deployment:**
```bash
# Use production configuration
docker-compose -f docker-compose.prod.yml up -d
```

**→ [Complete Deployment Guide](docs/DEPLOYMENT.md)**

## 📚 API Endpoints

**18 endpoints across 5 controllers:**

- **🔐 Auth** (4 endpoints) - User registration, login, password management
- **🔑 ApiKey** (3 endpoints) - API key lifecycle management
- **🚀 Starships** (8 endpoints) - Full CRUD operations for starships
- **🌱 Seed** (1 endpoint) - Database seeding from SWAPI
- **👑 Admin** (2 endpoints) - Administrative operations

**Quick API Examples:**
```bash
# Get all starships (with authentication cookie)
GET /api/starships

# Create new starship
POST /api/starships
{
  "name": "Millennium Falcon",
  "manufacturer": "Corellian Engineering Corporation",
  "model": "YT-1300 light freighter"
}

# Seed data from SWAPI
POST /api/seed/starships
```

**→ [Complete API Reference](docs/API_REFERENCE.md)**

## 🏗️ Architecture & Development

**Clean Architecture Design:**
- **Presentation Layer**: Controllers, middleware, filters
- **Application Layer**: Services, DTOs, validators  
- **Domain Layer**: Models, interfaces, entities
- **Infrastructure Layer**: Data access, external APIs, logging

**Design Patterns:**
- Repository Pattern
- Unit of Work
- Dependency Injection
- Strategy Pattern (Authentication)
- Factory Pattern (API Keys)

**→ [Complete Development Guide](docs/DEVELOPMENT.md)**

## 🧪 Testing

**Comprehensive Testing Framework:**
- **Unit Tests**: Controller, service, and repository testing with Moq
- **Integration Tests**: Full HTTP pipeline and database testing
- **Code Coverage**: >80% coverage with detailed reporting
- **Testing Tools**: xUnit, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing

```bash
# Run all tests
dotnet test

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

**→ [Complete Development Guide](docs/DEVELOPMENT.md)**

---

## ⚙️ Configuration

**Key Configuration Areas:**
- **Database**: SQL Server LocalDB (development) / SQL Server (production)
- **Authentication**: Dynamic API keys with 30-minute expiration
- **Logging**: NLog with multiple targets (console, files)
- **CORS**: Configurable cross-origin resource sharing
- **Docker**: Production-ready containerization

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=StarWarsDB;Trusted_Connection=true"
  },
  "ApiKeySettings": {
    "ExpirationMinutes": 30,
    "HeaderName": "X-API-Key"
  }
}
```

---

## 🚀 Performance Features

- **Async/Await**: Non-blocking operations throughout
- **Database Indexing**: Optimized queries for API key lookups
- **Pagination**: Efficient data loading with configurable page sizes
- **Bulk Operations**: Optimized SWAPI data seeding
- **Connection Pooling**: Entity Framework Core connection management

---

## 🐛 Troubleshooting

**Common Issues & Solutions:**

- **Database Connection** - Verify SQL Server LocalDB installation and connection string
- **API Key Expired** - Use `/api/apikey/regenerate` or login again
- **SWAPI Seeding Fails** - Check internet connectivity and authentication
- **Cookie Authentication** - Clear browser cookies and re-login

**Diagnostic Commands:**
```bash
# Check database status
dotnet ef database update

# Test API authentication
curl -X POST "https://localhost:7108/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123!"}'

# View application logs
type "logs\nlog-AspNetCore-own-2025-07-29.log"
```

---

## 🤝 Contributing

**We welcome contributions!**

1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/amazing-feature`
3. **Commit** your changes: `git commit -m 'Add amazing feature'`
4. **Push** to branch: `git push origin feature/amazing-feature`
5. **Open** a Pull Request

**Development Guidelines:**
- Follow existing code style and patterns
- Add unit tests for new features
- Update documentation as needed
- Ensure all tests pass before submitting

**→ [Complete Development Guide](docs/DEVELOPMENT.md)**

---

## 📈 Performance Metrics

- **Response Time**: <100ms for typical API calls
- **Throughput**: >1000 requests/second in production
- **Database**: Indexed queries with <10ms lookup times
- **Memory Usage**: <100MB baseline application footprint

---

## 🔒 Security Features

- **🔐 Authentication**: Dynamic API keys with secure generation
- **🛡️ Authorization**: Role-based access control ready
- **🍪 Cookie Security**: HttpOnly, Secure, SameSite protection
- **🔒 Password Security**: PBKDF2 hashing with complexity requirements
- **🚨 Audit Logging**: Comprehensive security event tracking
- **⏰ Token Expiration**: Short-lived tokens with automatic refresh

---

## 📞 Support

- kelvinesiri@gmail.com

---

**May the Force be with you!** ⭐
