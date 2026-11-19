# Runbook

## Run locally
API (port 5104, seeded invented data in Development):
```bash
dotnet build src/Cinewave.Api/Cinewave.Api.csproj        # offline here: add --configfile /home/claude/tools/offline-nuget.config
ASPNETCORE_URLS=http://localhost:5104 ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Cinewave.Api --no-build
curl localhost:5104/health/ready
```
Web (Vite on 5173, proxies `/api` and `/cdn` to 5104):
```bash
cd web && npm ci && npm run dev     # open http://localhost:5173
```
Demo: pick a profile (Adaeze NG, Thandi ZA, Kofi GB, Wanjiru KE = expired) and a device ("On phone"/"On TV"). Watch on phone, pause at 12:30, switch to TV: Continue watching and the resume prompt say 12:30.

## Run tests
```bash
dotnet test                                    # CI (real xUnit)
/home/claude/tools/run-xunit-offline.sh tests/Cinewave.Tests src/Cinewave.Api/Cinewave.Api.csproj \
  src/Modules/Catalogue/Cinewave.Catalogue.csproj src/Modules/Playback/Cinewave.Playback.csproj \
  src/Modules/Subscriptions/Cinewave.Subscriptions.csproj src/Modules/Profiles/Cinewave.Profiles.csproj   # offline sandbox
cd web && npx vitest run && npm run build
```

## Demo media
`cdn-origin/demo/testpattern/` holds a 16 s invented test pattern in two renditions (360p, 180p, 4 s segments, ~1.2 MB), looped by the playlists into a 20-minute VOD. Regenerate with `tools/make-demo-media.sh` (needs ffmpeg; without it the committed files are used). Every seeded title points at this one stream.

## Configuration
| Key | Default | Meaning |
|---|---|---|
| `Cdn:BaseUrl` | `/cdn` | Prefix of manifest URLs returned by play. Production: the CDN host. |
| `Cdn:ServeLocalOrigin` | `true` | Serve `cdn-origin/` as static files under `/cdn`. Off in production. |
| `Cdn:LocalPath` | `<bin>/cdn-origin` | Folder for the local origin. |
| `Subscriptions:RenewBaseUrl` | `/account/renew` | Renew link in 402 responses. |
| `Seed:DemoData` | true in Development | Seed invented catalogue, profiles, subscriptions, progress. |

## Deploy
Not yet (slice 2+): one container for the API, HLS renditions in blob storage behind a CDN, `Cdn:ServeLocalOrigin=false`.

## Monitor and alerts
`/health/live`, `/health/ready`. SLOs in `quality.md`. Page on: play success rate burn (5xx or manifest 404s), progress PUT error rate.

## Common incidents
- **Viewers in one country see an empty catalogue**: licence windows ended or territory wrong on the account; check `GET /api/profiles/{id}/browse` `territory`.
- **Play returns 402 for a paying viewer**: subscription `paidUntil` not extended (renewal webhook missing, slice 3).
- **Player spins forever**: manifest URL unreachable on the CDN; `/health/ready` reports the local origin; check CDN logs.
