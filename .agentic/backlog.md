# Backlog

Vertical slices, smallest valuable first.

## Now
- [x] Sketch leg: tokens + three variants of the slice-1 screen (`sketch-ui`)
- [x] Slice 1: A subscriber browses catalogue rows, plays a film with adaptive streaming, and resumes where they stopped on another device
  - [x] Given a subscriber in a territory, then only titles licensed there are shown
  - [x] Given a title, when play is pressed, then an HLS stream starts from a CDN-style URL (not the API)
  - [x] Given playback stops at 12:30, then 'Continue watching' resumes at 12:30 on another device
  - [x] Given an expired subscription, then play returns 402 with a renew link
  - [x] Test notes: one test per criterion; seeded invented data.

## Next
- [ ] Slice 2: persistence (Postgres via EF Core) behind the same repository interfaces
- [ ] CI green with all gates

## Later
- [ ] Sign-in (OIDC) so profile ids come from a token, not the URL
- [ ] Hosted payment page hand-off and renewal webhook (idempotent, ledger entries)
- [ ] Signed CDN URLs/cookies and a real CDN host in front of blob storage
- [ ] Mood picker on browse (from Sketch variant C) once there is viewing data
- [ ] Playwright e2e for flow 1 in CI (slice 1 was verified with a local Playwright run)
