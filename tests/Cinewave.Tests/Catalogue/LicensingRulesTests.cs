using Cinewave.Catalogue.Contracts;
using Cinewave.Catalogue.Domain;

namespace Cinewave.Catalogue.Tests;

public class LicensingRulesTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset End = new(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Window_covers_its_territory_from_start_inclusive_to_end_exclusive()
    {
        var window = new LicenceWindow(Territory.Parse("NG"), Start, End);

        Assert.True(window.Covers(Territory.Parse("NG"), Start));
        Assert.True(window.Covers(Territory.Parse("NG"), End.AddTicks(-1)));
        Assert.False(window.Covers(Territory.Parse("NG"), End));
        Assert.False(window.Covers(Territory.Parse("NG"), Start.AddTicks(-1)));
    }

    [Fact]
    public void Window_never_covers_another_territory()
    {
        var window = new LicenceWindow(Territory.Parse("ZA"), Start, End);

        Assert.False(window.Covers(Territory.Parse("NG"), Start.AddDays(10)));
    }

    [Fact]
    public void Window_must_end_after_it_starts()
    {
        Assert.Throws<ArgumentException>(() => new LicenceWindow(Territory.Parse("KE"), End, Start));
    }

    [Theory]
    [InlineData("ng", "NG")]
    [InlineData(" Za ", "ZA")]
    public void Territory_codes_are_normalised(string raw, string expected)
    {
        Assert.Equal(expected, Territory.Parse(raw).Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("NGA")]
    [InlineData("N1")]
    public void Territory_rejects_anything_but_two_letters(string raw)
    {
        Assert.Throws<ArgumentException>(() => Territory.Parse(raw));
    }

    [Fact]
    public void Title_is_licensed_when_any_of_its_windows_covers_the_territory_now()
    {
        var title = TitleBuilder.Make("t-1", windows:
        [
            new LicenceWindow(Territory.Parse("NG"), Start, Start.AddMonths(1)),
            new LicenceWindow(Territory.Parse("NG"), Start.AddMonths(6), End),
        ]);

        Assert.True(title.IsLicensedIn(Territory.Parse("NG"), Start.AddDays(3)));
        Assert.False(title.IsLicensedIn(Territory.Parse("NG"), Start.AddMonths(3)), "gap between windows");
        Assert.True(title.IsLicensedIn(Territory.Parse("NG"), Start.AddMonths(7)));
    }
}

internal static class TitleBuilder
{
    public static Title Make(string id, IReadOnlyList<LicenceWindow> windows, string[]? collections = null, bool featured = false) =>
        new(id, $"Film {id}", 2024, "Nigeria", 100, ["Drama"], "Synopsis.", "A. Director",
            new TitleArt(["#7a2e1f", "#a5532a"], "stripes"), "demo/testpattern", collections ?? ["festival"], featured, windows);
}
