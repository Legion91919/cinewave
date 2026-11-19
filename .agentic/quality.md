# Quality

## Definition of done
- Acceptance criteria met and demonstrated.
- Unit tests for rules, integration tests for each adapter, one end-to-end test per key flow.
- CI green: build, tests, lint, dependency and secret scan.
- Docs in `.agentic/` updated in the same change.

## Service levels
| SLI | SLO | Why |
|---|---|---|
| Play start success (`POST /play` 2xx or expected 402/451, then manifest 200) | 99.9% over 30 days | the main job |
| Time to first frame (client-reported) | p95 < 2 s | product outcome |
| `POST /play` latency | p95 < 150 ms | part of the 2 s budget |
| Browse latency (`GET /browse`) | p95 < 300 ms | read-heavy home screen |
| Progress PUT success | 99.5% | resume on any device |
| Resume accuracy (other device resumes within 10 s of last position) | 99% | product outcome |
| Video bytes served by API | 0 | product outcome; CDN only |
| API availability | 99.9% over 30 days | |

## Acceptance criteria -> tests (slice 1)
| Criterion | Tests |
|---|---|
| Only titles licensed in the territory | `LicensingRulesTests`, `CatalogueServiceTests`, `Slice1AcceptanceTests.AC1_*` (3) |
| HLS from a CDN-style URL, not the API | `Slice1AcceptanceTests.AC2_*` (manifest + segment served as static files; no `/cdn` or media API routes) |
| Resume at 12:30 on another device | `ProgressRulesTests`, `Slice1AcceptanceTests.AC3_*` (LWW, replay, late older write, validation) |
| Expired subscription -> 402 + renew link | `EntitlementTests`, `Slice1AcceptanceTests.AC4_*` |
| Module boundaries | `ModuleBoundaryTests` |
| Web | vitest: `time.test.ts`, `client.test.ts` (402/451/PUT), `ContinueWatchingRow.test.tsx` (states) |
