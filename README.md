# NextGen SaaS Enterprise API 

# 🚀 NextGen SaaS Enterprise Platform — .NET 8 (Backend)

A fully functional enterprise-grade SaaS starter platform built with modern .NET 8.
This project demonstrates real-world architecture, including:

- Multi-tenant SaaS design

- JWT/OAuth2 authentication

- Role-based authorization

- Repository + Unit of Work pattern

- SQL Server + EF Core

- Minimal APIs following clean architecture

- Error handling middleware

- AI Insight Engine (example business logic)

- Swagger with Bearer Token authentication

- Production-ready folder structure

This is a complete backend designed to showcase high-level engineering skill for enterprise SaaS, fintech, automation, cloud-native systems, and microservices platforms.
 
# 🎯 Features Included
# ✔ Enterprise Architecture

- Clean separation: Domain → Application → Infrastructure → API

- Repository + UnitOfWork pattern

- Tenant-aware context resolution

- Multi-layered dependency injection setup

# ✔ Authentication & Authorization

- JWT bearer authentication (HMAC SHA256)

- Role-based access (Admin, User)

- Token claims include:

   - sub

   - role

   - tenant_id

- Swagger Authorize button enabled

# ✔ Multi-Tenant Support

- TenantMiddleware reads:

   - tenant_id claim from JWT, OR

   - X-Tenant-ID request header

- APIs automatically filter data by tenant context

# ✔ Database Layer

- SQL Server via EF Core

- InMemory fallback available

- Automatic seed data:

- Admin + User accounts

- Tenant

- Sample account + transactions

# ✔ Global Middleware

- Centralized ErrorHandlingMiddleware

- Automatically returns structured JSON for exceptions

- Consistent logging using ASP.NET logging abstractions

# ✔ Domain Entities

- Tenant

- Account

- Transaction

- AppUser

- Each entity demonstrates:

- Clean modeling

- Relationships

- Tenant isolation enforcement

# ✔ AI Insight Engine

- A service that generates intelligent insights for account activity:

- Evaluates credits/debits

- Detects spending patterns

- Generates financial summaries

- Returns natural language insights

# 🧱 Project Structure


```text
src/
 ├── NextGen.Domain
 │     ├── Entities
 │     └── Abstractions
 │
 ├── NextGen.Application
 │     ├── DTOs
 │     └── Services
 │
 ├── NextGen.Infrastructure
 │     ├── EF Core DbContext
 │     ├── Repository<T>
 │     └── UnitOfWork
 │
 ├── NextGen.Api
       ├── Program.cs (Minimal API)
       ├── Middleware
       ├── appsettings.json
       └── Swagger configuration
```


# 🔌 API Endpoints
# 🔑 Authentication
- Method	Endpoint	Description
- POST	/api/auth/login	Returns JWT token
# 🧾 Accounts
- Method	Endpoint	Auth
- GET	/api/accounts	Requires Bearer token
# 💳 Transactions
- Method	Endpoint	Auth
- GET	/api/accounts/{id}/transactions	Yes
- POST	/api/accounts/{id}/transactions	Yes
# 🤖 AI Insights
- Method	Endpoint	Auth
- GET	/api/accounts/{id}/ai-insight	Yes
# 🛠 Admin Only
- Method	Endpoint	Role
- GET	/api/tenants	Admin

# 🔐 Authentication Flow

Login via:

POST /api/auth/login
{
  "userName": "admin",
  "password": "admin123"
}


Copy JWT token from response:

eyJhbGciOiJIUzI1NiIsInR5cCI...


In Swagger → Click Authorize → Enter:

Bearer eyJhbGciOiJIUzI1NiIsInR5cCI...


All /api/... endpoints become accessible.

# 🗄 SQL Server Configuration

Update appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=NextGenSaaS;Trusted_Connection=True;TrustServerCertificate=True"
}


To use migrations:

cd src/NextGen.Infrastructure
dotnet ef migrations add Initial
dotnet ef database update

# 🧪 Sample Users
Username	Password	Role	Tenant
admin	admin123	Admin	Default Tenant
user	user123	User	Default Tenant
# 🚀 Running the Project

cd src
dotnet restore
dotnet build
dotnet run --project NextGen.Api


Open Swagger UI:

# 📌 https://localhost:{port}/swagger
