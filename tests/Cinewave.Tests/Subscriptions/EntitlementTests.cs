using Cinewave.Subscriptions.Application;
using Cinewave.Subscriptions.Contracts;
using Cinewave.Subscriptions.Domain;
using Cinewave.Subscriptions.Infrastructure;
using Cinewave.Support.Tests;

namespace Cinewave.Subscriptions.Tests;

public class EntitlementTests
{
    private static readonly DateTimeOffset PaidUntil = new(2026, 12, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Subscription_is_active_until_the_paid_until_instant()
    {
        var sub = new Subscription("acc-1", "standard", "Standard", new Money(2500m, "NGN"), PaidUntil);

        Assert.True(sub.IsActiveAt(PaidUntil.AddSeconds(-1)));
        Assert.False(sub.IsActiveAt(PaidUntil));
    }

    [Fact]
    public void Money_is_decimal_with_a_currency()
    {
        var price = new Money(99.99m, "ZAR");

        Assert.Equal(99.99m, price.Amount);
        Assert.Throws<ArgumentException>(() => new Money(1m, "rand"));
        Assert.Throws<ArgumentException>(() => new Money(-1m, "ZAR"));
    }

    [Fact]
    public async Task Expired_entitlement_carries_a_renew_link()
    {
        var repo = new InMemorySubscriptionRepository();
        repo.Save(new Subscription("acc-1", "standard", "Standard", new Money(499m, "KES"), PaidUntil));
        var clock = new ManualClock(PaidUntil.AddDays(1));
        var service = new SubscriptionService(repo, clock, "/account/renew");

        var entitlement = await service.CheckAsync("acc-1", CancellationToken.None);

        Assert.Equal(EntitlementStatus.Expired, entitlement.Status);
        Assert.Equal("/account/renew?plan=standard", entitlement.RenewUrl);
    }

    [Fact]
    public async Task Account_without_subscription_has_no_entitlement()
    {
        var service = new SubscriptionService(new InMemorySubscriptionRepository(), new ManualClock(PaidUntil), "/account/renew");

        var entitlement = await service.CheckAsync("acc-none", CancellationToken.None);

        Assert.Equal(EntitlementStatus.None, entitlement.Status);
        Assert.StartsWith("/account/renew", entitlement.RenewUrl);
    }
}
