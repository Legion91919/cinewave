using Cinewave.Catalogue.Application;
using Cinewave.Catalogue.Contracts;
using Cinewave.Catalogue.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Cinewave.Catalogue;

/// <summary>Registers the Catalogue module. Its only public surface is <see cref="ICatalogue"/> and the Contracts records.</summary>
public static class CatalogueModule
{
    public static IServiceCollection AddCatalogueModule(this IServiceCollection services, bool seedDemoData)
    {
        services.AddSingleton(sp =>
        {
            var repo = new InMemoryTitleRepository();
            if (seedDemoData)
            {
                CatalogueSeed.Populate(repo, sp.GetRequiredService<TimeProvider>().GetUtcNow());
            }

            return repo;
        });
        services.AddSingleton<ITitleRepository>(sp => sp.GetRequiredService<InMemoryTitleRepository>());
        services.AddSingleton<ICatalogue>(sp => new CatalogueService(sp.GetRequiredService<ITitleRepository>(), sp.GetRequiredService<TimeProvider>()));
        return services;
    }
}
