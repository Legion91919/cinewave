# Product brief

> **Showcase project.** Sahel Pictures (fictional), a film distributor launching subscription streaming of African cinema is a fictional client and every name, user and number here is invented.
> Cinewave is an original video streaming service, built to show how the Baton model handles a consumer-scale product.

## Client
Sahel Pictures (fictional), a film distributor launching subscription streaming of African cinema

## Problem
The distributor's catalogue of 600 African films only reaches cinemas and festivals; audiences abroad and at home can't stream them legally.

## Users
Subscribers (evenings, phones and smart TVs); the distributor's catalogue team (weekly uploads).

## Outcomes
Playback starts in under 2 seconds; viewers can resume on any device; no video bytes pass through the API servers.

## Constraints
Licensing differs per country, so the catalogue is filtered by territory; payment card data never touches our servers.

## Out of scope (first release)
Anything not needed for the outcomes above in the first three slices.

## Traits (for the architecture advisor)
- MediaDelivery: "films in several bitrates for phones to TVs"
- ReadHeavy: "browsing and watching dwarf catalogue changes"
- PersonalData: "accounts, payment references, viewing history"
- MoneyMovement: "monthly subscriptions"
- SeveralBusinessAreas: "catalogue, playback, subscriptions, recommendations"
- SmallTeam: "five engineers"

Global-scale profile used for the evolution plan: MediaDelivery, ReadHeavy, GlobalScale, PersonalData, MoneyMovement.
