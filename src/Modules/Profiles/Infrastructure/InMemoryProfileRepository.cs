using System.Collections.Concurrent;
using Cinewave.Profiles.Application;
using Cinewave.Profiles.Domain;

namespace Cinewave.Profiles.Infrastructure;

internal sealed class InMemoryProfileRepository : IProfileRepository
{
    private readonly ConcurrentDictionary<string, Account> _accounts = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Profile> _profiles = new(StringComparer.Ordinal);
    private readonly ConcurrentQueue<string> _order = new();

    public void AddAccount(Account account) => _accounts[account.Id] = account;

    public void AddProfile(Profile profile)
    {
        if (_profiles.TryAdd(profile.Id, profile))
        {
            _order.Enqueue(profile.Id);
        }
    }

    public Profile? FindProfile(string id) => _profiles.GetValueOrDefault(id);

    public Account? FindAccount(string id) => _accounts.GetValueOrDefault(id);

    public IReadOnlyList<Profile> AllProfiles() => _order.Select(id => _profiles[id]).ToList();

    /// <summary>Invented demo households.</summary>
    public void SeedDemo()
    {
        AddAccount(new Account("acc-adaeze", "NG"));
        AddAccount(new Account("acc-thandi", "ZA"));
        AddAccount(new Account("acc-kofi", "GB"));
        AddAccount(new Account("acc-wanjiru", "KE"));
        AddProfile(new Profile("p-adaeze", "acc-adaeze", "Adaeze", "#6b3a5c"));
        AddProfile(new Profile("p-tobi", "acc-adaeze", "Tobi", "#2c4a6b"));
        AddProfile(new Profile("p-thandi", "acc-thandi", "Thandi", "#3f5b3a"));
        AddProfile(new Profile("p-kofi", "acc-kofi", "Kofi", "#8a6a1f"));
        AddProfile(new Profile("p-wanjiru", "acc-wanjiru", "Wanjiru", "#7a2e1f"));
    }
}
