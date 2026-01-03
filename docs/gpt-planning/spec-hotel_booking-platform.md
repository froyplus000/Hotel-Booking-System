# SPEC-1-Hotel Booking Platform

- [SPEC-1-Hotel Booking Platform](#spec-1-hotel-booking-platform)
  - [Background](#background)
  - [Requirements](#requirements)
    - [Must Have](#must-have)
    - [Should Have](#should-have)
    - [Could Have](#could-have)
    - [Won’t Have (MVP)](#wont-have-mvp)
    - [Non-Functional](#non-functional)
  - [Method](#method)
    - [High-Level Architecture (MVP)](#high-level-architecture-mvp)
    - [Key Tables (PostgreSQL)](#key-tables-postgresql)
    - [Critical Flows](#critical-flows)
  - [Implementation](#implementation)
    - [Repo \& Solution Layout](#repo--solution-layout)
    - [Tech Choices](#tech-choices)
    - [CI/CD (GitHub Actions)](#cicd-github-actions)
    - [Local Dev](#local-dev)
    - [API Surface (initial)](#api-surface-initial)
    - [Testing Strategy](#testing-strategy)
  - [Milestones](#milestones)
  - [Gathering Results](#gathering-results)

## Background

**Confirmed (Nov 28, 2025)**

You are building a portfolio-ready **multi-tenant hotel marketplace (mini‑Agoda)**: partner hotels list inventory; guests search across properties and book. Admins manage hotels, rooms, pricing, and availability. Partners get a portal/API.

Learning goals:

- Design for scalability, reliability, and fault tolerance (resiliency, caching, async workflows, idempotency).
- Demonstrate production-grade DevOps (Docker, automated tests, CI/CD with GitHub Actions, rolling/blue‑green deploys, infra-as-code).
- Showcase cost-conscious, cloud-native AWS architecture suitable for an individual developer.

Assumptions:

- MVP journeys: browse/search, view availability, short-lived hold, create booking with **mock payment** (Stripe optional later), manage booking, partner onboarding & portal.
- Traffic/SLO: sustain **50 req/s**, burst **150 req/s**, scalable to **300 req/s** by horizontal scale + caching; availability **99.9%**; RPO **≤ 1 min**, RTO **≤ 15 min** for booking/availability.
- Geography & currency: **Thailand-only**, time zone **Asia/Bangkok (ICT)**, currency **THB**.
- Performance tests via **k6** or **Locust** with synthetic data; scenario mix (≈90% reads / 10% writes) enforced as CI/CD gates.

## Requirements

### Must Have

- **Guest**: search hotels by location/dates/guests; filter/sort; view room types, photos, amenities, price per night and total.
- **Availability**: accurate per‑room‑type availability; prevent oversell using atomic hold + expiry.
- **Booking**: create/cancel booking; email confirmation; **mock payment** with idempotent booking API.
- **Partner (Hotel)**: onboarding (create property), CRUD room types/rates/inventory, blackout dates; simple bulk upload (CSV/JSON).
- **Admin**: approve partners, moderate listings, view system health.
- **Security**: JWT auth for guests/partners; RBAC (guest, partner, admin); basic rate limiting and input validation.
- **Observability**: structured logs, metrics, traces; audit logs for inventory & bookings.
- **DevOps**: Dockerized services, GitHub Actions CI (build/test, SAST), CD to AWS; IaC for all infra; seed scripts for demo data.

### Should Have

- **Caching**: search results & price cache; CDN for images.
- **Resiliency**: retries with backoff, circuit breakers, timeouts; idempotency keys on write APIs.
- **Partner API**: authenticated endpoints for inventory/pricing updates.
- **Search UX**: pagination, map preview, saved searches.
- **Backoffice**: simple dashboards: occupancy, ADR, booking pace.

### Could Have

- **Payments (Stripe)** in test mode; **promo codes**; **tax/fees** rules per locale.
- **Waitlist** if no availability; **webhooks** for partner updates; **multi-language/currency**.
- **Images**: S3 upload with pre-signed URLs; automatic thumbnail generation.

### Won’t Have (MVP)

- Channel manager integrations (e.g., SiteMinder), dynamic pricing/ML, reviews, loyalty, marketplace payouts.

### Non-Functional

- **Performance**: P50 search < 300 ms (warm cache), P95 < 1 s; booking P95 < 2 s.
- **Data**: soft deletes, PII minimization; backups & PITR; GDPR-friendly data export/delete (manual if needed).
- **Scalability**: horizontal scale for read-heavy paths; async pipelines for thumbnails/emails.
- **Testability**: unit/integration/load tests; synthetic traffic job; ephemeral preview env per PR.

## Method

### High-Level Architecture (MVP)

- **API:** ASP.NET Core .NET 8 Web API (REST, versioned). ALB → **ECS Fargate** (2–3 tasks min, autoscale CPU/RPS).
- **Auth:** **Amazon Cognito** (JWT roles: guest/partner/admin).
- **Data:** **RDS PostgreSQL** (Multi‑AZ for demo prod), **ElastiCache Redis** (search cache + holds), **S3+CloudFront** (images, CSV imports).
- **Async:** **SQS** (emails/imports/expiry), **SES** (email), **EventBridge** (scheduled hold sweeps).
- **Observability/Security:** CloudWatch + X‑Ray (OTel), AWS WAF on ALB.
- **Tenancy:** marketplace with `hotel_id` on all hotel‑owned rows.
- **Data ingestion:** **Portal‑managed + CSV bulk upload** → S3 → SQS → worker.

### Key Tables (PostgreSQL)

`hotels, users, room_types, rate_plans, rate_plan_prices(date, price_minor...), inventory(date, allotment, held, sold), holds(expires_at), bookings, booking_items, payments(mock), idempotency_keys, audit_log, imports`

### Critical Flows

- **Search:** compute availability from `inventory` + `rate_plan_prices`; Redis cache (TTL 30–60s).
- **Hold:** tx per night → check `available = allotment - held - sold` ≥ qty → insert `holds`, increment `inventory.held`, TTL in Redis, SQS `HoldCreated`.
- **Book:** validate idempotency, re‑price, create `bookings`/`items`, move `held → sold`, write `payments(mock)`, enqueue email.
- **Expire:** worker/EventBridge scans expired holds, decrements `held` (idempotent).

## Implementation

### Repo & Solution Layout

- Monorepo (GitHub):
  - `src/Api` (.NET 8 Web API)
  - `src/Worker` (.NET background worker for SQS/imports/expiry)
  - `src/Domain` (entities, value objects, policies)
  - `src/Application` (use cases, validators, DTOs)
  - `src/Infrastructure` (EF Core, Redis, SQS, SES, S3, Cognito auth)
  - `tests/Unit`, `tests/Integration`, `tests/Load` (k6/Locust scripts)
  - `deploy/terraform` (IaC), `deploy/docker` (Dockerfiles, compose for local)
  - `docs/` (ADR, diagrams, seeds)

### Tech Choices

- **.NET 8 + EF Core** for data access; **Polly** for retries; **FluentValidation**; **OpenTelemetry**; **Serilog**.
- **Terraform** for AWS IaC (VPC, ALB, ECS services, RDS, Redis, SQS, SES, Cognito, CloudFront/S3).

### CI/CD (GitHub Actions)

- **CI:** on PR → restore, build, test, linters (dotnet format), SCA (CodeQL), build Docker images, run integration tests (Postgres+Redis in services), publish artifacts & SBOM.
- **CD:** on `main` tag → push images to ECR → Terraform plan/apply → ECS deploy (rolling) → smoke tests → notify.
- **Perf gate:** nightly job runs k6 against staging; fail if P95/SLOs regress.

### Local Dev

- `docker compose up` → API, Worker, Postgres, Redis, LocalStack (SQS/S3/SES mock).
- EF Core migrations, seed script for sample hotels/rooms/prices.

### API Surface (initial)

- Public: `GET /search`, `POST /holds` (Idempotency-Key), `POST /bookings` (Idempotency-Key), `GET/DELETE /bookings/{id}`
- Partner: `PUT /room-types/{id}`, `PUT /rate-plans/{id}`, `POST /hotels/{id}/inventory/import` (pre‑signed S3)

### Testing Strategy

- **Unit** (policies/pricing), **Integration** (EF Core + containers), **Contract** (HTTP via WireMock.Net), **Load** (k6/Locust), **Chaos-lite** (fail Redis/SQS locally).

## Milestones

1. **M0 — Project Bootstrap (1–2 days)**: repo, solution skeleton, health endpoint, Docker, compose env.
2. **M1 — Data Model & Migrations (1–2 days)**: key tables, seeds; basic EF repositories.
3. **M2 — Search & Catalog (2–3 days)**: list hotels/rooms, Redis cache, basic filters.
4. **M3 — Inventory & Holds (3–4 days)**: inventory model, atomic hold API, expiry worker.
5. **M4 — Booking (3–4 days)**: booking API, idempotency, mock payment, email.
6. **M5 — Partner CSV Import (2–3 days)**: S3 presign, SQS import, upserts, logs.
7. **M6 — Observability & SLOs (1–2 days)**: OTel traces, dashboards, alarms.
8. **M7 — CI/CD & AWS Deploy (3–4 days)**: ECR, ECS Fargate, RDS, Redis, Cognito, CloudFront/S3 via Terraform; blue/green later.
9. **M8 — Load Test & Hardening (2–3 days)**: k6 scripts, WAF rules, rate limits, backoff/circuit breakers.
10. **M9 — Polish & Docs (1–2 days)**: README, ADRs, diagrams, demo script.

## Gathering Results

- **SLOs:** Search P95 < 1s; Booking P95 < 2s; error rate < 1%.
- **Perf target:** sustain 50 req/s, burst 150; verify with k6.
- **Reliability drills:** terminate ECS task during booking (idempotency holds), RDS failover test, Redis outage fallback.
- **Cost check:** monthly estimate and rightsizing.
