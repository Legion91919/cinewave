namespace Cinewave.Playback.Domain;

internal sealed record ProgressRecord(
    string ProfileId,
    string TitleId,
    double PositionSeconds,
    double DurationSeconds,
    string DeviceId,
    string DeviceName,
    DateTimeOffset ClientTimestamp,
    DateTimeOffset ReceivedAt);
