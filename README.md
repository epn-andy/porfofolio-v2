# Portfolio V2 — Setup Guide

## Prerequisites
- .NET 10 SDK
- Node.js 22+
- PostgreSQL 15+

---

## Backend Setup (`/backend`)

### 1. Configure Database & JWT

Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=portfolio_db;Username=postgres;Password=YOUR_PASSWORD"
  },
  "JwtSettings": {
    "SecretKey": "your-256-bit-secret-here",
    "Issuer": "PortfolioAPI",
    "Audience": "PortfolioApp",
    "ExpiryMinutes": 1440
  },
  "AllowedOrigins": ["http://localhost:5173"],
  "AdminSettings": {
    "Email": "admin@yourdomain.com",
    "PlainPassword": "your-admin-password"
  }
}
```

### 2. Apply EF Core Migration

```bash
cd backend
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. Seed Admin User

Add `AdminSettings:PlainPassword` temporarily to `appsettings.json`, then run:
```bash
dotnet run --seed
```
> Remove `PlainPassword` from config after seeding!

### 4. Run API

```bash
dotnet run
# API: https://localhost:5001
```

---

## Frontend Setup (`/frontend`)

```bash
cd frontend
npm install
npm run dev
# App: http://localhost:5173
```

Admin dashboard: `http://localhost:5173/admin/login`

---

## Architecture: The Automated Pipeline

- Light mode: blueprint/architectural diagram aesthetic
- Dark mode: secure developer terminal (IDE-style)
- Job history as a visual pipeline graph
- Tailwind CSS dark mode via `class` strategy

## Security

- JWT in HttpOnly, Secure, SameSite=Strict cookie
- Passwords hashed with BCrypt
- All admin write endpoints require `[Authorize]`
- Markdown sanitized with DOMPurify
- Rate limiting: 60 req/min public, 10 req/min login 

