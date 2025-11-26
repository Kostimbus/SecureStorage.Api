# 🔐 Secure Storage Service

Secure Storage Service — a minimal, security-focused file storage REST API built with **.NET 10 / ASP.NET Core**.
Files are encrypted with **AES-GCM** before being stored; authentication is JWT-based. The project demonstrates Clean Architecture, DI, EF Core, Docker and CI-ready structure.

---

## ⚙️ Prerequisites

- .NET 10 SDK
- `dotnet-ef` tool (global)
- PostgreSQL (or SQLite for local quick start)
- Docker & Docker Compose (optional)

---

## 🔗 Main Endpoints

- To be noted...

---

## ▶️ How to run (locally)

1. Setup appsettings.Development.json with connection string and Jwt key

2. Run migrations
    ``` dotnet ef database update -p src/SecureStorage.Infrastructure/ -s src/SecureStorage.Api/ ```
    
3. Start API
    ``` dotnet run --project src/SecureStorage.Api ```