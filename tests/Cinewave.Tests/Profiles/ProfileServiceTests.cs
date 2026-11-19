using Cinewave.Profiles.Application;
using Cinewave.Profiles.Domain;
using Cinewave.Profiles.Infrastructure;

namespace Cinewave.Profiles.Tests;

public class ProfileServiceTests
{
    [Fact]
    public async Task Profile_inherits_its_accounts_territory()
    {
        var repo = new InMemoryProfileRepository();
        repo.AddAccount(new Account("acc-1", "ZA"));
        repo.AddProfile(new Profile("p-1", "acc-1", "Thandi", "#6b3a5c"));
        var service = new ProfileService(repo);

        var profile = await service.FindAsync("p-1", CancellationToken.None);

        Assert.NotNull(profile);
        Assert.Equal("ZA", profile!.Territory);
        Assert.Equal("acc-1", profile.AccountId);
        Assert.Null(await service.FindAsync("p-unknown", CancellationToken.None));
    }
}
