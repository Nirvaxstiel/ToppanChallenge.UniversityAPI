# University API - Setup Guide

## Prerequisites

- .NET 6+ SDK
- SQL Server

## Quick Start

1. Clone repo and navigate to project dir
2. Configure `appsettings.json`:

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

3. Reset DB (if needed):

   ```bash
   rm -rf Migrations/
   dotnet ef migrations add Initial
   dotnet ef database update
   ```

4. Run:
   ```bash
   dotnet run
   ```

## Testing

- Access Swagger: `https://localhost/swagger`
- Register → Login → Use JWT in Auth header
- Test endpoints: Universities, Bookmarks

Postman Collections:

- [Download Postman Collecton](New%20Collection.postman_collection.json)

# Package Installation Commands

## Main API Project (UniversityAPI)

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Swashbuckle.AspNetCore
```

## Test Project

```bash
dotnet add package coverlet.collector
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package Moq
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
```

## Service Project

```bash
dotnet add package System.IdentityModel.Tokens.Jwt
```

## Utility Project

```bash
dotnet add package Microsoft.Extensions.Configuration.Abstractions
dotnet add package Microsoft.Extensions.Configuration.Binder
dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions
dotnet add package TinyMapper
```

## Framework Project

```bash
dotnet add package Microsoft.AspNetCore.Http.Abstractions
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

## Secret Management

Sensitive values such as the JWT signing key and database connection string are no longer stored in appsettings.json. Set the following environment variables in your deployment or local environment:

- `TOPPAN_UNIVERSITYAPI_JWT_KEY` — The secret key for JWT token signing
- `TOPPAN_UNIVERSITYAPI_DB_CONNECTION` — The database connection string

For local development, you can use the .NET User Secrets feature or set environment variables directly.

### Using .NET User Secrets (Local Development)

1. **Navigate to your API project directory:**
   ```bash
   cd UniversityAPI
   ```
2. **Initialize user-secrets for your project:**
   ```bash
   dotnet user-secrets init
   ```
   This will add a `UserSecretsId` to your `UniversityAPI.csproj`.
3. **Set your secrets:**
   ```bash
   dotnet user-secrets set "TOPPAN_UNIVERSITYAPI_JWT_KEY" "your-very-secret-key"
   dotnet user-secrets set "TOPPAN_UNIVERSITYAPI_DB_CONNECTION" "your-sql-connection-string"
   ```
4. **Run your project as usual:**
   ```bash
   dotnet run
   ```

> **Note:** User secrets are only used in development and are not deployed or checked into source control.

## Notes

- Run commands in each project's directory
