# Understanding Workflows - Containerization with Docker

This is the first time I individually working on a back end project that's includes area like System Design, DevOps and Deployment to Cloud. I'm not familiar with workflows in a project like this. Therefore, I decided to spend time learning deeper about the workflow and Docker configuration for the project.

- [Understanding Workflows - Containerization with Docker](#understanding-workflows---containerization-with-docker)
  - [High Level Overview](#high-level-overview)
  - [Outer loop (realistic environment check)](#outer-loop-realistic-environment-check)
    - [Why --build?](#why---build)
    - [So:](#so)
  - [Software Development Life Cycle](#software-development-life-cycle)
    - [Where Docker and CI/CD fit?](#where-docker-and-cicd-fit)
  - [What is LocalStack in docker-compose.yml?](#what-is-localstack-in-docker-composeyml)
    - [LocalStack = “fake AWS in a Docker container”](#localstack--fake-aws-in-a-docker-container)
  - [How Dockerfile and docker-compose.yml fit together?](#how-dockerfile-and-docker-composeyml-fit-together)
    - [Dockerfile (Api)](#dockerfile-api)
    - [docker-compose.yml – wiring the system together](#docker-composeyml--wiring-the-system-together)

## High Level Overview

For this project, think of it's like **3 Environments**:

1. Pure Local Dev
   - Run API & Worker with `dotnet run` in VS Code
   - DB/Redis/etc can run either locally or in Docker
2. Local "mini-cloud" (docker-compose)
   - API + Worker + Postgres + Redis + LocalStack all run as containers using `docker compose up`
   - This mimics "how it will look on AWS" but still on my machine
3. Real Cloud (AWS)
   - Later: ECS Fargate runs the same Docker images; RDS, ElastiCache, SQS, SES, etc.
     > Day-to-Day developer workflow will mainly bounce between (1) and (2), and CI/CD will be the automated path from (1/2) to (3).

## Outer loop (realistic environment check)

Every so often (e.g., when finish a chunk of work, or before pushing):

- Build & run the full stack in containers: `docker compose up --build`

### Why --build?

- The image that runs `Api.dll` is built from your source by the `Api.Dockerfile`
- If don’t rebuild, the container is still running the old compiled code, so your API won’t reflect your latest changes.

### So:

- Fast iteration: dotnet run (host) + DB/Redis in Docker.
- Realistic check: docker compose up --build (everything in Docker).

> Later, CI will also run a variant of this outer loop on GitHub.

---

## Software Development Life Cycle

![Software Development Life Cycle - Diagram](https://media.geeksforgeeks.org/wp-content/uploads/20240612173423/Phases-of-Agile-SDLC.webp)

In Short:

1. **Requirement Gathering** – requirements, scope ( SPEC-1 doc ).
2. **Design** – architecture, DB design, API design ( Clean Architecture + AWS design ).
3. **Coding** – actually write the C# code.
4. **Testing** – unit, integration, load tests.
5. **Deployment** – ship to AWS.
6. **Operate & Improve (Feedback)** – monitoring, bug fixes, new features.

### Where Docker and CI/CD fit?

A normal loop for each feature/milestone:

1. **Develop locally**
   - Run API/Worker with dotnet run.
   - Talk to Postgres/Redis/LocalStack (either in Docker or on host).
   - Write unit tests and run dotnet test.
2. **Check containerization**
   - docker compose up --build
   - Confirm:
     - API responds on http://localhost:8080/health
     - Swagger at /swagger
     - Worker logs are ticking
3. **Commit & push**
   - Push to GitHub.
4. **CI pipeline (GitHub Actions) kicks in**
   - Restore, build, test.
   - Spin up Postgres/Redis as services in CI.
   - Run tests.
   - (Later) build Docker images and push to ECR.
5. **CD pipeline**
   - On tag or release branch:
     - Take the same images you tested.
     - Deploy to ECS Fargate using Terraform.
     - Run smoke tests on staging/prod.

## What is LocalStack in docker-compose.yml?

Compose file contains:

```yaml
localstack:
  image: localstack/localstack:latest
  environment:
    - SERVICES=sqs,s3,ses
    - AWS_DEFAULT_REGION=ap-southeast-1
  ports: ["4566:4566"]
```

And the API/Worker containers have env vars:

```yaml
- AWS__ServiceURL=http://localstack:4566
- AWS__Region=ap-southeast-1
```

### LocalStack = “fake AWS in a Docker container”

- It emulates S3, SQS, SES, etc.
- When the code uses the AWS SDK (AWSSDK.S3, AWSSDK.SQS, etc.) and points to `AWS__ServiceURL=http://localstack:4566`, it will talk to LocalStack instead of real AWS.

**Why this is awesome:**

- You can develop and test S3 uploads, SQS messaging, and SES emails without creating real AWS resources.
- CI can run against LocalStack, completely offline from AWS, so tests are fast and free.

---

## How Dockerfile and docker-compose.yml fit together?

- **Dockerfile** : _How to build one image (like blueprint or recipe for Api or Worker)_
- **docker-compose.yml** : _How to run multiple containers together_

### Dockerfile (Api)

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

What each part does:

1. `FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base`

   - Start from a lightweight runtime image that can run ASP.NET apps.

2. `FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build`

   - Use a bigger SDK image for building your code (restore, publish).

3. `COPY ./src ./src`

   - Copy your /src folder into the build container.

4. `RUN dotnet restore ... && dotnet publish ... -o /app/publish`

   - Restore NuGet packages and compile/publish the app to /app/publish.

5. `FROM base AS final + COPY --from=build /app/publish`

   - Create a final image only containing the published app + runtime, not the full SDK → smaller & more secure.

6. `ENTRYPOINT ["dotnet", "Api.dll"]`

   - When the container starts, run dotnet `Api.dll`

The Worker Dockerfile is almost the same but uses `Worker.csproj` and `Worker.dll`

> **Dockerfile** = how to turn your repo into a runnable container image.

### docker-compose.yml – wiring the system together

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
```

**Key ideas:**

- `build` → tells Docker which Dockerfile to use to build the image.
- `environment` → env vars you’d normally put in appsettings.json or secrets; they configure DB/Redis/AWS.
- `ports: ["8080:8080"]` → map container port 8080 to your host’s 8080 → http://localhost:8080.
- `depends_on` → compose will start db, redis, localstack before starting api.

The other services (db, redis, localstack, worker) are similar:

- `db` uses `postgres:15` and sets `POSTGRES_PASSWORD` etc.
- `redis` uses `redis:`7.
- `worker` builds from `Worker.Dockerfile` and shares the same connection strings.

> Compose = “boot this whole topology for me.”
