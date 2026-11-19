using System.Net;
using System.Text.Json;
using Cinewave.Support.Tests;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Cinewave.Api.Tests;

/// <summary>One test (or more) per acceptance criterion of slice 1, against the real HTTP pipeline and seeded data.</summary>
public class Slice1AcceptanceTests
{
    private static List<string> TitleIdsIn(JsonElement browse) =>
        browse.GetProperty("rows").EnumerateArray()
            .SelectMany(r => r.GetProperty("items").EnumerateArray())
            .Select(i => i.GetProperty("id").GetString()!)
            .ToList();

    // AC1: Given a subscriber in a territory, then only titles licensed there are shown.
    [Fact]
    public async Task AC1_browse_shows_only_titles_licensed_in_the_viewers_territory()
    {
        await using var api = await TestApi.StartAsync();

        var nigeria = await api.GetJsonAsync("/api/profiles/p-adaeze/browse");
        var southAfrica = await api.GetJsonAsync("/api/profiles/p-thandi/browse");

        Assert.Equal("NG", nigeria.GetProperty("territory").GetString());
        var ng = TitleIdsIn(nigeria);
        var za = TitleIdsIn(southAfrica);
        Assert.Contains("lagos-after-rain", ng);
        Assert.DoesNotContain(za, id => id == "lagos-after-rain");
        Assert.Contains("red-earth-choir", za);
        Assert.DoesNotContain(ng, id => id == "red-earth-choir");
        Assert.DoesNotContain(ng, id => id == "gold-coast-static"); // NG window expired
        Assert.DoesNotContain(ng, id => id == "weavers-of-kano");   // NG window not started
    }

