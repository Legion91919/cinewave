namespace Cinewave.Catalogue.Contracts;

/// <summary>Generated poster art: colours from the design tokens and a pattern name. No photos.</summary>
public sealed record TitleArt(IReadOnlyList<string> Palette, string Pattern);

public sealed record TitleSummary(
    string Id,
    string Name,
    int Year,
    string Country,
    int RuntimeMinutes,
    IReadOnlyList<string> Genres,
    string Synopsis,
    string Director,
    TitleArt Art);

public sealed record CatalogueRow(string Id, string Title, IReadOnlyList<TitleSummary> Items);

public sealed record BrowsePage(string Territory, TitleSummary? Hero, IReadOnlyList<CatalogueRow> Rows);

public enum TitleAvailability
{
    Available,
    NotLicensed,
    NotFound,
}

/// <summary>Result of looking a title up for a territory. <see cref="StreamPath"/> is relative to the CDN origin.</summary>
public sealed record TitleLookup(TitleAvailability Availability, TitleSummary? Title, string? StreamPath);

/// <summary>The Catalogue module's public surface: what is licensed where, right now.</summary>
public interface ICatalogue
{
    Task<BrowsePage> BrowseAsync(string territory, CancellationToken cancellationToken);

    Task<TitleLookup> FindAsync(string titleId, string territory, CancellationToken cancellationToken);

    Task<IReadOnlyList<TitleSummary>> AvailableAmongAsync(IEnumerable<string> titleIds, string territory, CancellationToken cancellationToken);
}
