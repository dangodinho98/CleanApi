# CleanApi

CleanApi is a small full-stack app: an ASP.NET Core JSON API secured with JWT, plus a React single-page app for signing in and managing your own items. In Development, the API also serves Swagger UI for exploring endpoints.

## User story

You want a place to keep track of items that belong to you. To join, you register with an **email**, a **password**, and a **display name**—or you sign in later with email and password. Once you are in, you work with **items**: each one needs a **title** and a **description**. You can browse your list, add new items, and change or delete them when things move on.

## Approach and decisions

Here is how I reasoned about the shape of the solution and a few trade-offs.

**Layers (Domain → Application → Infrastructure → Web)** — I kept the core model and rules separate from SQL and HTTP so the domain stays easy to read and the app layer can be tested without a database. Infrastructure holds ADO.NET and schema bootstrap; the web project is mostly controllers, JWT wiring, and CORS. That split costs a few extra projects but pays off when you want to swap storage or add tests around services.

**`CleanApi.Shared`** — A small class library referenced by the backend and .NET test projects (not the React app) for cross-cutting pieces that should stay aligned: database catalog names and configuration keys (`Constants/Database`), loading SQL connection settings for tests (`TestConfigurationBuilder`), and fluent test data builders (`UserBuilder`, `ItemBuilder`). It keeps those concerns out of Domain/Application while avoiding duplicated magic strings between production and test code.

**Data access** — I used ADO.NET with parameterized commands instead of an ORM so the SQL stays explicit and reviewable, and every user-supplied value goes through parameters (no string-built queries). A small initializer creates the database and tables if they are missing and seeds a demo user so a fresh clone is runnable after you point at a SQL Server instance.

**Auth** — Passwords are hashed with BCrypt behind a small `IPasswordHasher` abstraction so hashing policy lives in one place. The API uses JWT bearer tokens so the SPA can stay stateless on the server side; Swagger gets the same scheme in Development for quick manual checks.

**Frontend** — React with Vite for a fast dev loop and a simple client-side router. The dev server proxies `/api` to the local API to avoid CORS pain during development. The build uses a fixed base path (`/CleanApi/`) and an optional IIS deploy script because static hosting on Windows often sits under a virtual directory—`VITE_API_BASE` bakes in the API URL when the UI and API are on different origins.

**Validation and tests** — Request DTOs use data annotations so invalid payloads return HTTP 400 with a structured validation problem body before work hits the database; application services apply the same rules for domain consistency. Automated tests cover validation, auth and item services, SQL repositories (integration), and HTTP flows where a database is available.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (the web project targets `net10.0`)
- [SQL Server](https://www.microsoft.com/sql-server) (or LocalDB) reachable from your machine
- [Node.js](https://nodejs.org/) 18 or newer

## Backend

### Configuration

1. **Database** — Set `Database:ConnectionString` for your SQL Server instance using [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or environment variables. Example: from `src/backend/CleanApi.Web`, run `dotnet user-secrets set Database:ConnectionString "Server=...;Database=CleanApi;..."`. On startup, the app creates the database and schema if needed and runs a small demo seed.

2. **JWT** — `Jwt:Issuer`, `Jwt:Audience`, and `Jwt:ExpirationMinutes` are in `appsettings.json`. Set **`Jwt:SigningKey`** via user secrets or environment variables (at least 32 characters for HS256). Example: `dotnet user-secrets set Jwt:SigningKey "<your-random-key-at-least-32-chars>"`. The app will not start without it.

### Run

From the repository root:

```bash
dotnet run --project src/backend/CleanApi.Web/CleanApi.Web.csproj
```

Or:

```bash
cd src/backend/CleanApi.Web
dotnet run
```

### URLs

With the default profile in `Properties/launchSettings.json`:

- HTTPS: `https://localhost:7288`
- HTTP: `http://localhost:5288`

In Development, Swagger UI is at `/swagger` (for example `https://localhost:7288/swagger`).

## Frontend

### Install and dev server

```bash
cd src/frontend
npm install
npm run dev
```

Vite serves the app on **port 5173**. The dev config proxies `/api` to `https://localhost:7288` (with certificate validation relaxed for the local dev cert).

Start the **backend first** (or keep it running on port 7288) so API calls from the SPA succeed.

The Vite `base` is `/CleanApi/`, so open the app at a URL like **`http://localhost:5173/CleanApi/`** (paths such as `/CleanApi/items` match the router basename).

### Build and preview (optional)

```bash
npm run build
npm run preview
```

For a static build that talks to an API on another origin, set `VITE_API_BASE` at build time. Local development normally uses relative `/api` URLs and the Vite proxy, so you do not need it for `npm run dev`.

### Serving the UI from IIS (optional)

The SPA is built with Vite `base` **`/CleanApi/`** so it matches an IIS application path such as `http://localhost/CleanApi/`. Point an IIS site or application at the deployed static files (not the repo root).

To build and copy the production bundle plus a small `web.config`, use the PowerShell script from `src/frontend`:

```powershell
cd src/frontend
.\scripts\deploy-iis.ps1 -DeployPath 'C:\inetpub\wwwroot\CleanApi'
```

- **`-DeployPath`** — Folder IIS should serve (created if missing); contents are mirrored from `dist` via `robocopy`.

Run the API wherever you host ASP.NET Core (for example Kestrel during development on port 7288); the static IIS site only needs to reach that API URL from the browser.
