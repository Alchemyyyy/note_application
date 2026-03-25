# Notes Application

A full-stack notes app built with:

- Frontend: Vue 3 + TypeScript + Tailwind
- Backend: ASP.NET Core Web API + Dapper
- Database: SQL Server

## Submission Links

- GitHub Repository: `https://github.com/<your-username>/<your-repo>`
- GitHub Pages URL: `https://<your-username>.github.io/<your-repo>/`

Replace placeholders after creating your GitHub repository and enabling Pages.

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

This repo includes a workflow at `.github/workflows/deploy-pages.yml` that deploys the frontend automatically on pushes to `main`.

### Required one-time GitHub settings

1. Go to `Repository Settings -> Pages`.
2. Under `Build and deployment`, set `Source` to `GitHub Actions`.
3. Push to `main`.

After deployment, your Pages URL will be:

`https://<your-username>.github.io/<repo-name>/`
