using Cinewave.Profiles.Application;
using Cinewave.Profiles.Contracts;
using Cinewave.Profiles.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Cinewave.Profiles;

/// <summary>Registers the Profiles module. Its only public surface is <see cref="IProfiles"/> and the Contracts records.</summary>
public static class ProfilesModule
{
    public static IServiceCollection AddProfilesModule(this IServiceCollection services, bool seedDemoData)
    {
        services.AddSingleton(_ =>
        {
            var repo = new InMemoryProfileRepository();
            if (seedDemoData)
            {
                repo.SeedDemo();
            }

            return repo;
        });
        services.AddSingleton<IProfileRepository>(sp => sp.GetRequiredService<InMemoryProfileRepository>());
        services.AddSingleton<IProfiles>(sp => new ProfileService(sp.GetRequiredService<IProfileRepository>()));
        return services;
    }
}
