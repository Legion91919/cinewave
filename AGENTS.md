# AGENTS.md

Cinewave: The distributor's catalogue of 600 African films only reaches cinemas and festivals; audiences abroad and at home can't stream them legally.

## Read first
Everything about this project lives in the hidden `.agentic/` folder.
Start with `.agentic/README.md`; it says which file answers which question.
Load only the files your task needs.

## What is live
- Slice 1: territory-filtered catalogue, play -> CDN HLS manifest, cross-device resume (idempotent progress PUTs), 402 + renew link.
- API `src/Cinewave.Api` (port 5104) + modules `src/Modules/{Catalogue,Playback,Subscriptions,Profiles}`; web `web/` (React + hls.js). How to run: `.agentic/runbook.md`.

## Hard rules
- Work in thin vertical slices: one ticket from `.agentic/backlog.md` at a time.
- No change without a test that fails first. Run the full test suite before you finish.
- A decision that is hard to reverse gets an ADR in `.agentic/decisions/`.
- Never commit secrets, real client data or personal data. Use `.env.example`.
- Do not grow this file. New knowledge goes in the right `.agentic/` file; history goes in `.agentic/changelog.md`.
- When you learn something that would help on the next project, add it to `.agentic/lessons.md`.
