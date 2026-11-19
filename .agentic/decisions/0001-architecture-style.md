# 0001. Architecture style

- Status: Accepted
- Date: 2026-11-19

## Context

The distributor's catalogue of 600 African films only reaches cinemas and festivals; audiences abroad and at home can't stream them legally. Launch traits: MediaDelivery, ReadHeavy, PersonalData, MoneyMovement, SeveralBusinessAreas, SmallTeam. At global scale the advisor recommends Microservices.

## Decision

Launch as a modular monolith (Catalogue, Playback, Subscriptions, Profiles) with HLS renditions in blob storage behind a CDN, playback progress as small idempotent writes, and payments via a hosted provider page. Evolution trigger: Move Playback events to their own ingestion pipeline when progress writes pass ~2,000/second; split Catalogue as a cached read service when browse traffic needs to scale separately (reference #5).

## Alternatives considered

- Microservices plus an event stream from day one (reference #5 at global scale): heavy for 600 titles and five engineers.
- Streaming video through the API: simplest to code, but would cost a fortune in bandwidth and fall over on the first premiere.

## Consequences

- Module boundaries match the future services, so extraction is a deployment change, not a rewrite.
- One deployable and one database for the launch team to run.
