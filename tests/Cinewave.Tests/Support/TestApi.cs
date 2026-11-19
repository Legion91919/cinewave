using System.Net.Http.Json;
using System.Text.Json;
using Cinewave.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cinewave.Support.Tests;

/// <summary>Starts the real API on a random local port with seeded demo data and a manual clock.</summary>
public sealed class TestApi : IAsyncDisposable
{
    private readonly WebApplication _app;

    private TestApi(WebApplication app, ManualClock clock)
    {
        _app = app;
        Clock = clock;
        Http = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
    }

    public HttpClient Http { get; }

    public ManualClock Clock { get; }

    public IServiceProvider Services => _app.Services;

    public static async Task<TestApi> StartAsync(ManualClock? clock = null)
    {
        clock ??= ManualClock.At("2026-11-19T19:00:00Z");
        var app = CinewaveApp.Build(
            ["--urls=http://127.0.0.1:0", "--environment=Development", "--Seed:DemoData=true", "--Logging:LogLevel:Default=Warning"],
            b => b.Services.AddSingleton<TimeProvider>(clock));
        await app.StartAsync();
        return new TestApi(app, clock);
    }

    public async Task<JsonElement> GetJsonAsync(string path)
    {
        var res = await Http.GetAsync(new Uri(path, UriKind.Relative));
        var body = await res.Content.ReadAsStringAsync();
        Assert.True(res.IsSuccessStatusCode, $"GET {path} -> {(int)res.StatusCode}: {body}");
        return JsonDocument.Parse(body).RootElement;
    }

    public Task<HttpResponseMessage> PutProgressAsync(string profileId, string titleId, double position, string deviceId, DateTimeOffset clientTimestamp, double duration = 1200)
        => Http.PutAsJsonAsync(
            new Uri($"/api/profiles/{profileId}/progress/{titleId}", UriKind.Relative),
            new { positionSeconds = position, durationSeconds = duration, deviceId, deviceName = deviceId, clientTimestamp });

    public async ValueTask DisposeAsync()
    {
        Http.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
