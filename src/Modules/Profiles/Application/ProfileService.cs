using Cinewave.Profiles.Contracts;
using Cinewave.Profiles.Domain;

namespace Cinewave.Profiles.Application;

internal sealed class ProfileService(IProfileRepository repo) : IProfiles
{
    public Task<ProfileView?> FindAsync(string profileId, CancellationToken cancellationToken)
    {
        var profile = repo.FindProfile(profileId);
        return Task.FromResult(profile is null ? null : ToView(profile));
    }

    public Task<IReadOnlyList<ProfileView>> ListAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<ProfileView> list = repo.AllProfiles().Select(ToView).OfType<ProfileView>().ToList();
        return Task.FromResult(list);
    }

    private ProfileView? ToView(Profile p)
    {
        var account = repo.FindAccount(p.AccountId);
        return account is null ? null : new ProfileView(p.Id, account.Id, p.DisplayName, account.Territory, p.AvatarColour);
    }
}
