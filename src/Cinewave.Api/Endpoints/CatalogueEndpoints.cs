using Cinewave.Catalogue.Contracts;
using Cinewave.Playback.Contracts;
using Cinewave.Profiles.Contracts;

namespace Cinewave.Api.Endpoints;

internal static class CatalogueEndpoints
{
    public static void MapCatalogueEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/profiles/{profileId}/browse", async (string profileId, IProfiles profiles, ICatalogue catalogue, CancellationToken ct) =>
        {
            var profile = await profiles.FindAsync(profileId, ct);
            return profile is null ? Problems.ProfileNotFound(profileId) : Results.Ok(await catalogue.BrowseAsync(profile.Territory, ct));
        });

        api.MapGet("/profiles/{profileId}/continue-watching", async (string profileId, IProfiles profiles, ICatalogue catalogue, IPlayback playback, CancellationToken ct) =>
        {
            var profile = await profiles.FindAsync(profileId, ct);
            if (profile is null)
            {
                return Problems.ProfileNotFound(profileId);
            }

            var progress = await playback.InProgressAsync(profileId, ct);
            var licensed = (await catalogue.AvailableAmongAsync(progress.Select(p => p.TitleId), profile.Territory, ct)).ToDictionary(t => t.Id);
            var items = progress
                .Where(p => licensed.ContainsKey(p.TitleId))
                .Select(p => new ContinueWatchingItem(p.TitleId, licensed[p.TitleId], p.PositionSeconds, p.DurationSeconds,
                    Math.Max(0, p.DurationSeconds - p.PositionSeconds), p.DeviceId, p.DeviceName, p.ClientTimestamp));
            return Results.Ok(items);
        });

        api.MapGet("/profiles/{profileId}/titles/{titleId}", async (string profileId, string titleId, IProfiles profiles, ICatalogue catalogue, IPlayback playback, CancellationToken ct) =>
        {
            var profile = await profiles.FindAsync(profileId, ct);
            if (profile is null)
            {
                return Problems.ProfileNotFound(profileId);
            }

            var lookup = await catalogue.FindAsync(titleId, profile.Territory, ct);
            return lookup.Availability switch
            {
                TitleAvailability.NotFound => Problems.TitleNotFound(titleId),
                TitleAvailability.NotLicensed => Problems.NotLicensed(profile.Territory),
                _ => Results.Ok(new TitleDetail(lookup.Title!, await playback.GetProgressAsync(profileId, titleId, ct))),
            };
        });
    }
}

internal sealed record ContinueWatchingItem(
    string TitleId,
    TitleSummary Title,
    double PositionSeconds,
    double DurationSeconds,
    double RemainingSeconds,
    string DeviceId,
    string DeviceName,
    DateTimeOffset UpdatedAt);

internal sealed record TitleDetail(TitleSummary Title, ProgressView? Progress);
