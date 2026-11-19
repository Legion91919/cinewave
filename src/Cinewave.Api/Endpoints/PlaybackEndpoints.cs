using Cinewave.Catalogue.Contracts;
using Cinewave.Playback.Contracts;
using Cinewave.Profiles.Contracts;
using Cinewave.Subscriptions.Contracts;

namespace Cinewave.Api.Endpoints;

internal static class PlaybackEndpoints
{
    public static void MapPlaybackEndpoints(this RouteGroupBuilder api)
    {
        // Starts playback: checks licence, then entitlement, then hands back a CDN manifest URL. No media flows through here.
        api.MapPost("/profiles/{profileId}/titles/{titleId}/play", async (
            string profileId, string titleId, HttpContext http,
            IProfiles profiles, ICatalogue catalogue, ISubscriptions subscriptions, IPlayback playback, CancellationToken ct) =>
        {
            var profile = await profiles.FindAsync(profileId, ct);
            if (profile is null)
            {
                return Problems.ProfileNotFound(profileId);
            }

            var lookup = await catalogue.FindAsync(titleId, profile.Territory, ct);
            if (lookup.Availability == TitleAvailability.NotFound)
            {
                return Problems.TitleNotFound(titleId);
            }

            if (lookup.Availability == TitleAvailability.NotLicensed)
            {
                return Problems.NotLicensed(profile.Territory);
            }

            var entitlement = await subscriptions.CheckAsync(profile.AccountId, ct);
            if (entitlement.Status != EntitlementStatus.Active)
            {
                http.Response.Headers.Link = $"<{entitlement.RenewUrl}>; rel=\"payment\"";
                return Results.Problem(
                    statusCode: StatusCodes.Status402PaymentRequired,
                    title: "Subscription required",
                    detail: entitlement.Status == EntitlementStatus.Expired
                        ? $"Your subscription ended on {entitlement.PaidUntil:yyyy-MM-dd}. Renew to keep watching."
                        : "This account has no subscription yet.",
                    extensions: new Dictionary<string, object?> { ["renewUrl"] = entitlement.RenewUrl });
            }

            var progress = await playback.GetProgressAsync(profileId, titleId, ct);
            return Results.Ok(new PlayResponse(
                titleId,
                lookup.Title!.Name,
                playback.ManifestPathFor(lookup.StreamPath!),
                playback.ResumePositionFor(progress),
                progress?.DeviceName,
                progress?.ClientTimestamp));
        });

        // Idempotent: replays and late, older writes are ignored (last write wins by client timestamp).
        api.MapPut("/profiles/{profileId}/progress/{titleId}", async (
            string profileId, string titleId, ProgressUpdate update, IProfiles profiles, IPlayback playback, CancellationToken ct) =>
        {
            if (await profiles.FindAsync(profileId, ct) is null)
            {
                return Problems.ProfileNotFound(profileId);
            }

            var result = await playback.SaveProgressAsync(profileId, titleId, update, ct);
            if (result.Outcome == ProgressWriteOutcome.Invalid)
            {
                return Results.ValidationProblem(result.Errors.ToDictionary(e => e.Key, e => e.Value));
            }

            var c = result.Current!;
            return Results.Ok(new ProgressResponse(result.Outcome == ProgressWriteOutcome.Applied, c.PositionSeconds, c.DurationSeconds, c.DeviceId, c.DeviceName, c.ClientTimestamp));
        });

        api.MapGet("/profiles/{profileId}/progress/{titleId}", async (string profileId, string titleId, IPlayback playback, CancellationToken ct) =>
        {
            var progress = await playback.GetProgressAsync(profileId, titleId, ct);
            return progress is null
                ? Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "No progress saved")
                : Results.Ok(progress);
        });
    }
}

internal sealed record PlayResponse(string TitleId, string TitleName, string ManifestUrl, double ResumeAtSeconds, string? ResumeFromDevice, DateTimeOffset? StoppedAt);

internal sealed record ProgressResponse(bool Applied, double PositionSeconds, double DurationSeconds, string DeviceId, string DeviceName, DateTimeOffset ClientTimestamp);
