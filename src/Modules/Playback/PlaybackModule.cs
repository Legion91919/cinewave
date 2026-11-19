using Cinewave.Playback.Application;
using Cinewave.Playback.Contracts;
using Cinewave.Playback.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Cinewave.Playback;

/// <summary>Registers the Playback module. Its only public surface is <see cref="IPlayback"/> and the Contracts records.</summary>
public static class PlaybackModule
{
    /// <param name="cdnBaseUrl">Base of the CDN origin that serves HLS files, e.g. "/cdn" locally or "https://media.example" in production.</param>
    public static IServiceCollection AddPlaybackModule(this IServiceCollection services, string cdnBaseUrl, bool seedDemoData)
    {
        services.AddSingleton(sp =>
        {
            var store = new InMemoryProgressStore();
            if (seedDemoData)
            {
                PlaybackSeed.Populate(store, sp.GetRequiredService<TimeProvider>().GetUtcNow());
            }

            return store;
        });
        services.AddSingleton<IProgressStore>(sp => sp.GetRequiredService<InMemoryProgressStore>());
        services.AddSingleton<IPlayback>(sp => new PlaybackService(sp.GetRequiredService<IProgressStore>(), sp.GetRequiredService<TimeProvider>(), cdnBaseUrl));
        return services;
    }
}
