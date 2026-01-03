# Implementation Plans for M0 - M2.md

This document gives concrete, copy‑pasteable steps and code scaffolds to complete **Milestones M0–M2** for the backend‑only MVP. Use it as repo documentation.

> Target stack: **.NET 8 Web API**, **EF Core + PostgreSQL**, **Redis**, **Docker/Compose**, **LocalStack** (SQS/S3/SES mocks), **k6** (basic load), **GitHub Actions (CI)**.

- [Implementation Plans for M0 - M2.md](#implementation-plans-for-m0---m2md)
  - [M0 — Project Bootstrap](#m0--project-bootstrap)
    - [1) Repo structure (monorepo)](#1-repo-structure-monorepo)
    - [2) Create solutions \& projects](#2-create-solutions--projects)
    - [3) Packages](#3-packages)
    - [4) Dockerfiles](#4-dockerfiles)
    - [5) Local docker-compose](#5-local-docker-compose)
    - [6) Health endpoint (Api)](#6-health-endpoint-api)
  - [M1 — Data Model \& EF Core Migrations](#m1--data-model--ef-core-migrations)
    - [1) Minimal entities (Domain)](#1-minimal-entities-domain)
    - [2) DbContext \& configuration (Infrastructure)](#2-dbcontext--configuration-infrastructure)
    - [3) Wire DbContext in Api](#3-wire-dbcontext-in-api)
    - [4) Initial migration \& database](#4-initial-migration--database)
    - [5) Seed script (optional for demo)](#5-seed-script-optional-for-demo)
  - [M2 — Search \& Catalog (Read Path)](#m2--search--catalog-read-path)
    - [1) API contract](#1-api-contract)
    - [2) Query logic (Application)](#2-query-logic-application)
    - [3) Redis cache service (Infrastructure)](#3-redis-cache-service-infrastructure)
    - [4) Search endpoint (Api)](#4-search-endpoint-api)
    - [5) Unit \& integration tests](#5-unit--integration-tests)
    - [6) Basic k6 script (optional at M2)](#6-basic-k6-script-optional-at-m2)
  - [CI (minimal for M0–M2)](#ci-minimal-for-m0m2)
  - [Ready‑to‑run](#readytorun)
  - [What to commit in M0–M2](#what-to-commit-in-m0m2)

---

## M0 — Project Bootstrap

### 1) Repo structure (monorepo)

```
/ (root)
├─ src/
│  ├─ Api/
│  ├─ Worker/
│  ├─ Domain/
│  ├─ Application/
│  └─ Infrastructure/
├─ tests/
│  ├─ Unit/
│  └─ Integration/
├─ deploy/
│  ├─ docker/
│  └─ terraform/
├─ docs/
│  └─ IMPLEMENTATION-M0-M2.md
└─ .github/workflows/
```

### 2) Create solutions & projects

```bash
# from repo root
dotnet new gitignore

dotnet new sln -n HotelBooking

# Projects
cd src

dotnet new webapi -n Api -f net8.0 --use-controllers

dotnet new console -n Worker -f net8.0

dotnet new classlib -n Domain -f net8.0

dotnet new classlib -n Application -f net8.0

dotnet new classlib -n Infrastructure -f net8.0

# Add to solution
cd ..
dotnet sln add src/Api/src/Api.csproj || dotnet sln add src/Api/Api.csproj
dotnet sln add src/Worker/Worker.csproj
dotnet sln add src/Domain/Domain.csproj
dotnet sln add src/Application/Application.csproj
dotnet sln add src/Infrastructure/Infrastructure.csproj

# Wire references
cd src/Api
dotnet add reference ../Application/Application.csproj ../Infrastructure/Infrastructure.csproj ../Domain/Domain.csproj
cd ../Worker
dotnet add reference ../Application/Application.csproj ../Infrastructure/Infrastructure.csproj ../Domain/Domain.csproj
```

### 3) Packages

```bash
# Infrastructure
cd src/Infrastructure
 dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
 dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
 dotnet add package StackExchange.Redis
 dotnet add package AWSSDK.S3
 dotnet add package AWSSDK.SQS
 dotnet add package AWSSDK.SimpleEmail
 dotnet add package Amazon.Extensions.NETCore.Setup
 dotnet add package OpenTelemetry.Exporter.Otlp
 dotnet add package OpenTelemetry.Extensions.Hosting
 dotnet add package Serilog.AspNetCore
 dotnet add package Serilog.Sinks.Console
 dotnet add package Polly

# Api
cd ../Api
 dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
 dotnet add package Swashbuckle.AspNetCore
 dotnet add package FluentValidation.AspNetCore
 dotnet add package OpenTelemetry.Exporter.Otlp
 dotnet add package Serilog.AspNetCore

# Tests
cd ../../tests/Integration
 dotnet new xunit -n Integration
 dotnet add package Respawn
 dotnet add package Npgsql
 dotnet add package Testcontainers
cd ../Unit
 dotnet new xunit -n Unit
```

### 4) Dockerfiles

**`deploy/docker/Api.Dockerfile`**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./src ./src
RUN dotnet restore ./src/Api/Api.csproj \
 && dotnet publish ./src/Api/Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
```

**`deploy/docker/Worker.Dockerfile`**

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./src ./src
RUN dotnet restore ./src/Worker/Worker.csproj \
 && dotnet publish ./src/Worker/Worker.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Worker.dll"]
```

### 5) Local docker-compose

**`docker-compose.yml`** (root)

```yaml
version: "3.9"
services:
  api:
    build:
      context: .
      dockerfile: deploy/docker/Api.Dockerfile
    environment:
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__Postgres=Host=db;Username=postgres;Password=postgres;Database=hotel
      - Redis__Connection=redis:6379
      - AWS__ServiceURL=http://localstack:4566
      - AWS__Region=ap-southeast-1
    ports: ["8080:8080"]
    depends_on: [db, redis, localstack]

  worker:
    build:
      context: .
      dockerfile: deploy/docker/Worker.Dockerfile
    environment:
      - ConnectionStrings__Postgres=Host=db;Username=postgres;Password=postgres;Database=hotel
      - Redis__Connection=redis:6379
      - AWS__ServiceURL=http://localstack:4566
      - AWS__Region=ap-southeast-1
    depends_on: [db, redis, localstack]

  db:
    image: postgres:15
    environment:
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: hotel
    ports: ["5432:5432"]
    volumes:
      - pgdata:/var/lib/postgresql/data

  redis:
    image: redis:7
    ports: ["6379:6379"]

  localstack:
    image: localstack/localstack:2
    environment:
      - SERVICES=sqs,s3,ses
      - AWS_DEFAULT_REGION=ap-southeast-1
    ports: ["4566:4566"]

volumes:
  pgdata:
```

### 6) Health endpoint (Api)

Add `/health` returning 200 OK so compose readiness checks are easy.

---

## M1 — Data Model & EF Core Migrations

### 1) Minimal entities (Domain)

**`src/Domain/Entities.cs`**

```csharp
namespace Domain;

public record Hotel(Guid Id, string Name, string Address, decimal Lat, decimal Lng, string Status);
public record RoomType(Guid Id, Guid HotelId, string Name, int Capacity, string AmenitiesJson, bool Active);
public record RatePlan(Guid Id, Guid HotelId, Guid RoomTypeId, string Name, string Currency, string CancellationPolicyJson, bool Active);
public record RatePlanPrice(Guid Id, Guid RatePlanId, DateOnly Date, long PriceMinor, int? MinStay, int? MaxStay);
public record Inventory(Guid Id, Guid HotelId, Guid RoomTypeId, DateOnly Date, int Allotment, int Held, int Sold);
public record Hold(Guid Id, Guid HotelId, Guid RoomTypeId, DateOnly Date, int Qty, DateTime ExpiresAt, string CustomerRef, string IdempotencyKey);
public record Booking(Guid Id, Guid HotelId, Guid? UserId, string CustomerName, string CustomerEmail, string Currency, long TotalMinor, string Status, DateTime CreatedAt);
public record BookingItem(Guid Id, Guid BookingId, Guid RoomTypeId, DateOnly CheckIn, DateOnly CheckOut, string NightlyPriceMinorJson);
public record Payment(Guid Id, Guid BookingId, string Provider, string Status, long AmountMinor, string PayloadJson);
public record IdempotencyKey(Guid Id, string Scope, string Key, string RequestHash, DateTime CreatedAt, DateTime ExpiresAt);
public record AuditLog(Guid Id, Guid? ActorUserId, Guid? HotelId, string Action, string Entity, Guid EntityId, string PayloadJson, DateTime CreatedAt);
public record ImportJob(Guid Id, Guid HotelId, string Type, string S3Key, string Status, DateTime CreatedAt, string Log);
```

### 2) DbContext & configuration (Infrastructure)

**`src/Infrastructure/AppDbContext.cs`**

```csharp
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<RatePlan> RatePlans => Set<RatePlan>();
    public DbSet<RatePlanPrice> RatePlanPrices => Set<RatePlanPrice>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Hold> Holds => Set<Hold>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ImportJob> Imports => Set<ImportJob>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasPostgresExtension("uuid-ossp");
        b.Entity<Hotel>().HasKey(x => x.Id);
        b.Entity<RoomType>().HasKey(x => x.Id);
        b.Entity<RatePlan>().HasKey(x => x.Id);
        b.Entity<RatePlanPrice>().HasKey(x => x.Id);
        b.Entity<Inventory>().HasKey(x => x.Id);
        b.Entity<Hold>().HasKey(x => x.Id);
        b.Entity<Booking>().HasKey(x => x.Id);
        b.Entity<BookingItem>().HasKey(x => x.Id);
        b.Entity<Payment>().HasKey(x => x.Id);
        b.Entity<IdempotencyKey>().HasKey(x => x.Id);
        b.Entity<AuditLog>().HasKey(x => x.Id);
        b.Entity<ImportJob>().HasKey(x => x.Id);

        // Important indexes
        b.Entity<Inventory>().HasIndex(x => new { x.HotelId, x.RoomTypeId, x.Date });
        b.Entity<RatePlanPrice>().HasIndex(x => new { x.RatePlanId, x.Date });
    }
}
```

### 3) Wire DbContext in Api

**`src/Api/Program.cs`** (snippets)

```csharp
using Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var cs = builder.Configuration.GetConnectionString("Postgres")!;

builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(cs));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
```

### 4) Initial migration & database

```bash
# from repo root
export ConnectionStrings__Postgres="Host=localhost;Username=postgres;Password=postgres;Database=hotel"

# add EF tools if needed
 dotnet tool install --global dotnet-ef || true

# create migration in Infrastructure project
cd src/Infrastructure
 dotnet ef migrations add InitialCreate -s ../Api/Api.csproj
 dotnet ef database update -s ../Api/Api.csproj
```

### 5) Seed script (optional for demo)

Create a hosted service or a one‑off console to insert a couple of hotels, room types, 90 days of prices & inventory (5–10 rooms each) to enable search immediately.

---

## M2 — Search & Catalog (Read Path)

### 1) API contract

- `GET /search?location=Bangkok&checkIn=2026-01-10&checkOut=2026-01-12&guests=2&page=1&pageSize=20`  
  **200**

```json
{
  "results": [
    {
      "hotelId": "...",
      "hotelName": "...",
      "distanceKm": 1.2,
      "roomTypes": [
        {
          "roomTypeId": "...",
          "name": "Deluxe",
          "capacity": 2,
          "lowestNightlyMinor": 180000,
          "availableNights": 2
        }
      ]
    }
  ],
  "total": 125,
  "page": 1,
  "pageSize": 20
}
```

### 2) Query logic (Application)

- Parse input → map to bbox or simple city filter (keep it simple for MVP).
- For each hotel → select room types with **continuous availability** across the date range from `inventory` where `allotment - held - sold > 0` per night.
- Join with `rate_plan_prices` to compute minimum nightly price.
- Cache the entire response in Redis with key = hash(params), TTL 30–60s.

### 3) Redis cache service (Infrastructure)

**`src/Infrastructure/Cache/ICache.cs`**

```csharp
public interface ICache
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default);
}
```

**`src/Infrastructure/Cache/RedisCache.cs`**

```csharp
using StackExchange.Redis;
using System.Text.Json;

public class RedisCache(IConnectionMultiplexer mux) : ICache
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var val = await mux.GetDatabase().StringGetAsync(key);
        return val.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(val!);
    }
    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
        => mux.GetDatabase().StringSetAsync(key, JsonSerializer.Serialize(value), ttl);
}
```

Register in `Program.cs`:

```csharp
using StackExchange.Redis;
using Infrastructure;

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(builder.Configuration["Redis:Connection"]!));
builder.Services.AddScoped<ICache, RedisCache>();
```

### 4) Search endpoint (Api)

**`src/Api/Endpoints/SearchEndpoints.cs`** (minimal)

```csharp
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearch(this IEndpointRouteBuilder app)
    {
        app.MapGet("/search", async (string location, DateOnly checkIn, DateOnly checkOut, int guests, int page, int pageSize, AppDbContext db, ICache cache) =>
        {
            var key = $"search:{location}:{checkIn}:{checkOut}:{guests}:{page}:{pageSize}";
            var cached = await cache.GetAsync<object>(key);
            if (cached is not null) return Results.Ok(cached);

            // Simplified location filter: all hotels for MVP
            var nights = Enumerable.Range(0, (checkOut.DayNumber - checkIn.DayNumber))
                                   .Select(i => DateOnly.FromDayNumber(checkIn.DayNumber + i))
                                   .ToArray();

            var hotels = await db.Hotels.AsNoTracking().ToListAsync();
            var roomTypes = await db.RoomTypes.AsNoTracking().ToListAsync();

            // TODO: real availability & price query (join inventory + rate_plan_prices)
            var response = new { results = new object[] { }, total = 0, page, pageSize };

            await cache.SetAsync(key, response, TimeSpan.FromSeconds(45));
            return Results.Ok(response);
        });
        return app;
    }
}
```

Call in `Program.cs`:

```csharp
app.MapSearch();
```

_(You’ll expand the query to compute per‑night availability and min prices using SQL with window aggregates or grouping.)_

### 5) Unit & integration tests

- **Unit:** policies that compute `available = allotment - held - sold`, min‑stay validations.
- **Integration:** spin Postgres/Redis via testcontainers; seed a hotel & room; assert that `/search` shows available rooms for a 2‑night range.

### 6) Basic k6 script (optional at M2)

**`tests/Load/k6-search.js`**

```js
import http from "k6/http";
import { sleep } from "k6";
export const options = { vus: 10, duration: "30s" };
export default function () {
  http.get(
    "http://localhost:8080/search?location=Bangkok&checkIn=2026-01-10&checkOut=2026-01-12&guests=2&page=1&pageSize=20"
  );
  sleep(1);
}
```

---

## CI (minimal for M0–M2)

**`.github/workflows/ci.yml`**

```yaml
name: ci
on:
  pull_request:
  push:
    branches: [main]
jobs:
  build-test:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
          POSTGRES_DB: hotel
        ports: ["5432:5432"]
        options: >-
          --health-cmd "pg_isready -U postgres" --health-interval 10s --health-timeout 5s --health-retries 5
      redis:
        image: redis:7
        ports: ["6379:6379"]
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: "8.0.x" }
      - run: dotnet restore
      - run: dotnet build --no-restore -c Release
      - run: dotnet test --no-build -c Release --logger "trx;LogFileName=test.trx"
```

---

## Ready‑to‑run

1. `docker compose up --build`
2. Open Swagger at `http://localhost:8080/swagger` and call `/health`.
3. Apply migration & seed data.
4. Call `/search` (placeholder), then flesh out availability & pricing query.

---

## What to commit in M0–M2

- Solution + projects, Dockerfiles, compose, EF Core context + initial migration, placeholder Search endpoint, Redis cache service, basic CI pipeline, (optional) k6 script.

> Next document will cover **M3 (Inventory & Holds)** and **M4 (Booking)** with idempotency, transactions, and the worker.