    [Fact]
    public async Task AC1_title_not_licensed_in_the_territory_is_unavailable_for_legal_reasons()
    {
        await using var api = await TestApi.StartAsync();

        var detail = await api.Http.GetAsync(new Uri("/api/profiles/p-adaeze/titles/red-earth-choir", UriKind.Relative));
        var play = await api.Http.PostAsync(new Uri("/api/profiles/p-adaeze/titles/red-earth-choir/play", UriKind.Relative), null);

        Assert.Equal((HttpStatusCode)451, detail.StatusCode);
        Assert.Equal((HttpStatusCode)451, play.StatusCode);
        Assert.Equal("application/problem+json", play.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task AC1_licence_window_closing_removes_the_title_without_a_restart()
    {
        await using var api = await TestApi.StartAsync();
        Assert.Contains("nightbus-to-bulawayo", TitleIdsIn(await api.GetJsonAsync("/api/profiles/p-adaeze/browse")));

        api.Clock.Advance(TimeSpan.FromDays(400)); // past every seeded NG window

        Assert.DoesNotContain(TitleIdsIn(await api.GetJsonAsync("/api/profiles/p-adaeze/browse")), id => id == "nightbus-to-bulawayo");
    }

    // AC2: Given a title, when play is pressed, then an HLS stream starts from a CDN-style URL (not the API).
    [Fact]
    public async Task AC2_play_returns_a_cdn_manifest_url_that_is_served_as_static_hls()
    {
        await using var api = await TestApi.StartAsync();

        var res = await api.Http.PostAsync(new Uri("/api/profiles/p-adaeze/titles/nightbus-to-bulawayo/play", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var play = JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement;
        var manifestUrl = play.GetProperty("manifestUrl").GetString()!;

        Assert.StartsWith("/cdn/", manifestUrl);
        Assert.DoesNotContain("/api/", manifestUrl);
        Assert.True(manifestUrl.EndsWith(".m3u8", StringComparison.Ordinal));

        var master = await api.Http.GetAsync(new Uri(manifestUrl, UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, master.StatusCode);
        Assert.Equal("application/vnd.apple.mpegurl", master.Content.Headers.ContentType?.MediaType);
        var masterText = await master.Content.ReadAsStringAsync();
        Assert.StartsWith("#EXTM3U", masterText);
        Assert.Contains("#EXT-X-STREAM-INF", masterText);

        // Follow the first rendition and fetch its first segment, exactly as a player would.
        var variant = masterText.Split('\n').Select(l => l.Trim()).First(l => l.EndsWith(".m3u8", StringComparison.Ordinal));
        var baseUrl = manifestUrl[..(manifestUrl.LastIndexOf('/') + 1)];
        var media = await api.Http.GetStringAsync(new Uri(baseUrl + variant, UriKind.Relative));
        var segment = media.Split('\n').Select(l => l.Trim()).First(l => l.EndsWith(".ts", StringComparison.Ordinal));
        var variantBase = (baseUrl + variant)[..((baseUrl + variant).LastIndexOf('/') + 1)];
        var seg = await api.Http.GetAsync(new Uri(variantBase + segment, UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, seg.StatusCode);
        Assert.Equal("video/mp2t", seg.Content.Headers.ContentType?.MediaType);
        Assert.True((await seg.Content.ReadAsByteArrayAsync()).Length > 1000);
    }

    [Fact]
    public async Task AC2_no_api_endpoint_serves_media()
    {
        await using var api = await TestApi.StartAsync();

        var routes = api.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Select(e => e.RoutePattern.RawText ?? string.Empty)
            .ToList();

        Assert.NotEmpty(routes);
        Assert.DoesNotContain(routes, r => r.TrimStart('/').StartsWith("cdn", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(routes, r => r.Contains(".m3u8", StringComparison.Ordinal) || r.Contains(".ts", StringComparison.Ordinal));
    }

    // AC3: Given playback stops at 12:30, then 'Continue watching' resumes at 12:30 on another device.
    [Fact]
    public async Task AC3_progress_saved_on_one_device_resumes_at_the_same_position_on_another()
    {
        await using var api = await TestApi.StartAsync();
        var stoppedAt = api.Clock.GetUtcNow();

        var put = await api.PutProgressAsync("p-adaeze", "nightbus-to-bulawayo", 750, "phone", stoppedAt);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        // Another device: continue-watching row and play both resume at 12:30.
        var row = await api.GetJsonAsync("/api/profiles/p-adaeze/continue-watching");
        var item = Assert.Single(row.EnumerateArray().Where(i => i.GetProperty("titleId").GetString() == "nightbus-to-bulawayo"));
        Assert.Equal(750d, item.GetProperty("positionSeconds").GetDouble());
        Assert.Equal("phone", item.GetProperty("deviceName").GetString());

        var play = await api.Http.PostAsync(new Uri("/api/profiles/p-adaeze/titles/nightbus-to-bulawayo/play", UriKind.Relative), null);
        var body = JsonDocument.Parse(await play.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(750d, body.GetProperty("resumeAtSeconds").GetDouble());
    }

    [Fact]
    public async Task AC3_progress_puts_are_idempotent_and_last_write_wins_by_client_timestamp()
    {
        await using var api = await TestApi.StartAsync();
        var t0 = api.Clock.GetUtcNow();

        await api.PutProgressAsync("p-tobi", "the-salt-road", 750, "phone", t0);
        var replay = await api.PutProgressAsync("p-tobi", "the-salt-road", 750, "phone", t0);
        var late = await api.PutProgressAsync("p-tobi", "the-salt-road", 120, "tv", t0.AddMinutes(-3)); // older write arriving late

        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        Assert.Equal(HttpStatusCode.OK, late.StatusCode);
        var lateBody = JsonDocument.Parse(await late.Content.ReadAsStringAsync()).RootElement;
        Assert.False(lateBody.GetProperty("applied").GetBoolean());
        Assert.Equal(750d, lateBody.GetProperty("positionSeconds").GetDouble());

        var stored = await api.GetJsonAsync("/api/profiles/p-tobi/progress/the-salt-road");
        Assert.Equal(750d, stored.GetProperty("positionSeconds").GetDouble());
        Assert.Equal("phone", stored.GetProperty("deviceId").GetString());

        var newer = await api.PutProgressAsync("p-tobi", "the-salt-road", 900, "tv", t0.AddMinutes(2));
        var newerBody = JsonDocument.Parse(await newer.Content.ReadAsStringAsync()).RootElement;
        Assert.True(newerBody.GetProperty("applied").GetBoolean());
        Assert.Equal(900d, newerBody.GetProperty("positionSeconds").GetDouble());
    }

    [Fact]
    public async Task AC3_invalid_progress_is_a_validation_problem()
    {
        await using var api = await TestApi.StartAsync();

        var res = await api.PutProgressAsync("p-adaeze", "the-salt-road", -5, "phone", api.Clock.GetUtcNow());

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Contains("positionSeconds", await res.Content.ReadAsStringAsync());
    }

    // AC4: Given an expired subscription, then play returns 402 with a renew link.
    [Fact]
    public async Task AC4_expired_subscription_gets_402_with_a_renew_link()
    {
        await using var api = await TestApi.StartAsync();

        var res = await api.Http.PostAsync(new Uri("/api/profiles/p-wanjiru/titles/nightbus-to-bulawayo/play", UriKind.Relative), null);

        Assert.Equal(HttpStatusCode.PaymentRequired, res.StatusCode);
        Assert.Equal("application/problem+json", res.Content.Headers.ContentType?.MediaType);
        var problem = JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement;
        Assert.StartsWith("/account/renew", problem.GetProperty("renewUrl").GetString());
        Assert.True(res.Headers.TryGetValues("Link", out var links));
        Assert.Contains("rel=\"payment\"", string.Join(",", links!));
    }

    [Fact]
    public async Task AC4_subscription_lapsing_turns_play_into_402()
    {
        await using var api = await TestApi.StartAsync();
        var ok = await api.Http.PostAsync(new Uri("/api/profiles/p-adaeze/titles/the-salt-road/play", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);

        api.Clock.Advance(TimeSpan.FromDays(45)); // past the seeded paid-until date

        var lapsed = await api.Http.PostAsync(new Uri("/api/profiles/p-adaeze/titles/the-salt-road/play", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.PaymentRequired, lapsed.StatusCode);
    }

    [Fact]
    public async Task Unknown_profile_is_a_404_problem()
    {
        await using var api = await TestApi.StartAsync();

        var res = await api.Http.GetAsync(new Uri("/api/profiles/p-nobody/browse", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.Equal("application/problem+json", res.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Health_endpoints_report_live_and_ready()
    {
        await using var api = await TestApi.StartAsync();

        Assert.Equal(HttpStatusCode.OK, (await api.Http.GetAsync(new Uri("/health/live", UriKind.Relative))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await api.Http.GetAsync(new Uri("/health/ready", UriKind.Relative))).StatusCode);
    }
}
