using Cinewave.Playback.Domain;

namespace Cinewave.Playback.Application;

internal interface IProgressStore
{
    /// <summary>Atomically replaces the record for (profile, title) when <paramref name="shouldReplace"/> says so. Returns the stored record and whether it changed.</summary>
    (ProgressRecord Stored, bool Replaced) Upsert(ProgressRecord candidate, Func<ProgressRecord?, bool> shouldReplace);

    ProgressRecord? Find(string profileId, string titleId);

    IReadOnlyList<ProgressRecord> ForProfile(string profileId);
}
