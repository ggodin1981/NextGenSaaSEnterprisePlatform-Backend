# NextGen SaaS Enterprise API 

- .NET 8 Minimal APIs
- JWT auth + Swagger Bearer support
- Role-based & tenant-aware
- Repository + UnitOfWork
- SQL Server (with InMemory fallback)

Open `src/NextGen.sln`, set `NextGen.Api` as startup, run.
Use `/api/auth/login` to get a JWT, then click **Authorize** in Swagger and call `/api/accounts`, `/api/.../transactions`, `/api/.../ai-insight`, `/api/tenants`.
