using Cinewave.Catalogue.Application;
using Cinewave.Catalogue.Contracts;
using Cinewave.Catalogue.Domain;
using Cinewave.Catalogue.Infrastructure;
using Cinewave.Support.Tests;

namespace Cinewave.Catalogue.Tests;

public class CatalogueServiceTests
{
    private static readonly ManualClock Clock = ManualClock.At("2026-11-19T19:00:00Z");

    private static CatalogueService ServiceWith(params Title[] titles)
    {
        var repo = new InMemoryTitleRepository();
        repo.AddCollection(new CatalogueCollection("festival", "Festival favourites", 1));
        repo.AddCollection(new CatalogueCollection("docs", "Documentaries", 2));
        foreach (var t in titles)
        {
            repo.Add(t);
        }

        return new CatalogueService(repo, Clock);
    }

    private static LicenceWindow Open(string territory) =>
        new(Territory.Parse(territory), Clock.GetUtcNow().AddDays(-30), Clock.GetUtcNow().AddDays(30));

    [Fact]
    public async Task Browse_shows_only_titles_licensed_in_the_viewers_territory()
    {
        var service = ServiceWith(
            TitleBuilder.Make("ng-only", [Open("NG")]),
            TitleBuilder.Make("za-only", [Open("ZA")]),
            TitleBuilder.Make("both", [Open("NG"), Open("ZA")]));

        var page = await service.BrowseAsync("NG", CancellationToken.None);

        var ids = page.Rows.SelectMany(r => r.Items).Select(i => i.Id).ToList();
        Assert.Contains("ng-only", ids);
        Assert.Contains("both", ids);
        Assert.DoesNotContain(ids, id => id == "za-only");
        Assert.Equal("NG", page.Territory);
    }

    [Fact]
    public async Task Browse_hides_expired_and_not_yet_started_windows()
    {
        var now = Clock.GetUtcNow();
        var service = ServiceWith(
            TitleBuilder.Make("expired", [new LicenceWindow(Territory.Parse("NG"), now.AddDays(-90), now.AddDays(-1))]),
            TitleBuilder.Make("future", [new LicenceWindow(Territory.Parse("NG"), now.AddDays(1), now.AddDays(90))]),
            TitleBuilder.Make("current", [Open("NG")]));

        var page = await service.BrowseAsync("NG", CancellationToken.None);

        var ids = page.Rows.SelectMany(r => r.Items).Select(i => i.Id).ToList();
        Assert.Equal("current", Assert.Single(ids));
    }

    [Fact]
    public async Task Rows_without_licensed_titles_are_left_out_and_hero_is_the_featured_title()
    {
        var service = ServiceWith(
            TitleBuilder.Make("doc", [Open("ZA")], ["docs"]),
            TitleBuilder.Make("star", [Open("NG")], ["festival"], featured: true));

        var page = await service.BrowseAsync("NG", CancellationToken.None);

        Assert.Single(page.Rows);
        Assert.Equal("festival", page.Rows[0].Id);
        Assert.Equal("star", page.Hero?.Id);
    }

    [Fact]
    public async Task Find_distinguishes_unknown_titles_from_titles_not_licensed_here()
    {
        var service = ServiceWith(TitleBuilder.Make("za-only", [Open("ZA")]));

        Assert.Equal(TitleAvailability.NotFound, (await service.FindAsync("nope", "ZA", CancellationToken.None)).Availability);
        Assert.Equal(TitleAvailability.NotLicensed, (await service.FindAsync("za-only", "NG", CancellationToken.None)).Availability);
        var found = await service.FindAsync("za-only", "ZA", CancellationToken.None);
        Assert.Equal(TitleAvailability.Available, found.Availability);
        Assert.Equal("demo/testpattern", found.StreamPath);
    }
}
