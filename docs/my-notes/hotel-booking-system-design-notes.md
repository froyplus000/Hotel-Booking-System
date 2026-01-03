# System Design Notes

This is a note of my understanding regarding to System design for this project.

## Understanding folder structure

```text
src/
  Api/
  Worker/
  Domain/
  Application/
  Infrastructure/
tests/
  Unit/
  Integration/
deploy/
  docker/
  terraform/

```

If we look at folder structure of this Hotel Booking project, we'll see that there are multiple projects for a single solution file. The reason is we want separation of concern in this project, that's why we have many projects to represent each roles and reposibilities in the system.

## Clean Architecture

- **Domain**
  - Core business concepts
  - Contain Entities like Hotel, RoomType, Booking, etc.
  - This is the brain of Hotel Booking project
- **Application**
  - Use case coordinator
  - Contain use cases or services like SearchHotels, CreateHold, CreateBooking, etc.
  - Interfaces like IHotelRepository, IBookingRepository, etc.
  - The Application layer says what should happen in a scenario, but not how data is stored or sent.
  - It calls interfaces instead of concrete classes, so it doesn't know or care if the data comes from Postgres or a Text file.
- **Infrastructure**
  - `AppDbContext` with EF Core and Postgres Schema
  - Repository implementations like `HotelRepository : IHotelRepository`
  - Redis cache implementation like `RedisCache : ICache`
  - Adapters for AWS
- **API**
  - HTTP front door
  - ASP.NET Core `Programj.cs`
  - Contreollers or minimal API endpoints (`/search`, `/holds`, `/bookings`, `/health`).
  - DTOs (request & response models)
  - Validation
  - Auth config
  - Swagger
  - HTTP details, routing, model binding, status codes
- **Worker**
  - Background jobs
  - .NET backgroud service that
    - Listens to SQS queues.
    - Processes CSV imports.
    - Sends emails.
    - Expires holds, etc
  - It still uses `Application` + `Domain` + `Infrastructure` like `API` does.
  - Differences from `API`:
    - `API` responds to HTTP
    - `Worker` responds to messages, events, time-based triggers.
- **Tests and Deploy**
  - `tests/Unit` – test small pieces like “availability calculation is correct”.
  - `tests/Integration` – start a test Postgres/Redis (Testcontainers) and test real queries and endpoints.
  - `deploy/docker` – Dockerfiles for Api & Worker.
  - `deploy/terraform` – AWS infra as code: ECS, RDS, Redis, SQS, etc.

## How do the projects depend on each other?

- Concretely (and this is exactly what the docs wired up):
  - Api references: Application, Infrastructure, Domain
  - Worker references: Application, Infrastructure, Domain
  - Application references: Domain only
  - Domain references: nothing
  - Infrastructure references: Domain (for entities) and maybe Application (for interfaces/ports), but Domain/Application do not reference Infrastructure
    > So your inner code doesn’t know about outer frameworks. That’s the core Clean Architecture idea adapted in the plan.

## Why not just one big project?

Imagine you put everything into a single HotelBooking.Api project:

1. Coupling hell

- Controllers start reaching directly into EF DbContext everywhere.
- Business rules sneak into controllers or EF configurations.
- Changing a database field might break controllers, workers, and tests in surprising ways.

2. Hard to test

- To test a simple rule like “check-out must be after check-in”, you’d need ASP.NET & EF Core around it.
- With Domain separated, you can test that rule using plain C# unit tests with no database.

3. Hard to evolve

- You decide to add gRPC or GraphQL later?
  - In the current design, that’s a new delivery project that also calls Application. Your core logic doesn’t change.
- You decide to split into microservices later?
  - The way your Domain/Application are structured makes it easier to carve out pieces.

4. Portfolio & interviews

- People will immediately recognise:
  - modular monolith
  - clean layering
  - clear boundaries (Domain / Application / Infrastructure / Api / Worker).

> That’s a lot more impressive than a “single giant Web API project”.

![Dependencies and Runtime Request Flow](../screenshots/dependencies-request-flows.png)
