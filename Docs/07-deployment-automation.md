# Deployment automation and CI notes

This file describes the minimal CI/CD pieces added to help deploy the project according to `Docs/*`.

1) What I added

- `backend/Dockerfile` — multi-stage Dockerfile targeting .NET 10 for `PaymentReport.Api`.
- `frontend/vercel.json` — SPA rewrite to `index.html` (helps Vercel SPA routing).
- `.github/workflows/frontend.yml` — builds frontend and uploads `dist` as artifact.
- `.github/workflows/backend.yml` — builds and publishes backend and uploads `publish` as artifact. Optional Docker push steps are commented and require registry secrets.

2) How to use these workflows

- Frontend: Vercel can integrate directly with GitHub (recommended). If you prefer GitHub Actions to deploy, add a Vercel token and use `amondnet/vercel-action` or similar.
- Backend: Render can build from repo directly using the `backend/Dockerfile`. Alternatively, enable the commented Docker steps and provide `GHCR_TOKEN` (or another registry token) to push images to a registry and configure Render to pull images.

3) Render setup (summary)

- Create 4 Web Services on Render:
  - `training-center-gateway` (root/gateway folder, Dockerfile path if present)
  - `course-schedule-service` (root/services/course-schedule-service)
  - `student-attendance-service` (root/services/student-attendance-service)
  - `payment-report-service` (root/backend) — Dockerfile present in `backend/`

- Set environment variables on each service (see `Docs/05-environment-variables.md`).

4) Vercel setup (summary)

- Create a Vercel project, point Root Directory at `frontend`, set Build Command `npm run build` and Output Directory `dist`.
- Add `VITE_API_BASE_URL` in Vercel environment variables.

5) Azure SQL and migrations

- Create Azure SQL server and 3 databases (see `Docs/04-azure-sql-setup-guide.md`).
- Provide connection strings as `ConnectionStrings__DefaultConnection` in Render env per service.
- Migration strategy:
  - Option A (recommended for demo): run `dotnet ef database update` from CI or locally before starting service.
  - Option B: enable migration-on-start in `Program.cs` (current code recreates DB). For production, prefer explicit migrations.

6) Secrets

- Do not commit secrets. Use Render/Vercel/Azure secret management.

7) Next steps I can do for you

- Create a PR with these changes (I can push to your fork or to the origin if you give access).
- Add Render service templates or a `render.yaml` if you want Render to auto-sync (requires Render Dashboard access).
