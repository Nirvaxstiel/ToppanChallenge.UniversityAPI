# University API - Setup Guide

## 📦 Prerequisites

- [.NET 6+ SDK](https://dotnet.microsoft.com/)
- SQL Server (local or remote instance)

---

## 🚀 Quick Start

1. **Clone the repo** and navigate to the main project:

   ```bash
   git clone <repo-url>
   cd UniversityAPI
   ```

2. **Configure `appsettings.json` (for quick dev setup):**

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=UniversityDB;Trusted_Connection=True;"
   },
   "Jwt": {
     "Key": "your-64-character-secure-key",
     "Issuer": "UniversityAPI",
     "Audience": "UniversityAPIUsers"
   }
   ```

3. **(Optional) Reset the database:**

   ```bash
   rm -rf Migrations/
   dotnet ef migrations add Initial
   dotnet ef database update
   ```

4. **Run the API:**

   ```bash
   dotnet run
   ```

---

## 🧪 Testing the API

- Launch Swagger UI: [https://localhost/swagger](https://localhost/swagger)
- Test Flow:

  1. Register a user
  2. Log in to retrieve a JWT token
  3. Authorize Swagger using the token
  4. Try endpoints (e.g. Universities, Bookmarks)

**Postman:**
Download the collection:
[New Collection.postman_collection.json](New%20Collection.postman_collection.json)

---

## 🔐 Secret Management

> **Avoid storing secrets in `appsettings.json`. Use environment variables or `.NET` user-secrets.**

### Required Environment Variables

| Variable                                   | Description                  |
| ------------------------------------------ | ---------------------------- |
| `TOPPAN_UNIVERSITYAPI_JWT_KEY`             | JWT signing key              |
| `TOPPAN_UNIVERSITYAPI_DB_CONNECTION`       | SQL Server connection string |
| `TOPPAN_UNIVERSITYAPI_ADMIN_INIT_USERNAME` | Seed admin username          |
| `TOPPAN_UNIVERSITYAPI_ADMIN_INIT_EMAIL`    | Seed admin email             |
| `TOPPAN_UNIVERSITYAPI_ADMIN_INIT_PASSWORD` | Seed admin password          |

---

### Using .NET User Secrets (Local Development)

1. Navigate to the API project directory:

   ```bash
   cd UniversityAPI
   ```

2. Initialize user-secrets:

   ```bash
   dotnet user-secrets init
   ```

3. Set the required secrets:

   ```bash
   dotnet user-secrets set "TOPPAN_UNIVERSITYAPI_JWT_KEY" "your-very-secret-key"
   dotnet user-secrets set "TOPPAN_UNIVERSITYAPI_DB_CONNECTION" "your-sql-connection-string"
   dotnet user-secrets set "TOPPAN_UNIVERSITYAPI_ADMIN_INIT_USERNAME" "youradmin"
   dotnet user-secrets set "TOPPAN_UNIVERSITYAPI_ADMIN_INIT_EMAIL" "admin@yourdomain.com"
   dotnet user-secrets set "TOPPAN_UNIVERSITYAPI_ADMIN_INIT_PASSWORD" "YourSecurePassword"
   ```

4. Run the project:

   ```bash
   dotnet run
   ```

> 🔒 **Note:** User secrets are only used in development and never committed to source control.

---

## 🧪 Test Execution Notes

> ✅ Full test automation and parallel execution coming soon.

- For now, run tests **folder-by-folder** to isolate errors and ensure individual success.
- You can use `dotnet test` in specific test folders to target subgroups.
- Later, I'll enable parallelization via `xunit.runner.json` and CI support.

---

## 📦 Package Installation

> Run one of the following methods depending on your scenario.

---

### 🟢 Option 1: Restore All from Project Files (Recommended)

If dependencies are already listed in `.csproj` files (e.g., after cloning the repo), just run:

```bash
dotnet restore
```

> This will install all required NuGet packages across all projects in the solution.

---

### 🟡 Option 2: Manual Installation (For Custom or Fresh Setup)

#### 🔹 UniversityAPI (Main API)

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Swashbuckle.AspNetCore
```

#### 🔸 Test Project

```bash
dotnet add package coverlet.collector
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package Moq
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
```

#### 🔹 Service Project

```bash
dotnet add package System.IdentityModel.Tokens.Jwt
```

#### 🔸 Utility Project

```bash
dotnet add package Microsoft.Extensions.Configuration.Abstractions
dotnet add package Microsoft.Extensions.Configuration.Binder
dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions
dotnet add package TinyMapper
```

#### 🔹 Framework Project

```bash
dotnet add package Microsoft.AspNetCore.Http.Abstractions
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

---

## 📝 Notes

- Run `dotnet add package` in the **correct project directory** if you are doing the manual approach.
