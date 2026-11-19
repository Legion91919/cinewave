using System.Globalization;

namespace Cinewave.Support.Tests;

/// <summary>A TimeProvider the test controls.</summary>
public sealed class ManualClock(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;

    public static ManualClock At(string iso) => new(DateTimeOffset.Parse(iso, CultureInfo.InvariantCulture));

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now = _now.Add(by);
}
