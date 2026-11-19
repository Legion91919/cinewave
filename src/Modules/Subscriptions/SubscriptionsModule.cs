using Cinewave.Subscriptions.Application;
using Cinewave.Subscriptions.Contracts;
using Cinewave.Subscriptions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Cinewave.Subscriptions;

/// <summary>Registers the Subscriptions module. Its only public surface is <see cref="ISubscriptions"/> and the Contracts records.</summary>
public static class SubscriptionsModule
{
    /// <param name="renewBaseUrl">Where viewers renew: the hosted payment page entry point.</param>
    public static IServiceCollection AddSubscriptionsModule(this IServiceCollection services, string renewBaseUrl, bool seedDemoData)
    {
        services.AddSingleton(sp =>
        {
            var repo = new InMemorySubscriptionRepository();
            if (seedDemoData)
            {
                repo.SeedDemo(sp.GetRequiredService<TimeProvider>().GetUtcNow());
            }

            return repo;
        });
        services.AddSingleton<ISubscriptionRepository>(sp => sp.GetRequiredService<InMemorySubscriptionRepository>());
        services.AddSingleton<ISubscriptions>(sp => new SubscriptionService(sp.GetRequiredService<ISubscriptionRepository>(), sp.GetRequiredService<TimeProvider>(), renewBaseUrl));
        return services;
    }
}
