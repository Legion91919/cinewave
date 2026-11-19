using System.Text.Json.Serialization;
using Cinewave.Api.Cdn;
using Cinewave.Api.Endpoints;
using Cinewave.Catalogue;
using Cinewave.Playback;
using Cinewave.Profiles;
using Cinewave.Subscriptions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Cinewave.Api;

/// <summary>Composition root. Tests call <see cref="Build"/> to run the real pipeline on a random port.</summary>
public static class CinewaveApp
{
    public static WebApplication Build(string[] args, Action<WebApplicationBuilder>? configure = null)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.Configuration;
        var seed = config.GetValue("Seed:DemoData", builder.Environment.IsDevelopment());
        var cdn = CdnOriginOptions.From(config);

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton(cdn);
        builder.Services.AddProblemDetails();
        builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        builder.Services.AddHealthChecks().AddCheck<CdnOriginHealthCheck>("cdn-origin", tags: ["ready"]);

        builder.Services.AddCatalogueModule(seed);
        builder.Services.AddPlaybackModule(cdn.BaseUrl, seed);
        builder.Services.AddSubscriptionsModule(config["Subscriptions:RenewBaseUrl"] ?? "/account/renew", seed);
        builder.Services.AddProfilesModule(seed);

        configure?.Invoke(builder);

        var app = builder.Build();
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        if (cdn.ServeLocalOrigin)
        {
            app.UseCdnOrigin(cdn);
        }

        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });

        var api = app.MapGroup("/api");
        api.MapProfileEndpoints();
        api.MapCatalogueEndpoints();
        api.MapPlaybackEndpoints();
        return app;
    }
}
