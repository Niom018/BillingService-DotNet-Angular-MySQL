# Billing Service

A full-stack invoicing system: create orders, record how they were paid
(cash, card, or mobile financial services like bKash/Nagad/Rocket/Upay), and
generate a downloadable PDF invoice. Built with .NET 10, Angular, and MySQL,
and deployed live on AWS.

**Live demo:** http://13.49.85.30
(seeded login: `admin@billingservice.local` / `Admin@12345`)

## Features

- JWT authentication with role-based authorization (Admin / Manager / Cashier)
- Product and customer management
- Order creation with multiple line items, price snapshotting per line
- Order lifecycle: Pending -> Confirmed -> Completed, enforced by domain rules
- Payment recording for Cash, Card, or MFS (with provider + transaction
  reference for bKash/Nagad/Rocket/Upay)
- On-demand PDF invoice generation and download
- Global exception handling, structured logging (Serilog), Swagger docs
- Angular frontend covering the full flow: login, products, customers,
  order creation, payment, invoice download
- Deployed on AWS (EC2 + nginx + MySQL), provisioned via Terraform

## Stack

- **API**: ASP.NET Core 10 Web API, ASP.NET Identity + JWT auth, Serilog, Swagger
- **Data**: EF Core 9.0.x + Pomelo MySQL provider
- **PDF**: QuestPDF (pinned to 2024.12.1, Arial font - see note below)
- **Frontend**: Angular 18 (standalone components, signals)
- **Infra**: Terraform (AWS EC2, security group, elastic IP), nginx reverse proxy
- **Tests**: xUnit, FluentAssertions

## Why EF Core 9.0.x on a net10.0 app

As of mid-2026 the official `Pomelo.EntityFrameworkCore.MySql` package still
has an open tracking issue for EF Core 10 support
([PomeloFoundation/Pomelo.EntityFrameworkCore.MySql#2007](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/issues/2007)).
EF Core 9.0.x packages run fine on a `net10.0` target, so that's what this
project pins to for now.

## Why QuestPDF is pinned to 2024.12.1 with Arial

Newer QuestPDF versions combined with the default bundled font produced
invoices with badly clipped right-aligned text (columns and totals cut off
mid-word). Downgrading QuestPDF and explicitly setting the font to Arial
resolved it. Worth re-testing on a newer QuestPDF release before upgrading.

## Project layout
BillingService/
src/
BillingService.Domain/ entities, enums - no framework dependencies
BillingService.Application/ DTOs, services, validation, interfaces
BillingService.Infrastructure/ EF Core, Identity, MySQL, repositories
BillingService.Api/ controllers, auth, PDF generation, Program.cs
tests/
BillingService.Tests/ xUnit tests against the Domain layer
billing-ui/ Angular frontend
BillingService-Infra/ Terraform infra for AWS deployment

## Getting set up locally

```bash
cd BillingService
dotnet new sln -n BillingService
dotnet sln add src/BillingService.Domain/BillingService.Domain.csproj
dotnet sln add src/BillingService.Application/BillingService.Application.csproj
dotnet sln add src/BillingService.Infrastructure/BillingService.Infrastructure.csproj
dotnet sln add src/BillingService.Api/BillingService.Api.csproj
dotnet sln add tests/BillingService.Tests/BillingService.Tests.csproj
dotnet restore
dotnet build
```

**Local secrets** (like a `.env` file): copy
`src/BillingService.Api/appsettings.Development.json.example` to
`appsettings.Development.json` in that same folder and fill in a real MySQL
password, JWT key, and seed-admin credentials. This file is gitignored.

**Database:**
```bash
dotnet tool install --global dotnet-ef
cd src/BillingService.Api
dotnet ef migrations add InitialCreate --project ../BillingService.Infrastructure --startup-project .
dotnet ef database update --project ../BillingService.Infrastructure --startup-project .
```

**Run the API:**
```bash
dotnet run --project src/BillingService.Api
```
Check `GET /health`, then `/swagger` in Development mode. Log in via
`POST /api/Auth/login` with your seeded admin, click Authorize in Swagger
with the returned token, and try the endpoints.

**Run the frontend:**
```bash
cd billing-ui
npm install
npm start
```
Opens at http://localhost:4200. Make sure `apiUrl` in
`src/environments/environment.ts` points at your running API.

## Deploying to AWS

See [`BillingService-Infra/README.md`](./BillingService-Infra/README.md) for
the full step-by-step: provisioning a free-tier EC2 instance with Terraform,
publishing and uploading the API and Angular build, running the database
migration remotely, and starting everything via systemd + nginx.

## Roadmap

- [x] **Phase 1** - Domain entities, EF Core configurations, DbContext, API
      skeleton with auth wiring, first unit tests
- [x] **Phase 2** - Application services (OrderService, PaymentService),
      FluentValidation rules, AutoMapper profiles
- [x] **Phase 3** - API controllers (Auth, Products, Customers, Orders,
      Payments), role-based authorization, global exception handling
- [x] **Phase 4** - QuestPDF invoice generator wired to `IInvoicePdfGenerator`
- [x] **Phase 5** - Angular frontend: order entry, payment method picker
      (cash/card/MFS with provider sub-select), invoice download
- [x] **Phase 6** - Live AWS deployment via Terraform (EC2, nginx, MySQL)
- [ ] **Phase 7** - CI/CD: auto-deploy on push via GitHub Actions
- [ ] Nice-to-haves: `GET /api/Orders` list endpoint, integration tests
      (`WebApplicationFactory`), Docker Compose for local dev, HTTPS on the
      live deployment (currently HTTP only)

## Data model notes

- `Payment.Method` is `Cash`, `Card`, or `Mfs`. When it's `Mfs`,
  `Payment.MfsProvider` records which provider (bKash, Nagad, Rocket, Upay)
  and `TransactionReference` stores the provider's transaction ID - this is
  what makes "how the payment was collected" queryable and shows up on the
  invoice PDF.
- `OrderItem` snapshots `UnitPrice` at the moment of purchase, so a later
  price change on `Product` never rewrites historical invoices.
- `Order.Items` is intentionally read-only from outside the entity - use
  `Order.AddItem(product, quantity)` so business rules (e.g. "can't modify a
  confirmed order") can't be bypassed.
- Public self-registration always grants the least-privilege `Cashier` role;
  Admin/Manager accounts are created via an admin-only endpoint.