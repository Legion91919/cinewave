namespace Cinewave.Subscriptions.Contracts;

/// <summary>An amount of money. Always decimal, always with an ISO 4217 currency.</summary>
public sealed record Money
{
    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Money can't be negative.", nameof(amount));
        }

        if (string.IsNullOrEmpty(currency) || currency.Length != 3 || !currency.All(c => c is >= 'A' and <= 'Z'))
        {
            throw new ArgumentException($"'{currency}' is not an ISO 4217 currency code.", nameof(currency));
        }

        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public string Currency { get; }
}

public enum EntitlementStatus
{
    Active,
    Expired,
    None,
}

/// <summary>Whether an account may play titles right now. <see cref="RenewUrl"/> points at the hosted payment page (card data never touches our servers).</summary>
public sealed record Entitlement(string AccountId, EntitlementStatus Status, string? PlanName, Money? MonthlyPrice, DateTimeOffset? PaidUntil, string RenewUrl);

/// <summary>The Subscriptions module's public surface.</summary>
public interface ISubscriptions
{
    Task<Entitlement> CheckAsync(string accountId, CancellationToken cancellationToken);
}
