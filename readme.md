```markdown
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
- [Download Postman Collecton](New Collection.postman_collection.json)
