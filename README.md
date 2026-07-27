# Billing Service

Production-style invoicing system: create orders, record how they were paid
(cash, card, or mobile financial services like bKash/Nagad/Rocket), and
generate a PDF invoice. Built with .NET 10, Angular, and MySQL.

## Stack

- **API**: ASP.NET Core 10 Web API, ASP.NET Identity + JWT auth, Serilog, Swagger
- **Data**: EF Core 9.0.x + Pomelo MySQL provider
- **PDF**: QuestPDF
- **Frontend**: Angular (added in a later phase)
- **Tests**: xUnit, FluentAssertions

## Why EF Core 9.0.x on a net10.0 app

As of mid-2026 the official `Pomelo.EntityFrameworkCore.MySql` package still
has an open tracking issue for EF Core 10 support
(PomeloFoundation/Pomelo.EntityFrameworkCore.MySql#2007). EF Core 9.0.x
packages run fine on a `net10.0` target, so that's what this project pins to
for now. Check that issue before upgrading - once Pomelo ships a stable EF
Core 10 release, bump `Microsoft.EntityFrameworkCore.Design`,
`Microsoft.AspNetCore.Identity.EntityFrameworkCore`, and
`Pomelo.EntityFrameworkCore.MySql` together.

## Project layout

```
BillingService/
  src/
    BillingService.Domain/          entities, enums - no framework dependencies
    BillingService.Application/     DTOs, service interfaces, validation (phase 2)
    BillingService.Infrastructure/  EF Core, Identity, MySQL
    BillingService.Api/             controllers, auth, Program.cs
  tests/
    BillingService.Tests/           xUnit tests against the Domain layer
```

## Getting set up locally

This was scaffolded without a local .NET SDK, so the solution file (.sln)
isn't included yet - generate it with the .NET CLI:

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

Then:

1. Create a MySQL database (e.g. `billing_service`).
2. Update `src/BillingService.Api/appsettings.json` with your real connection
   string and a real JWT signing key (32+ random characters - don't ship the
   placeholder).
3. Install the EF Core CLI tool if you don't have it: `dotnet tool install --global dotnet-ef`
4. From `src/BillingService.Api`, run:
   ```bash
   dotnet ef migrations add InitialCreate --project ../BillingService.Infrastructure --startup-project .
   dotnet ef database update --project ../BillingService.Infrastructure --startup-project .
   ```
5. `dotnet run --project src/BillingService.Api` and hit `GET /health` to
   confirm it's alive, then check `/swagger` once you're in Development mode.

## Roadmap

- [x] **Phase 1** - Domain entities, EF Core configurations, DbContext, API
      skeleton with auth wiring, first unit tests *(this drop)*
- [ ] **Phase 2** - Application services (OrderService, PaymentService),
      FluentValidation rules, AutoMapper profiles
- [ ] **Phase 3** - API controllers (Auth, Products, Customers, Orders,
      Payments), role-based authorization policies
- [ ] **Phase 4** - QuestPDF invoice generator wired to `IInvoicePdfGenerator`
- [ ] **Phase 5** - Angular frontend: order entry, payment method picker
      (cash/card/MFS with provider sub-select), invoice view/download
- [ ] **Phase 6** - Integration tests (`WebApplicationFactory`), seed data,
      role seeding on startup
- [ ] **Phase 7** - Dockerize (API + Angular + MySQL via docker-compose) -
      you said skip this for now, easy to add later

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
