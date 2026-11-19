namespace Cinewave.Playback.Contracts;

/// <summary>A progress report from a device. <see cref="ClientTimestamp"/> is when the device observed the position; it decides last-write-wins.</summary>
public sealed record ProgressUpdate(double PositionSeconds, double DurationSeconds, string DeviceId, string? DeviceName, DateTimeOffset ClientTimestamp);

public sealed record ProgressView(
    string ProfileId,
    string TitleId,
    double PositionSeconds,
    double DurationSeconds,
    string DeviceId,
    string DeviceName,
    DateTimeOffset ClientTimestamp,
    DateTimeOffset ReceivedAt);

public enum ProgressWriteOutcome
{
    /// <summary>The update was newer than what we had and is now stored.</summary>
    Applied,

    /// <summary>The update was a replay or older than what we had; the stored position is unchanged.</summary>
    Ignored,

    /// <summary>The update failed validation.</summary>
    Invalid,
}

public sealed record ProgressWriteResult(ProgressWriteOutcome Outcome, ProgressView? Current, IReadOnlyDictionary<string, string[]> Errors);

/// <summary>The Playback module's public surface: playback progress and where streams live on the CDN.</summary>
public interface IPlayback
{
    Task<ProgressWriteResult> SaveProgressAsync(string profileId, string titleId, ProgressUpdate update, CancellationToken cancellationToken);

    Task<ProgressView?> GetProgressAsync(string profileId, string titleId, CancellationToken cancellationToken);

    /// <summary>Titles the profile has started and not finished, most recently watched first.</summary>
    Task<IReadOnlyList<ProgressView>> InProgressAsync(string profileId, CancellationToken cancellationToken);

    /// <summary>Where a player should start: the saved position, or 0 when there is none or the title was finished.</summary>
    double ResumePositionFor(ProgressView? progress);

    /// <summary>The HLS master playlist on the CDN origin for a stream path. Never an API URL.</summary>
    string ManifestPathFor(string streamPath);
}
