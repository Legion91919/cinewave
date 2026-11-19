using Cinewave.Subscriptions.Contracts;

namespace Cinewave.Subscriptions.Domain;

internal sealed record Subscription(string AccountId, string PlanCode, string PlanName, Money MonthlyPrice, DateTimeOffset PaidUntil)
{
    /// <summary>Paid-up until (exclusive) <see cref="PaidUntil"/>. No grace period in slice 1.</summary>
    public bool IsActiveAt(DateTimeOffset at) => at < PaidUntil;
}
