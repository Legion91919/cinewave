using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;

namespace Cinewave.Api.Cdn;

/// <summary>
/// Where HLS files live. In production <see cref="BaseUrl"/> is a CDN host in front of blob storage and the API serves no media at all.
/// Locally the same files are served as plain static files under /cdn, so no API endpoint ever streams video.
/// </summary>
internal sealed record CdnOriginOptions(string BaseUrl, bool ServeLocalOrigin, string LocalPath)
{
    public static CdnOriginOptions From(IConfiguration config) => new(
        config["Cdn:BaseUrl"] ?? "/cdn",
        config.GetValue("Cdn:ServeLocalOrigin", true),
        config["Cdn:LocalPath"] ?? Path.Combine(AppContext.BaseDirectory, "cdn-origin"));
}

internal static class CdnOriginExtensions
{
    public static IApplicationBuilder UseCdnOrigin(this IApplicationBuilder app, CdnOriginOptions cdn)
    {
        var types = new FileExtensionContentTypeProvider();
        types.Mappings[".m3u8"] = "application/vnd.apple.mpegurl";
        types.Mappings[".ts"] = "video/mp2t";
        types.Mappings[".m4s"] = "video/iso.segment";

        Directory.CreateDirectory(cdn.LocalPath);
        return app.UseStaticFiles(new StaticFileOptions
        {
            RequestPath = "/cdn",
            FileProvider = new PhysicalFileProvider(cdn.LocalPath),
            ContentTypeProvider = types,
            ServeUnknownFileTypes = false,
            OnPrepareResponse = ctx =>
            {
                var headers = ctx.Context.Response.Headers;
                headers.AccessControlAllowOrigin = "*"; // a real CDN host is a different origin from the web app
                headers.CacheControl = ctx.File.Name.EndsWith(".m3u8", StringComparison.Ordinal)
                    ? "public, max-age=60"
                    : "public, max-age=31536000, immutable";
            },
        });
    }
}

internal sealed class CdnOriginHealthCheck(CdnOriginOptions cdn) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!cdn.ServeLocalOrigin)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Media served by external CDN."));
        }

        var demo = Path.Combine(cdn.LocalPath, "demo", "testpattern", "master.m3u8");
        return Task.FromResult(File.Exists(demo)
            ? HealthCheckResult.Healthy("Local CDN origin has the demo stream.")
            : HealthCheckResult.Unhealthy("Local CDN origin is missing demo/testpattern/master.m3u8."));
    }
}
