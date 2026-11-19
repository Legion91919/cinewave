namespace Cinewave.Profiles.Contracts;

/// <summary>A viewer profile. <see cref="Territory"/> is the account's country of residence, which decides what is licensed for it.</summary>
public sealed record ProfileView(string Id, string AccountId, string DisplayName, string Territory, string AvatarColour);

/// <summary>The Profiles module's public surface.</summary>
public interface IProfiles
{
    Task<ProfileView?> FindAsync(string profileId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProfileView>> ListAsync(CancellationToken cancellationToken);
}
