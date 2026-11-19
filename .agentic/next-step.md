# Next step

- **Doing:** nothing in progress; slice 1 is done.
- **Done so far:** Sketch leg (`ux/`), slice 1 API (4 modules + composition root, 49 offline tests), web (browse, title detail, hls.js player, resume prompt, paywall; 10 vitest tests), CI web job, `docs/DESIGN.md`.
- **Next action:** start slice 2. Add `src/Modules/<Module>/Infrastructure/Postgres*Repository.cs` behind the existing internal interfaces, one schema per module, EF Core migrations checked in; keep `tests/Cinewave.Tests` green and add Testcontainers integration tests.
- **Blocked by:** NuGet is blocked in the sandbox (EF Core/Testcontainers need a machine with nuget.org).
