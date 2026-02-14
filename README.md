# Routiq 🌍✈️

**AI-Free, Deterministic Travel Route Planner** built with .NET 10, React, and PostgreSQL.

Routiq generates optimized multi-city travel itineraries based on your passport, budget, and trip duration — using real visa rules, flight data, and attraction costs. No AI guessing, just smart algorithms.

---

## 🏗️ Architecture

```
routiq/
├── Routiq.Api/          # .NET 10 Web API (Backend)
│   ├── Controllers/     # REST endpoints (Auth, Routes)
│   ├── Data/            # DbContext, DbInitializer, Seeders
│   ├── DTOs/            # Request/Response models
│   ├── Entities/        # Domain models (Destination, Flight, Attraction, User, VisaRule)
│   ├── Services/        # Business logic (RouteGenerator, CostService, AuthService)
│   └── SeedData/        # JSON seed files (flights.json, attractions.json)
└── Routiq.Web/          # React + Vite + Tailwind (Frontend)
    ├── src/
    │   ├── api/         # Axios API client
    │   ├── components/  # UI components (HeroInput, RouteCard, Charts)
    │   ├── context/     # Auth context (JWT)
    │   └── pages/       # Login, Register, Dashboard
    └── public/
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (Latest LTS)
- [PostgreSQL](https://www.postgresql.org/) (Running locally or via Docker)

### 1. Clone & Setup Database
```bash
git clone https://github.com/your-username/routiq.git
cd routiq
```

Configure your PostgreSQL connection string in `Routiq.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=routiq;Username=postgres;Password=yourpassword"
  }
}
```

### 2. Start the Backend API 🧠
```bash
cd Routiq.Api
dotnet ef database update   # Apply migrations
dotnet run                  # Starts at http://localhost:5107
```

The database is **automatically seeded** on first run with:
- 🌍 16 Destinations (Balkans, Western Europe, Southeast Asia, Turkey)
- 🛂 13 Visa Rules (Turkish passport)
- ✈️ 26 Flights (with price ranges)
- 🏛️ 36 Attractions (with costs & durations)
- 👤 1 Admin User

### 3. Start the Frontend Web App 🎨
```bash
cd Routiq.Web
npm install   # Only needed the first time
npm run dev   # Starts at http://localhost:5173
```

---

## 🔐 Authentication

| Account | Email | Password |
|---------|-------|----------|
| Admin | admin@routiq.com | Admin123! |

JWT-based authentication with protected routes. Register new accounts via `/register`.

---

## 🧪 How to Test

1. Open **http://localhost:5173** in your browser
2. Log in with the admin credentials above (or register a new account)
3. Fill in the **Trip Planner** form:
   - Passport Country: `Turkey`
   - Budget: `$1000`
   - Duration: `7 days`
4. Click **"Find Routes"**
5. View generated travel routes with cost breakdowns and interactive charts

---

## 📦 Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 10, Entity Framework Core, PostgreSQL |
| Auth | JWT (BCrypt password hashing) |
| Frontend | React 18, Vite, Tailwind CSS |
| UI | Framer Motion, Recharts, Lucide Icons |
| API Client | Axios |

---

## 🗃️ Seed Data

Seed data lives in `Routiq.Api/SeedData/` as JSON files:
- **`flights.json`** — Flight routes with min/avg/max price ranges and currency
- **`attractions.json`** — City attractions with estimated costs and durations

The seeder (`FlightAttractionSeeder.cs`) runs automatically on startup and only inserts data if the tables are empty. Core reference data (destinations, visa rules, admin user) is handled by `DbInitializer.cs`.

---

## 📄 License

This project is for educational and personal use.
