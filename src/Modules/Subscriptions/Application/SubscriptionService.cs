using Cinewave.Subscriptions.Contracts;

namespace Cinewave.Subscriptions.Application;

internal sealed class SubscriptionService(ISubscriptionRepository subscriptions, TimeProvider clock, string renewBaseUrl) : ISubscriptions
{
    private const string DefaultPlan = "standard";

    public Task<Entitlement> CheckAsync(string accountId, CancellationToken cancellationToken)
    {
        var sub = subscriptions.ForAccount(accountId);
        if (sub is null)
        {
            return Task.FromResult(new Entitlement(accountId, EntitlementStatus.None, null, null, null, RenewUrl(DefaultPlan)));
        }

        var status = sub.IsActiveAt(clock.GetUtcNow()) ? EntitlementStatus.Active : EntitlementStatus.Expired;
        return Task.FromResult(new Entitlement(accountId, status, sub.PlanName, sub.MonthlyPrice, sub.PaidUntil, RenewUrl(sub.PlanCode)));
    }

    private string RenewUrl(string planCode) => $"{renewBaseUrl}?plan={Uri.EscapeDataString(planCode)}";
}
