using Cinewave.Profiles.Contracts;

namespace Cinewave.Api.Endpoints;

internal static class ProfileEndpoints
{
    public static void MapProfileEndpoints(this RouteGroupBuilder api)
    {
        // Slice 1 has no sign-in: the demo lists every profile so a device can pick one. Auth arrives with accounts (see runbook).
        api.MapGet("/profiles", async (IProfiles profiles, CancellationToken ct) => Results.Ok(await profiles.ListAsync(ct)));
    }
}
