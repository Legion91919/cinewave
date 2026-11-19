using Cinewave.Playback.Application;
using Cinewave.Playback.Domain;

namespace Cinewave.Playback.Infrastructure;

/// <summary>Thread-safe in-memory progress store (slice 1). The compare-and-replace runs under one lock so concurrent devices can't interleave.</summary>
internal sealed class InMemoryProgressStore : IProgressStore
{
    private readonly Dictionary<(string Profile, string Title), ProgressRecord> _records = [];
    private readonly object _gate = new();

    public (ProgressRecord Stored, bool Replaced) Upsert(ProgressRecord candidate, Func<ProgressRecord?, bool> shouldReplace)
    {
        lock (_gate)
        {
            var key = (candidate.ProfileId, candidate.TitleId);
            _records.TryGetValue(key, out var existing);
            if (!shouldReplace(existing))
            {
                return (existing!, false);
            }

            _records[key] = candidate;
            return (candidate, true);
        }
    }

    public ProgressRecord? Find(string profileId, string titleId)
    {
        lock (_gate)
        {
            return _records.GetValueOrDefault((profileId, titleId));
        }
    }

    public IReadOnlyList<ProgressRecord> ForProfile(string profileId)
    {
        lock (_gate)
        {
            return _records.Values.Where(r => r.ProfileId == profileId).ToList();
        }
    }
}
