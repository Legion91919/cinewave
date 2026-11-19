using Cinewave.Catalogue.Contracts;
using Cinewave.Catalogue.Domain;

namespace Cinewave.Catalogue.Application;

internal sealed class CatalogueService(ITitleRepository titles, TimeProvider clock) : ICatalogue
{
    public Task<BrowsePage> BrowseAsync(string territory, CancellationToken cancellationToken)
    {
        var here = Territory.Parse(territory);
        var now = clock.GetUtcNow();
        var licensed = titles.All().Where(t => t.IsLicensedIn(here, now)).ToList();

        var rows = titles.Collections()
            .OrderBy(c => c.Order)
            .Select(c => new CatalogueRow(c.Id, c.Title, licensed.Where(t => t.Collections.Contains(c.Id)).Select(t => t.ToSummary()).ToList()))
            .Where(r => r.Items.Count > 0)
            .ToList();

        var hero = (licensed.FirstOrDefault(t => t.Featured) ?? licensed.FirstOrDefault())?.ToSummary();
        return Task.FromResult(new BrowsePage(here.Code, hero, rows));
    }

    public Task<TitleLookup> FindAsync(string titleId, string territory, CancellationToken cancellationToken)
    {
        var title = titles.Find(titleId);
        if (title is null)
        {
            return Task.FromResult(new TitleLookup(TitleAvailability.NotFound, null, null));
        }

        var licensed = title.IsLicensedIn(Territory.Parse(territory), clock.GetUtcNow());
        return Task.FromResult(licensed
            ? new TitleLookup(TitleAvailability.Available, title.ToSummary(), title.StreamPath)
            : new TitleLookup(TitleAvailability.NotLicensed, null, null));
    }

    public Task<IReadOnlyList<TitleSummary>> AvailableAmongAsync(IEnumerable<string> titleIds, string territory, CancellationToken cancellationToken)
    {
        var here = Territory.Parse(territory);
        var now = clock.GetUtcNow();
        IReadOnlyList<TitleSummary> result = titleIds
            .Select(titles.Find)
            .Where(t => t is not null && t.IsLicensedIn(here, now))
            .Select(t => t!.ToSummary())
            .ToList();
        return Task.FromResult(result);
    }
}
