# Notes Application

A full-stack notes app built with:

- Frontend: Vue 3 + TypeScript + Tailwind
- Backend: ASP.NET Core Web API + Dapper
- Database: SQL Server

## Submission Links

- GitHub Repository: `https://github.com/Alchemyyyy/note_application.git`
- GitHub Pages URL: `https://alchemyyyy.github.io/note_application/`

## Run Locally

### Backend

```bash
dotnet run --project backend/NotesApp.Api/NotesApp.Api.csproj --urls http://localhost:5005
```

### Frontend

```bash
cd frontend
npm install
npm run dev
```

## Tests

```bash
dotnet test backend/NotesApp.Tests/NotesApp.Tests.csproj -v minimal
```

## GitHub Pages Deployment

This repo includes `.github/workflows/deploy-pages.yml` to deploy frontend on push to `main`.

### Required one-time GitHub settings

1. Go to `Repository Settings -> Pages`.
2. Under `Build and deployment`, set `Source` to `GitHub Actions`.
3. Push to `main`.

Pages URL:

`https://alchemyyyy.github.io/note_application/`
