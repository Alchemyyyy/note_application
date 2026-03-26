# Note Application (Full Stack)

Full-stack notes application with authentication and user-scoped CRUD notes.

## Links

- Repository: https://github.com/Alchemyyyy/note_application
- Frontend (GitHub Pages): https://alchemyyyy.github.io/note_application/
- Backend API (Render): https://note-application-yf9s.onrender.com

## Features

- Register and login with JWT authentication
- Create, read, update, and delete notes
- Notes are isolated per user
- Search, filter, and sort notes
- Responsive UI (Vue + TailwindCSS)

## Tech Stack

- Frontend: Vue 3, TypeScript, Pinia, Axios, TailwindCSS
- Backend: ASP.NET Core Web API (.NET 8), Dapper
- Database: SQL Server
- Deployment: GitHub Pages (frontend), Render (backend), SQL Server on VPS

## Architecture

```text
Frontend (GitHub Pages)
        ->
Backend API (Render)
        ->
SQL Server (VPS)
```

## Environment Variables

### Backend (Render)

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:10000
PORT=10000

ConnectionStrings__DefaultConnection=Server=YOUR_VPS_IP,1433;Database=NotesDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=True;

JwtSettings__Key=YOUR_SECRET_KEY_AT_LEAST_32_BYTES
JwtSettings__Issuer=notes_app_issuer
JwtSettings__Audience=notes_app_audience
JwtSettings__DurationInMinutes=60

Cors__AllowedOrigins=https://alchemyyyy.github.io,http://localhost:5173,http://127.0.0.1:5173
```

### Frontend (GitHub Actions Variable)

```env
VITE_API_BASE_URL=https://note-application-yf9s.onrender.com
VITE_BASE_PATH=/note_application/
```

## Run Locally

1. Clone repository

```bash
git clone https://github.com/Alchemyyyy/note_application.git
cd note_application
```

2. Start backend on port `5005`

```bash
dotnet run --project backend/NotesApp.Api/NotesApp.Api.csproj --urls http://localhost:5005
```

3. Start frontend on port `5173`

```bash
cd frontend
npm install
npm run dev
```

## Tests

```bash
dotnet test backend/NotesApp.Tests/NotesApp.Tests.csproj -v minimal
```

## Notes

- Render free plan may sleep after inactivity; first request can be slower.
- Frontend uses hash routing for GitHub Pages refresh compatibility.