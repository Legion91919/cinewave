namespace Cinewave.Catalogue.Domain;

/// <summary>The right to stream one title in one territory from <see cref="StartsAt"/> (inclusive) to <see cref="EndsAt"/> (exclusive).</summary>
internal sealed record LicenceWindow
{
    public LicenceWindow(Territory territory, DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        if (endsAt <= startsAt)
        {
            throw new ArgumentException("A licence window must end after it starts.", nameof(endsAt));
        }

        Territory = territory;
        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public Territory Territory { get; }

    public DateTimeOffset StartsAt { get; }

    public DateTimeOffset EndsAt { get; }

    public bool Covers(Territory territory, DateTimeOffset at) =>
        Territory == territory && at >= StartsAt && at < EndsAt;
}
