# ASPNET Ecommerce

## Overview

ASPNET Ecommerce is a layered ASP.NET Core MVC storefront with an admin area, ASP.NET Core Identity, Entity Framework Core, TailwindCSS, and Flowbite.

The current implementation follows these rules:

- Controllers stay thin.
- Business logic lives in `Services/`.
- Persistence goes through `ApplicationDbContext`.
- Cart state lives in session.
- Wishlist, orders, reviews, settings, and slider items live in SQL Server.
- Homepage sections render only when the matching `ShowHomepage*` flag is enabled.
- Product pricing is centralized so catalog, cart, checkout, wishlist, and order snapshots use the same effective price.

## Stack

### Backend

- ASP.NET Core MVC on .NET 10
- Entity Framework Core 10
- ASP.NET Core Identity
- SQL Server
- SignalR

### Frontend

- Razor Views
- TailwindCSS 3
- Flowbite 4

## Main Features

### Storefront

- Register, login, logout, forgot password, reset password
- Browse categories and products
- Search, filter, and sort products
- Product detail, verified reviews, and related content
- Session-based cart
- Wishlist for authenticated users
- Checkout and order confirmation email when SMTP is configured
- Order history, details, lookup, and tracking
- Localization: English and Vietnamese
- Light/dark theme toggle

### Admin Area

- Dashboard
- User and role management
- Category CRUD
- Product CRUD with image upload and direct discount fields
- Order management and status updates
- Review moderation
- System settings management
- Homepage slider management

## Architecture Summary

Request flow:

`Request -> Controller -> Service -> DbContext / Identity / Session / SignalR -> View | Redirect | JSON`

Important source-of-truth boundaries:

- Cart: session
- Wishlist: database per authenticated user
- Auth: Identity + auth cookie
- Orders: database + status history
- Storefront shell data: layout reads services on each request

## Homepage

The homepage currently renders from `Views/Home/Index.cshtml`.

`HomeController.Index()` builds a `HomeViewModel` from:

- public system settings
- active categories when enabled
- new products when enabled
- featured products when enabled
- discounted products when enabled
- active slider items in display order

Homepage sections are controlled by `SystemSetting` flags:

- `ShowHomepageSlider`
- `ShowHomepageCategories`
- `ShowHomepageNewProducts`
- `ShowHomepageFeaturedProducts`
- `ShowHomepageDiscountProducts`

## Pricing Rules

Pricing logic is intentionally centralized:

- Per-product discount uses `IsDiscountActive` + `DiscountPercentage`.
- `ProductPricing` computes `EffectivePrice`, `DiscountAmount`, and `HasActiveDiscount`.
- Cart stores the effective unit price.
- Checkout rebuilds totals on the server.
- Order items snapshot the unit price used at checkout.

This prevents price drift between catalog, cart, checkout, wishlist, and stored orders.

## Local Development

### Prerequisites

- .NET 10 SDK
- Node.js and npm
- SQL Server

### Install frontend dependencies

```powershell
npm install
```

### Build Tailwind

```powershell
npm run css:build
```

Watch mode:

```powershell
npm run css:watch
```

### Run the app

```powershell
dotnet run
```

## Configuration

Default connection string is read from `ConnectionStrings:DefaultConnection`.

For local development, `appsettings.json` currently points to:

```json
Server=localhost\\SQLEXPRESS;Database=ASPNET_Ecommerce;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

For containers, override it with an environment variable:

```text
ConnectionStrings__DefaultConnection
```

## Docker Build

This repo now includes:

- `Dockerfile`
- `.dockerignore`
- `docker-compose.yml`

### Build image

```powershell
docker build -t aspnet-ecommerce .
```

### Run container

Example with a SQL Server connection string injected at runtime:

```powershell
docker run --rm -p 8080:8080 ^
  -e ASPNETCORE_URLS=http://+:8080 ^
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;Database=ASPNET_Ecommerce;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=True" ^
  aspnet-ecommerce
```

Notes:

- The image builds Tailwind during the Docker build stage.
- EF Core migrations run on app startup because `Program.cs` calls `Database.MigrateAsync()`.
- The target database must be reachable when the container starts.

## Docker Compose

`docker-compose.yml` now follows this deployment model:

- `app`: ASP.NET Core app built from `Dockerfile`
- `app-db`: SQL Server for the e-commerce application
- `npm`: Nginx Proxy Manager for reverse proxy, domain routing, and free Let's Encrypt SSL
- `npm-db`: MariaDB used internally by Nginx Proxy Manager

Persistent volumes are configured so database and proxy data survive restarts.

### Start the stack

```powershell
docker compose up --build
```

### Run in background

```powershell
docker compose up --build -d
```

### Stop the stack

```powershell
docker compose down
```

### Reset database volume

```powershell
docker compose down -v
```

Compose defaults:

- Nginx Proxy Manager admin UI: `http://localhost:81`
- Public HTTP: `http://localhost:80`
- Public HTTPS: `https://localhost:443`
- ASP.NET app is exposed only inside the Docker network on `app:8080`
- SQL Server is exposed only inside the Docker network on `app-db:1433`

The app connection string is hard-coded in `docker-compose.yml`:

```text
Server=app-db,1433;Database=ASPNET_Ecommerce;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=True
```

### Nginx Proxy Manager setup

After the stack starts:

1. Open `http://localhost:81`
2. Log in to Nginx Proxy Manager
3. Create a Proxy Host pointing your domain to `app` on port `8080`
4. Request a Let's Encrypt certificate in the NPM UI

The app container does not need to publish port `8080` to the host when requests are routed through NPM.

### What you should change before deploying

Edit `docker-compose.yml` and replace these default passwords:

- SQL Server SA password: `Your_password123`
- NPM database password: `ChangeThisNpmDbPassword123`
- NPM root database password: `ChangeThisNpmRootPassword123`

If you change the SQL Server password, update it in both places inside the compose file:

- `app.environment.ConnectionStrings__DefaultConnection`
- `app-db.environment.MSSQL_SA_PASSWORD`

## Project Notes

- Do not move business rules into controllers.
- Upload validation stays in services.
- Public API returns DTOs, not EF entities.
- Slider rendering uses active items only.
- Admin and storefront should share the same pricing logic.
