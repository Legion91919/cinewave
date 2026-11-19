# Components (from `screens/browse.html` and `screens/player.html`)

Every component uses token CSS variables only. States: **L**oading, **E**mpty, e**R**ror, **S**uccess.

| Component | Props | Loading | Empty | Error | Success |
|---|---|---|---|---|---|
| `TopBar` | `profiles`, `activeProfileId`, `territory`, `deviceName`, `onSwitchProfile`, `onSwitchDevice` | logo + skeleton pill | no profiles: "No profiles on this account" | profiles failed: territory pill reads "Territory unknown" | logo, nav, "Films licensed in {country}", profile + device switch |
| `Hero` | `title: TitleCard`, `onPlay`, `onMore` | skeleton lines + button | hidden (no licensed titles) | hidden (page error shown) | art, eyebrow, name, meta, synopsis, Play / More info |
| `ContinueWatchingRow` | `items: ContinueItem[]`, `onResume(titleId)` | 3 still skeletons | row hidden | row hidden, catalogue still shown | stills with "Resume at 12:30", progress bar, "86 min left · stopped on your phone" |
| `CatalogueRow` | `row: { id, title, items: TitleCard[] }`, `onOpen(titleId)` | 4 poster skeletons | row hidden | (handled by page) | heading + horizontal list of `PosterCard` |
| `PosterCard` | `title: TitleCard`, `onOpen` | skeleton block | n/a | n/a | generated gradient art, name, year · country; focusable link |
| `BrowsePage` | `profileId` | skeleton hero + rows | `EmptyNotice` "Nothing licensed here yet" | `ErrorNotice` with retry + reference | hero, Continue watching, rows |
| `TitleDetail` | `profileId`, `titleId`, `onPlay`, `onClose` | skeleton panel | n/a | 451 "Not available in your country" / generic error | art, meta, synopsis, director, Play or "Resume at mm:ss" |
| `Player` | `profileId`, `titleId`, `deviceId`, `onExit` | spinner while `POST /play` + manifest load | n/a | stream error dialog with retry (position kept) | hls.js video (native HLS fallback), progress saved every 10s + on pause/exit |
| `ResumePrompt` | `positionSeconds`, `stoppedOnDevice`, `onResume`, `onRestart` | n/a | not shown when position < 30s or > 95% | n/a | dialog "Resume at 12:30?" |
| `PaywallNotice` | `renewUrl`, `detail`, `onBack` | n/a | n/a | shown for 402 from play | "Your subscription has ended" + Renew link (hosted payment page) |
| `Notice` (Empty/Error) | `kind`, `title`, `message`, `action?` | n/a | empty copy | error copy, `role=alert` | n/a |

Data shapes come from the API (see `flows.md`): `TitleCard { id, name, year, country, runtimeMinutes, genres, synopsis, art { palette, pattern } }`, `ContinueItem { titleId, name, positionSeconds, durationSeconds, deviceId, updatedAt }`.
