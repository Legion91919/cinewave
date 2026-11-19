using Cinewave.Catalogue.Contracts;

namespace Cinewave.Catalogue.Domain;

internal sealed record CatalogueCollection(string Id, string Title, int Order);

internal sealed class Title(
    string id,
    string name,
    int year,
    string country,
    int runtimeMinutes,
    IReadOnlyList<string> genres,
    string synopsis,
    string director,
    TitleArt art,
    string streamPath,
    IReadOnlyList<string> collections,
    bool featured,
    IReadOnlyList<LicenceWindow> windows)
{
    public string Id { get; } = id;

    public string StreamPath { get; } = streamPath;

    public IReadOnlyList<string> Collections { get; } = collections;

    public bool Featured { get; } = featured;

    public IReadOnlyList<LicenceWindow> Windows { get; } = windows;

    public bool IsLicensedIn(Territory territory, DateTimeOffset at) => Windows.Any(w => w.Covers(territory, at));

    public TitleSummary ToSummary() => new(Id, name, year, country, runtimeMinutes, genres, synopsis, director, art);
}
