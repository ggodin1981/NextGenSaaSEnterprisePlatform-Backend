using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NextGen.Api.Middleware;
using NextGen.Application.Dtos;
using NextGen.Application.Services;
using NextGen.Application.ServicesImpl;
using NextGen.Domain.Abstractions;
using NextGen.Domain.Entities;
using NextGen.Infrastructure;
using NextGen.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// DATABASE: SQL Server (preferred) with fallback to InMemory
var connString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrWhiteSpace(connString))
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
    {
        opt.UseSqlServer(connString);
    });

    Console.WriteLine("Using SQL Server: " + connString);
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
    {
        opt.UseInMemoryDatabase("NextGenEnterprise");
    });

    Console.WriteLine("Using InMemory database (SQL Server not configured)");
}

// DI
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAiInsightService, AiInsightService>();

// JWT / Auth
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),

            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                ctx.NoResult();
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return ctx.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid or expired token",
                    detail = ctx.Exception.Message
                });
            },
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                if (!ctx.Response.HasStarted)
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return ctx.Response.WriteAsJsonAsync(new
                    {
                        error = "Authentication required",
                        detail = "Bearer token is missing or invalid"
                    });
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Swagger + CORS
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NextGen SaaS Enterprise API",
        Version = "v1",
        Description = "JWT-secured multi-tenant SaaS backend (.NET 8 Minimal APIs)"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    };

    c.AddSecurityRequirement(securityRequirement);
});

builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Tenants.Any())
    {
        var tenant = new Tenant { Name = "Enterprise Demo Tenant" };
        db.Tenants.Add(tenant);

        var adminUser = new AppUser
        {
            UserName = "admin",
            PasswordHash = "admin123",
            Role = "Admin",
            TenantId = tenant.Id
        };
        var normalUser = new AppUser
        {
            UserName = "user",
            PasswordHash = "user123",
            Role = "User",
            TenantId = tenant.Id
        };
        db.Users.AddRange(adminUser, normalUser);

        var account = new Account
        {
            TenantId = tenant.Id,
            Name = "Operating Account",
            Balance = 15000m
        };
        db.Accounts.Add(account);

        db.Transactions.AddRange(
            new Transaction { AccountId = account.Id, Amount = 5000, Type = "Credit", Description = "Initial funding" },
            new Transaction { AccountId = account.Id, Amount = 1200, Type = "Debit", Description = "Cloud hosting" },
            new Transaction { AccountId = account.Id, Amount = 800, Type = "Debit", Description = "SaaS subscriptions" }
        );

        db.SaveChanges();
    }
}

// Pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors();

app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

// Helper: require tenant context
Guid RequireTenantId(HttpContext ctx)
{
    var tenantId = ctx.GetTenantId();
    if (tenantId is null)
        throw new InvalidOperationException("Tenant context not resolved. Login or supply X-Tenant-ID.");
    return tenantId.Value;
}

// PUBLIC: LOGIN
app.MapPost("/api/auth/login", async (LoginRequest request, IUnitOfWork uow, IConfiguration config) =>
{
    var users = await uow.Users.ListAsync(u => u.UserName == request.UserName);
    var user = users.SingleOrDefault();
    if (user is null || user.PasswordHash != request.Password)
        return Results.Unauthorized();

    var jwtSectionLocal = config.GetSection("Jwt");
    var keyLocal = Encoding.UTF8.GetBytes(jwtSectionLocal["Key"]!);

    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
        new Claim("role", user.Role),
        new Claim("tenant_id", user.TenantId.ToString())
    };

    var creds = new SigningCredentials(new SymmetricSecurityKey(keyLocal), SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: jwtSectionLocal["Issuer"],
        audience: jwtSectionLocal["Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: creds
    );

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new LoginResponse(jwt, user.UserName, user.Role, user.TenantId));
}).AllowAnonymous();

// PROTECTED GROUP: requires valid token
var api = app.MapGroup("/api").RequireAuthorization();

// ACCOUNTS (tenant scoped)
api.MapGet("/accounts", async (HttpContext ctx, IUnitOfWork uow) =>
{
    var tenantId = RequireTenantId(ctx);
    var accounts = await uow.Accounts.ListAsync(a => a.TenantId == tenantId);
    return Results.Ok(accounts);
});

// TRANSACTIONS (tenant scoped)
api.MapGet("/accounts/{accountId:guid}/transactions",
    async (HttpContext ctx, Guid accountId, IUnitOfWork uow) =>
{
    var tenantId = RequireTenantId(ctx);

    var account = await uow.Accounts.GetByIdAsync(accountId);
    if (account is null || account.TenantId != tenantId)
        return Results.NotFound();

    var txns = await uow.Transactions.ListAsync(t => t.AccountId == accountId);
    var ordered = txns
        .OrderByDescending(t => t.Date)
        .Select(t => new TransactionDto(t.Id, t.Date, t.Amount, t.Type, t.Description))
        .ToList();

    return Results.Ok(ordered);
});

api.MapPost("/accounts/{accountId:guid}/transactions",
    async (HttpContext ctx, Guid accountId, CreateTransactionRequest request, IUnitOfWork uow) =>
{
    var tenantId = RequireTenantId(ctx);

    var account = await uow.Accounts.GetByIdAsync(accountId);
    if (account is null || account.TenantId != tenantId)
        return Results.NotFound();

    var txn = new Transaction
    {
        AccountId = accountId,
        Date = request.Date == default ? DateTime.UtcNow : request.Date,
        Amount = request.Amount,
        Type = request.Type,
        Description = request.Description
    };

    await uow.Transactions.AddAsync(txn);

    if (txn.Type.Equals("Credit", StringComparison.OrdinalIgnoreCase))
        account.Balance += txn.Amount;
    else
        account.Balance -= txn.Amount;

    await uow.SaveChangesAsync();

    var dto = new TransactionDto(txn.Id, txn.Date, txn.Amount, txn.Type, txn.Description);
    return Results.Created($"/api/accounts/{accountId}/transactions/{txn.Id}", dto);
});

// AI INSIGHT (tenant scoped)
api.MapGet("/accounts/{accountId:guid}/ai-insight",
    async (HttpContext ctx, Guid accountId, IUnitOfWork uow, IAiInsightService ai) =>
{
    var tenantId = RequireTenantId(ctx);

    var account = await uow.Accounts.GetByIdAsync(accountId);
    if (account is null || account.TenantId != tenantId)
        return Results.NotFound();

    var insight = await ai.BuildInsightForAccountAsync(account);
    return Results.Ok(new { insight });
});

// ADMIN-ONLY
api.MapGet("/tenants", async (HttpContext ctx, IUnitOfWork uow) =>
{
    if (!ctx.User.IsInRole("Admin"))
        return Results.Forbid();

    var tenants = await uow.Tenants.ListAsync();
    return Results.Ok(tenants);
});

app.Run();
