using Cinewave.Playback.Domain;

namespace Cinewave.Playback.Infrastructure;

/// <summary>Invented viewing history so "Continue watching" isn't empty on first run.</summary>
internal static class PlaybackSeed
{
    public static void Populate(InMemoryProgressStore store, DateTimeOffset now)
    {
        void Add(string profile, string title, double position, string device, string deviceName, TimeSpan ago) =>
            store.Upsert(new ProgressRecord(profile, title, position, 1200, device, deviceName, now - ago, now - ago), _ => true);

        Add("p-adaeze", "harmattan-letters", 310, "seed-tv", "living-room TV", TimeSpan.FromDays(2));
        Add("p-adaeze", "sixteen-kilometres", 600, "seed-laptop", "laptop", TimeSpan.FromDays(4));
        Add("p-thandi", "red-earth-choir", 420, "seed-phone", "phone", TimeSpan.FromHours(20));
        Add("p-wanjiru", "mama-ndege", 515, "seed-phone", "phone", TimeSpan.FromDays(9));
    }
}
