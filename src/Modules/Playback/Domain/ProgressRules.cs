using Cinewave.Playback.Contracts;

namespace Cinewave.Playback.Domain;

internal static class ProgressRules
{
    /// <summary>A device clock may run ahead by this much; beyond it, a write could "win" forever, so it is rejected.</summary>
    public static readonly TimeSpan MaxClockSkew = TimeSpan.FromMinutes(5);

    /// <summary>Below this many seconds a title counts as not started.</summary>
    public const double StartedAfterSeconds = 10;

    /// <summary>At or above this share of the runtime a title counts as finished.</summary>
    public const double FinishedAtShare = 0.95;

    public static IReadOnlyDictionary<string, string[]> Validate(ProgressUpdate update, DateTimeOffset now)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (update.DurationSeconds <= 0 || double.IsNaN(update.DurationSeconds) || double.IsInfinity(update.DurationSeconds))
        {
            errors["durationSeconds"] = ["Duration must be a positive number of seconds."];
        }

        if (update.PositionSeconds < 0 || double.IsNaN(update.PositionSeconds) || update.PositionSeconds > update.DurationSeconds + 1)
        {
            errors["positionSeconds"] = ["Position must be between 0 and the duration."];
        }

        if (string.IsNullOrWhiteSpace(update.DeviceId) || update.DeviceId.Length > 64)
        {
            errors["deviceId"] = ["A device id (1-64 characters) is required."];
        }

        if (update.ClientTimestamp > now + MaxClockSkew)
        {
            errors["clientTimestamp"] = ["The client timestamp is too far in the future."];
        }

        return errors;
    }

    /// <summary>Last write wins by client timestamp. Equal timestamps are a replay and change nothing.</summary>
    public static bool ShouldReplace(ProgressRecord? existing, ProgressUpdate incoming) =>
        existing is null || incoming.ClientTimestamp > existing.ClientTimestamp;

    public static bool IsInProgress(double position, double duration) =>
        position >= StartedAfterSeconds && position < duration * FinishedAtShare;

    public static double ResumePosition(double position, double duration) =>
        position >= duration * FinishedAtShare ? 0 : position;
}
