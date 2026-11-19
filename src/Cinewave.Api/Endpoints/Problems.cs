namespace Cinewave.Api.Endpoints;

internal static class Problems
{
    public static IResult ProfileNotFound(string profileId) =>
        Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Profile not found", detail: $"No profile '{profileId}'.");

    public static IResult TitleNotFound(string titleId) =>
        Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Title not found", detail: $"No title '{titleId}'.");

    public static IResult NotLicensed(string territory) =>
        Results.Problem(
            statusCode: StatusCodes.Status451UnavailableForLegalReasons,
            title: "Not available in your country",
            detail: $"This title isn't licensed for streaming in {territory} right now.");
}
