# Lessons

Generic, reusable lessons only. No client names, data or secrets.
- An HLS media playlist can reference the same few segments many times (with EXT-X-DISCONTINUITY) to make a long demo stream from a tiny committed asset.
- Inheriting from `TimeProvider` shadows the `System` namespace (it has a static `System` property): add `using System.Globalization;` instead of fully qualified names.
- When filtering browser network logs for "media through the API", Vite dev serves `/src/api/*.ts` modules; filter by content type, not extension.
