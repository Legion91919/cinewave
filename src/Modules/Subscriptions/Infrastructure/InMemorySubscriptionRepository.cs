using System.Collections.Concurrent;
using Cinewave.Subscriptions.Application;
using Cinewave.Subscriptions.Contracts;
using Cinewave.Subscriptions.Domain;

namespace Cinewave.Subscriptions.Infrastructure;

internal sealed class InMemorySubscriptionRepository : ISubscriptionRepository
{
    private readonly ConcurrentDictionary<string, Subscription> _byAccount = new(StringComparer.Ordinal);

    public void Save(Subscription subscription) => _byAccount[subscription.AccountId] = subscription;

    public Subscription? ForAccount(string accountId) => _byAccount.GetValueOrDefault(accountId);

    /// <summary>Invented demo subscriptions; prices are illustrative.</summary>
    public void SeedDemo(DateTimeOffset now)
    {
        Save(new Subscription("acc-adaeze", "standard", "Standard", new Money(2500m, "NGN"), now.AddDays(20)));
        Save(new Subscription("acc-thandi", "standard", "Standard", new Money(99m, "ZAR"), now.AddDays(12)));
        Save(new Subscription("acc-kofi", "premium", "Premium (4K, 4 screens)", new Money(8.99m, "GBP"), now.AddDays(27)));
        Save(new Subscription("acc-wanjiru", "standard", "Standard", new Money(499m, "KES"), now.AddDays(-3)));
    }
}
