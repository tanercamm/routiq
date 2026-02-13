# Routiq 🌍✈️

A modern, AI-powered travel route planner built with .NET 10 and React.

## 🚀 Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (Latest LTS)
- [PostgreSQL](https://www.postgresql.org/) (Running locally or via Docker)

### 1. Start the Backend API 🧠
The backend handles data, authentication, and route generation logic.

```bash
cd Routiq.Api
dotnet run
```
*The API will start at `http://localhost:5107`*

### 2. Start the Frontend Web App 🎨
The frontend is a premium React application built with Vite.

```bash
cd Routiq.Web
npm install  # Only needed the first time
npm run dev
```
*The Web App will start at `http://localhost:5173`*

## 🧪 How to Test
1. Open your browser to **http://localhost:5173**.
2. Fill in the **Trip Planner** form (Example: Passport: "Turkey", Budget: $1000, Duration: 7 Days).
3. Click **"Find Routes"**.
4. View the generated travel routes and costs in the results grid.
