# Handover

What the client needs to own this without us.

- [ ] Source, infrastructure-as-code and pipelines in the client's own accounts
- [ ] Every secret rotated and owned by the client
- [ ] Architecture and decisions explained (`architecture.md`, `decisions/`)
- [ ] Runbook rehearsed by a client engineer
- [ ] Known issues and next steps listed below
- [ ] AI agent setup documented so their team can keep using it

## Known issues
- No authentication yet: profile ids are in the URL (demo switcher). Do not deploy publicly before OIDC.
- In-memory stores: restarting the API loses progress (slice 2 adds Postgres).
- All titles share one demo stream; no signed CDN URLs yet.

## Recommended next steps
- Slice 2 persistence, then OIDC sign-in, then the hosted payment page and renewal webhook.