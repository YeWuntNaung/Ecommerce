# Bazaar MM

A multi-vendor marketplace for Myanmar: independent vendors get their own storefront, admins
review every product before it goes live, prices are whole MMK kyat, and shipping weights are
handled in kilograms.

- **Frontend** — Angular 22 (standalone components, SSR, zoneless, NgRx Signals, Tailwind v4)
- **Backend** — .NET 10 modular monolith (clean architecture, PostgreSQL, EF Core, cookie auth)

## Current scope

This repository is a working **vendor/catalog/moderation walking skeleton**:

- Seeded storefront with approved and pending products
- Username/password sign-in with role-based access (`Admin`, `VendorOwner`, `Customer`)
- Vendor workspace: create a multi-variant draft, submit it for review, see rejection feedback
- Admin moderation queue: approve or reject with a reason
- Public storefront and product detail pages that only expose approved products
- Domain events recorded to a transactional outbox
- Unit, architecture, and Testcontainers integration tests

Not built yet (deliberate next phases): cart, orders, weighted shipping quotes, manual payment
proof verification, commission models, vendor onboarding applications, media uploads, email.
They are listed in the [roadmap](#roadmap).

## Architecture

### Backend — one deployable, strict layers

| Project | Responsibility | May reference |
| --- | --- | --- |
| `Ecommerce.Domain` | Entities, value objects, invariants, domain events. No framework code. | nothing |
| `Ecommerce.Contracts` | DTOs shared with the API and clients. | nothing |
| `Ecommerce.Application` | Use-case handlers, validators, ports (`IProductRepository`, `IClock`, …). | Domain, Contracts |
| `Ecommerce.Infrastructure` | EF Core, PostgreSQL, Identity, outbox publisher. Implements application ports. | Application, Domain, Contracts |
| `Ecommerce.Api` | Composition root, endpoints, middleware, auth wiring. | all of the above |

Rules enforced by `tests/Ecommerce.ArchitectureTests`:

- Domain and Contracts never depend outward.
- Application never references EF Core, ASP.NET Core, Infrastructure, or Api.
- Api does not reach into persistence.

There is no MediatR or AutoMapper. Handlers are ordinary injected classes that return an explicit
`Result`/`Result<T>`: expected failures are values, exceptions mean bugs. FluentValidation handles
input validation, and `DomainException` is translated to ProblemDetails.

Modules are folders inside each project (`Catalog`, `Vendors`, …) with a schema per module in
PostgreSQL (`catalog`, `vendors`, `identity`, `messaging`). This keeps the door open to split a
module into a service later without paying the distributed-systems tax today.

### Frontend — four layers, ports and adapters

| Alias | Layer | Responsibility |
| --- | --- | --- |
| `@domain` | Domain | Plain TypeScript models, formatting helpers, and abstract gateway ports. |
| `@application` | Application | NgRx Signals stores orchestrating use cases through the ports. |
| `@infrastructure` | Infrastructure | `HttpClient` adapters that implement the gateway ports. |
| `@presentation` | Presentation | Standalone components; templates bind to stores, never to `HttpClient`. |
| `@core` | Composition | DI bindings, API base URL, route guards. |

`presentation → application → domain`, with `infrastructure` implementing `domain` ports and
`core` wiring them together. Swapping REST for another transport means changing
`src/app/core/providers.ts` only.

## Repository layout

```
Ecommerce.slnx
Directory.Build.props / Directory.Packages.props   Central package versions
src/
  Ecommerce.Domain/           primitives, vendors, catalog
  Ecommerce.Contracts/        request/response DTOs
  Ecommerce.Application/      handlers, validators, ports
  Ecommerce.Infrastructure/   EF Core, Identity, migrations, outbox
  Ecommerce.Api/              Program.cs + vertical endpoint files
frontend/
  src/app/domain/             models + gateway ports
  src/app/application/        signal stores
  src/app/infrastructure/     HTTP adapters
  src/app/presentation/       pages and shared UI
  src/app/core/               DI, guards, API base URL
tests/
  Ecommerce.Domain.Tests/         21 tests
  Ecommerce.Application.Tests/     9 tests
  Ecommerce.ArchitectureTests/     5 tests
  Ecommerce.Api.IntegrationTests/  7 tests (real Postgres via Testcontainers)
compose.yaml                        PostgreSQL (+ future Redis/MinIO/Mailpit)
deploy/                             Caddy + systemd examples
```

## Run locally

### Prerequisites

- .NET SDK **10.0.401** (pinned in `global.json`)
- Node.js **24+** and npm 11
- Docker (for PostgreSQL and the integration tests)

### 1. Start PostgreSQL

```bash
docker compose up -d
```

### 2. Start the API

```bash
dotnet tool restore
dotnet run --project src/Ecommerce.Api
```

- API: <http://localhost:5080>
- Health: <http://localhost:5080/health>
- OpenAPI UI (Scalar): <http://localhost:5080/scalar/v1>

Migrations and seed data run automatically on startup (`Database:AutoMigrate=true`).

### 3. Start the Angular app

```bash
cd frontend
npm install
npm start
```

- App: <http://localhost:4200>
- `proxy.conf.json` forwards `/api` and `/health` to `:5080`, so the browser sees one origin and
  the auth cookie behaves exactly as it will in production.

### Seeded accounts

| Role | Username | Password | Lands on |
| --- | --- | --- | --- |
| Admin | `admin` | `Admin123!` | `/admin/moderation` |
| Vendor owner | `vendor` | `Vendor123!` | `/vendor` |
| Demo storefront | — | — | `/store/demo-store` |

The seed creates one approved product (Wireless Earbuds) and one pending product (Smart Watch)
so both sides of the approval flow are visible immediately.

## Tests

```bash
# Backend: domain + application + architecture + integration (Docker required)
dotnet test

# Frontend: formatting, error mapping, and signal-store behaviour
cd frontend && npm test
```

Integration tests use `Testcontainers.PostgreSql` and the real composition root, so login,
cookie auth, role policies, storefront visibility, and the outbox are covered end to end.

## API surface

| Method | Path | Access |
| --- | --- | --- |
| `POST` | `/api/v1/auth/login` | anonymous |
| `POST` | `/api/v1/auth/logout` | authenticated |
| `GET` | `/api/v1/auth/me` | anonymous (401 when signed out) |
| `POST` | `/api/v1/auth/register` | anonymous (creates a Customer) |
| `GET` | `/api/v1/categories` | anonymous |
| `GET` | `/api/v1/stores/{slug}` | anonymous |
| `GET` | `/api/v1/stores/{slug}/products` | anonymous, approved only |
| `GET` | `/api/v1/stores/{slug}/products/{productSlug}` | anonymous, approved only |
| `GET` | `/api/v1/vendor/me` | `VendorOwner` |
| `GET` | `/api/v1/vendor/products` | `VendorOwner` |
| `POST` | `/api/v1/vendor/products` | `VendorOwner` |
| `POST` | `/api/v1/vendor/products/{id}/submit` | `VendorOwner` |
| `GET` | `/api/v1/admin/products/pending` | `Admin` |
| `POST` | `/api/v1/admin/products/{id}/approve` | `Admin` |
| `POST` | `/api/v1/admin/products/{id}/reject` | `Admin` |

Failures are RFC 7807 ProblemDetails. Validation errors carry a field map, which the SPA turns
into a sentence via `toUserMessage`.

## Design decisions

- **Money is `long` whole kyat.** MMK has no circulating subunit, so there is one rounding rule
  (`MidpointRounding.AwayFromZero`) in `Money`. No floating point money anywhere.
- **Weights are integer grams, entered as kilograms.** Shipping bands are defined in kg; storing
  grams avoids boundary drift (0.999 kg vs 1.000 kg is exact).
- **Cookie auth, no tokens in storage.** ASP.NET Core Identity uses an HTTP-only cookie. The dev
  proxy and the production reverse proxy both keep the app same-origin. Guards ask `/auth/me`
  once and cache the answer in `AuthStore`.
- **Admin approval is a domain state machine.** `Draft → PendingReview → Approved/Rejected`, with
  resubmission allowed after rejection and edits blocked while under review.
- **Read models are separate from repositories.** Storefront and moderation queries never load
  aggregates; list screens stay fast and persistence concerns stay out of controllers.
- **Transactional outbox.** Domain events are written in the same transaction as the state
  change and published by a background worker, so "saved but never notified" cannot happen.
- **No payment gateway.** Payments are manual by product decision; staff verification and
  commission calculations are part of the orders/ledger phase.
- **Demo data is gated.** `Seed:DemoData=false` bootstraps only an admin account (from
  `Seed:AdminPassword`) and creates no demo storefront.

## Configuration

`appsettings.json` holds safe local defaults. Override with environment variables in deployment
(`:` becomes `__`). See `.env.example` for the full set.

| Key | Meaning |
| --- | --- |
| `ConnectionStrings__Default` | PostgreSQL connection string |
| `Database__AutoMigrate` | Apply migrations and seed on startup |
| `Cors__AllowedOrigins__0` | Dev proxy origin; empty in production (same-origin) |
| `ReverseProxy__Enabled` | Trust loopback `X-Forwarded-*` headers from Caddy |
| `Seed__DemoData` | Create the demo store, products, and demo users |
| `Seed__AdminPassword` | Password for the seeded/bootstrapped admin |
| `API_BASE_URL` | Absolute API origin used by the SSR process |
| `NG_ALLOWED_HOSTS` | Public hostname(s) the SSR process will render for (no scheme/port) |
| `NG_TRUST_PROXY_HEADERS` | Forwarding headers trusted from the local reverse proxy |

Angular 22 refuses to render a request whose `Host` header is not allow-listed, as SSRF
protection. Behind Caddy the SSR process therefore runs with `NG_ALLOWED_HOSTS` set to the
public hostname (see `deploy/bazaar-web.service`); without it every page returns HTTP 400.

## OpenAPI client generation

`openapi-typescript@7.13` declares a `typescript@^5` peer, while Angular 22 ships TypeScript 6.
That is why generation is not part of the build. With the API running:

```bash
# one-off install that tolerates the peer range
npm install -D openapi-typescript@7.13 --legacy-peer-deps

# generates src/app/infrastructure/api/generated/schema.d.ts
npm run generate:api
```

If the peer mismatch becomes painful, `@hey-api/openapi-ts` is the drop-in alternative. The
hand-written DTOs in `frontend/src/app/domain` are intentionally kept in sync with
`Ecommerce.Contracts` by the typed `HttpClient` adapters.

## Deployment — single-origin Ubuntu VM

The target is one small Ubuntu VM running PostgreSQL, the API, the Angular SSR server, and Caddy.
Caddy terminates TLS and proxies `/api` and `/health` to Kestrel, everything else to Node. One
origin means no CORS in production and a same-site auth cookie.

```bash
# 1. Infrastructure
# Node 24+ is required by Angular 22; Ubuntu's packaged nodejs is usually too old.
curl -fsSL https://deb.nodesource.com/setup_24.x | sudo -E bash -
sudo apt install -y dotnet-runtime-10.0 nodejs caddy
docker compose up -d

# 2. API (published build, not the SDK)
dotnet publish src/Ecommerce.Api/Ecommerce.Api.csproj -c Release -o /srv/bazaar/api

# 3. Frontend (SSR build)
cd frontend
npm ci
npm run build
sudo mkdir -p /srv/bazaar/web
sudo cp -r dist/frontend/* /srv/bazaar/web/

# 4. Services and proxy
sudo cp deploy/bazaar-api.service deploy/bazaar-web.service /etc/systemd/system/
sudo cp deploy/Caddyfile.example /etc/caddy/Caddyfile   # edit the hostname
sudo sed -i 's/bazaar.example.com/your-domain.example/' /etc/systemd/system/bazaar-web.service
sudo systemctl daemon-reload
sudo systemctl enable --now bazaar-api bazaar-web caddy
```

Set `/srv/bazaar/api/.env` from `.env.example` with:

- a strong `ConnectionStrings__Default` password,
- `Cors__AllowedOrigins__0` removed (same-origin),
- `ReverseProxy__Enabled=true`,
- `Seed__DemoData=false` and a one-time `Seed__AdminPassword` to bootstrap the admin.

The web service gets its own environment in `deploy/bazaar-web.service`: `API_BASE_URL`
points at Kestrel, `HOST=127.0.0.1` keeps Node on loopback, and `NG_ALLOWED_HOSTS` must match
the public hostname.

Cloudflare Tunnel is a valid alternative to opening ports 80/443: point the tunnel at Caddy (or
directly at the two local services) and keep the VM firewalled to SSH only.

### Before calling it production

- Change or remove every seeded credential; validate the password policy.
- Add antiforgery tokens to cookie-authenticated state-changing endpoints (SameSite=Lax is the
  current mitigation; this is **required** before high-risk production use).
- Add rate limiting to login and registration.
- Put database backups and a migration runbook in place.
- Replace the outbox logger with real handlers (email, notifications) and add retry/backoff.
- Wire MinIO for product media and Redis if/when carts or rate limits need shared state.
- Set up structured log retention and health-check alerting (`/health`).

## Roadmap

1. **Orders** — cart, checkout, order state machine, vendor sub-orders.
2. **Shipping** — weight-band rules per vendor/region, quoted at checkout (kg-based).
3. **Payments** — customer chooses bank transfer / wallet / COD; proof upload; staff verification.
4. **Commission** — admin commission model (dropdown presets or free text), ledger and payouts.
5. **Vendor onboarding** — application form, slug reservation, approval workflow.
6. **Media** — MinIO-backed product images with moderation.
7. **Notifications** — outbox consumers for email and in-app events.
