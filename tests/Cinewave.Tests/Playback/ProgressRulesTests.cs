using Cinewave.Playback.Contracts;
using Cinewave.Playback.Domain;

namespace Cinewave.Playback.Tests;

public class ProgressRulesTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 19, 0, 0, TimeSpan.Zero);

    private static ProgressUpdate At(double position, DateTimeOffset ts, string device = "phone", double duration = 1200) =>
        new(position, duration, device, device, ts);

    private static ProgressRecord Stored(double position, DateTimeOffset ts) =>
        new("p-1", "t-1", position, 1200, "phone", "phone", ts, ts);

    [Fact]
    public void First_write_always_applies()
    {
        Assert.True(ProgressRules.ShouldReplace(null, At(750, Now)));
    }

    [Fact]
    public void Newer_client_timestamp_wins()
    {
        Assert.True(ProgressRules.ShouldReplace(Stored(120, Now.AddMinutes(-5)), At(750, Now)));
    }

    [Fact]
    public void Older_client_timestamp_is_ignored_even_if_it_arrives_last()
    {
        Assert.False(ProgressRules.ShouldReplace(Stored(750, Now), At(120, Now.AddMinutes(-5), "tv")));
    }

    [Fact]
    public void Replaying_the_same_write_is_a_no_op()
    {
        Assert.False(ProgressRules.ShouldReplace(Stored(750, Now), At(750, Now)));
    }

    [Theory]
    [InlineData(-1, 1200, "phone", 0, "positionSeconds")]
    [InlineData(10, 0, "phone", 0, "durationSeconds")]
    [InlineData(1300, 1200, "phone", 0, "positionSeconds")]
    [InlineData(10, 1200, "", 0, "deviceId")]
    [InlineData(10, 1200, "phone", 10, "clientTimestamp")]
    public void Invalid_updates_are_rejected(double position, double duration, string device, int minutesAhead, string field)
    {
        var errors = ProgressRules.Validate(new ProgressUpdate(position, duration, device, null, Now.AddMinutes(minutesAhead)), Now);

        Assert.Contains(field, errors.Keys);
    }

    [Fact]
    public void Small_clock_skew_is_accepted()
    {
        Assert.Empty(ProgressRules.Validate(At(10, Now.AddMinutes(2)), Now));
    }

    [Theory]
    [InlineData(750, 5880, true)]
    [InlineData(5, 5880, false)]
    [InlineData(5700, 5880, false)]
    public void In_progress_means_started_and_not_nearly_finished(double position, double duration, bool expected)
    {
        Assert.Equal(expected, ProgressRules.IsInProgress(position, duration));
    }

    [Theory]
    [InlineData(750, 1200, 750)]
    [InlineData(1180, 1200, 0)]
    public void Resume_position_restarts_finished_titles(double position, double duration, double expected)
    {
        Assert.Equal(expected, ProgressRules.ResumePosition(position, duration));
    }
}
