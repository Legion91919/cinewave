using Cinewave.Playback.Contracts;
using Cinewave.Playback.Domain;

namespace Cinewave.Playback.Application;

internal sealed class PlaybackService(IProgressStore store, TimeProvider clock, string cdnBaseUrl) : IPlayback
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors = new Dictionary<string, string[]>();

    public Task<ProgressWriteResult> SaveProgressAsync(string profileId, string titleId, ProgressUpdate update, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var errors = ProgressRules.Validate(update, now);
        if (errors.Count > 0)
        {
            return Task.FromResult(new ProgressWriteResult(ProgressWriteOutcome.Invalid, null, errors));
        }

        var candidate = new ProgressRecord(profileId, titleId, update.PositionSeconds, update.DurationSeconds,
            update.DeviceId, string.IsNullOrWhiteSpace(update.DeviceName) ? update.DeviceId : update.DeviceName, update.ClientTimestamp, now);
        var (stored, replaced) = store.Upsert(candidate, existing => ProgressRules.ShouldReplace(existing, update));
        return Task.FromResult(new ProgressWriteResult(replaced ? ProgressWriteOutcome.Applied : ProgressWriteOutcome.Ignored, ToView(stored), NoErrors));
    }

    public Task<ProgressView?> GetProgressAsync(string profileId, string titleId, CancellationToken cancellationToken)
    {
        var record = store.Find(profileId, titleId);
        return Task.FromResult(record is null ? null : ToView(record));
    }

    public Task<IReadOnlyList<ProgressView>> InProgressAsync(string profileId, CancellationToken cancellationToken)
    {
        IReadOnlyList<ProgressView> list = store.ForProfile(profileId)
            .Where(r => ProgressRules.IsInProgress(r.PositionSeconds, r.DurationSeconds))
            .OrderByDescending(r => r.ClientTimestamp)
            .Select(ToView)
            .ToList();
        return Task.FromResult(list);
    }

    public double ResumePositionFor(ProgressView? progress) =>
        progress is null ? 0 : ProgressRules.ResumePosition(progress.PositionSeconds, progress.DurationSeconds);

    public string ManifestPathFor(string streamPath) => $"{cdnBaseUrl.TrimEnd('/')}/{streamPath.Trim('/')}/master.m3u8";

    private static ProgressView ToView(ProgressRecord r) =>
        new(r.ProfileId, r.TitleId, r.PositionSeconds, r.DurationSeconds, r.DeviceId, r.DeviceName, r.ClientTimestamp, r.ReceivedAt);
}
