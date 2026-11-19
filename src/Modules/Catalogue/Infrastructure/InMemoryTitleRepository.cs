using System.Collections.Concurrent;
using Cinewave.Catalogue.Application;
using Cinewave.Catalogue.Domain;

namespace Cinewave.Catalogue.Infrastructure;

/// <summary>Thread-safe in-memory store for slice 1. Slice 2 swaps in Postgres behind the same interface.</summary>
internal sealed class InMemoryTitleRepository : ITitleRepository
{
    private readonly ConcurrentDictionary<string, Title> _titles = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, CatalogueCollection> _collections = new(StringComparer.Ordinal);
    private readonly ConcurrentQueue<string> _order = new();

    public void Add(Title title)
    {
        if (_titles.TryAdd(title.Id, title))
        {
            _order.Enqueue(title.Id);
        }
    }

    public void AddCollection(CatalogueCollection collection) => _collections[collection.Id] = collection;

    public IReadOnlyList<Title> All() => _order.Select(id => _titles[id]).ToList();

    public Title? Find(string id) => _titles.GetValueOrDefault(id);

    public IReadOnlyList<CatalogueCollection> Collections() => _collections.Values.OrderBy(c => c.Order).ToList();
}
