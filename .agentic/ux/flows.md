# User flows

## Flow 1 (slice 1): browse, play, resume on another device
1. **Open the app** → `screens/browse.html` (`BrowsePage`). Web calls `GET /api/profiles` (profile switcher) then `GET /api/profiles/{profileId}/browse` → hero + rows, only titles whose licensing window covers the account's territory *now*.
2. **See Continue watching** → same screen, first row. `GET /api/profiles/{profileId}/continue-watching` (in-progress titles still licensed in the territory, newest first).
3. **Open a title** → `TitleDetail` panel. `GET /api/profiles/{profileId}/titles/{titleId}` (451 if not licensed in the territory).
4. **Press Play** → `screens/player.html` (`Player`). `POST /api/profiles/{profileId}/titles/{titleId}/play` returns `{ manifestUrl, resumeAtSeconds }`; `manifestUrl` points at the CDN origin (`/cdn/...` locally, a CDN host in production) and the player fetches manifest + segments from there, never from `/api`.
5. **Resume prompt** → `ResumePrompt` when `resumeAtSeconds` is between 30s and 95% of the runtime.
6. **Watch and stop at 12:30** → `Player` sends `PUT /api/profiles/{profileId}/progress/{titleId}` `{ positionSeconds, durationSeconds, deviceId, clientTimestamp }` every 10s, on pause and on exit. Last write wins by `clientTimestamp`; replays and late, older writes are ignored.
7. **Open on another device** → `screens/browse.html`: Continue watching shows "Resume at 12:30 · stopped on your phone"; Play returns `resumeAtSeconds: 750`.
8. **Subscription expired** → Play returns **402** ProblemDetails with `renewUrl` → `PaywallNotice` with a Renew link (hosted payment page, slice 3).
