namespace Cinewave.Profiles.Domain;

/// <summary>A paying household. Holds the minimum personal data: an id and a country.</summary>
internal sealed record Account(string Id, string Territory);

internal sealed record Profile(string Id, string AccountId, string DisplayName, string AvatarColour);
