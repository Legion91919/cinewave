using Cinewave.Catalogue.Domain;

namespace Cinewave.Catalogue.Application;

internal interface ITitleRepository
{
    IReadOnlyList<Title> All();

    Title? Find(string id);

    IReadOnlyList<CatalogueCollection> Collections();
}
