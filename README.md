# 🔐 Secure Storage Service

Secure Storage Service — a minimal, security-focused file storage REST API built with **.NET 10 / ASP.NET Core**.
Files are encrypted with **AES-GCM** before being stored; authentication is JWT-based. Files are encrypted
per-user with per-file derived keys and stored on disk, while metadata is stored in a database (SQLite by default).


## Features

- User registration and login (JWT authentication)
- Role-based access (`User`, `Admin`)
- File upload (AES-GCM encryption with per-file key derived from master key)
- File download and list with pagination
- File metadata stored in database, encrypted data stored on disk
- Configurable maximum file size and allowed content types

- Swagger UI for API testing


## ⚙️ Prerequisites

- .NET 10 SDK
- `dotnet-ef` tool (global)
- PostgreSQL (or SQLite for local quick start)

- Docker & Docker Compose (optional)


## 🔗 Main Endpoints

- `POST /api/auth/register` – Register new user
    ```
    {
    "username": "alice",
    "password": "P@ssw0rd123",
    "email": "alice@example.com"
    }
    ```

- `POST /api/auth/login` – Login user, returns JWT
    ```
    {
    "username": "alice",
    "password": "P@ssw0rd123"
    }
    ```


- `POST /api/files/upload` – Upload file
    - Headers: 
        ```
        Authorization: Bearer {jwt}
        ```
    - Form-data:
        ```
        file: file to upload
        description: optional description
        ```

- `GET /api/files?page=1&pageSize=10` – List files (pagination)
    - Headers: 
        ```
        Authorization: Bearer {jwt}
        ```

- `GET /api/files/{id}/download` – Download file
    - Headers: 
        ```
        Authorization: Bearer {jwt}
        ```

- `DELETE /api/files/{id}` – Delete file
    - Headers: 
        ```
        Authorization: Bearer {jwt}
        ```


## ▶️ How to run (locally)

1. Setup `appsettings.Development.json` (`appsettings.json`) with connection string and Jwt key
    - Store encryption key securely (32 bytes Base64)
        ```
        dotnet user-secrets init --project .\SecureStorage.Api\
        dotnet user-secrets set "Encryption:Base64Key" "YOUR_BASE64_32_BYTE_KEY"
        ```
    - Optionally, set JWT secret in user-secrets:

        ```
        dotnet user-secrets set "Jwt:Key" "YOUR_JWT_SECRET"
        dotnet user-secrets set "Jwt:Issuer" "SecureStorage"
        dotnet user-secrets set "Jwt:Audience" "SecureStorageClients"
        ```

2. Run migrations
    ``` dotnet ef database update -p src/SecureStorage.Infrastructure/ -s src/SecureStorage.Api/ ```
    
3. Start API
    ``` 
    dotnet run --project src/SecureStorage.Api
    ```
    - Default URLs are

        `http://localhost:5149`

        `https://localhost:7149`

    - To use Swagger
    
        `http://localhost:5149/swagger`
        
        `https://localhost:7149/swagger`


## PowerShell 7+ Example

REGISTER
```
Invoke-RestMethod -Method Post -Uri "http://localhost:5149/api/auth/register" -ContentType "application/json" -Body '{"username":"alice","password":"P@ssw0rd123","email":"alice@example.com"}'
```

LOGIN
```
$jwt = (Invoke-RestMethod -Method Post -Uri "http://localhost:5149/api/auth/login" -ContentType "application/json" -Body '{"username":"alice","password":"P@ssw0rd123"}').token
```

UPLOAD
```
Invoke-RestMethod -Method Post -Uri "http://localhost:5149/api/files/upload" -Headers @{ Authorization = "Bearer $jwt" } -Form @{
    file = Get-Item "YOUR_DISK:\YOUR_PATH\YOUR_FILE"
    description = "YOUR_DESCRIPTION"
}
```

LIST FILES
```
Invoke-RestMethod -Method Get -Uri "http://localhost:5149/api/files?page=1&pageSize=10" -Headers @{ Authorization = "Bearer $jwt" }
```

DOWNLOAD
```
Invoke-WebRequest -Method Get -Uri "http://localhost:5149/api/files/{YOUR_FILE_ID}/download" -Headers @{ Authorization = "Bearer $jwt" } -OutFile "YOUR_DISK:\YOUR_PATH\YOUR_DESIRED_FILENAME.bin"
```

DELETE
```
IN PROCESS
```
