using Cinewave.Profiles.Domain;

namespace Cinewave.Profiles.Application;

internal interface IProfileRepository
{
    Profile? FindProfile(string id);

    Account? FindAccount(string id);

    IReadOnlyList<Profile> AllProfiles();
}
