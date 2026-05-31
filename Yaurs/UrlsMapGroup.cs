using Microsoft.AspNetCore.Http.HttpResults;
using Yaurs.Stats;

namespace Yaurs;

static class UrlsMapGroup
{
    private static ILogger? logger;

    public static void Register(WebApplication app)
    {
        logger = app.Logger;

        var urls = app.MapGroup("/urls");

        urls.MapPost("/", HandleCreate);
        urls.MapGet("/{id}/stats", HandleGetStats);
    }

    internal static async Task<Results<Created<CreatedUrlDto>, ValidationProblem>> HandleCreate(IUrlService urlService, CreateUrlDto createUrlDto)
    {
        if (!IsRedirectableUri(createUrlDto.TargetUri))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(CreateUrlDto.TargetUri)] = ["Must be an absolute HTTP or HTTPS URL."],
            });
        }

        var createdUrl = await urlService.CreateUrlAsync(createUrlDto.TargetUri);

        return TypedResults.Created($"/urls/{createdUrl.Id}", createdUrl);
    }

    internal static async Task<Results<Ok<IList<Stat>>, NotFound, UnauthorizedHttpResult>> HandleGetStats(IUrlService urlService, IStatsAuthenticator statsAuthenticator, HttpContext context, Ulid id)
    {
        var url = await urlService.GetUrlAsync(id);

        if (url is null)
        {
            if (logger is not null && logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("URL with ID {Id} not found", id);
            }

            return TypedResults.NotFound();
        }

        var statsKey = context.Request.Headers.Authorization.First()?.Replace("Bearer ", "");

        try
        {
            statsAuthenticator.Authenticate(url, statsKey);
        }
        catch (StatsKeyHashException e)
        {
            // TODO: make this code a bit more dry
            if (logger is not null && logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Unable to compute stats key hash: {message}", e.Message);
            }

            return TypedResults.Unauthorized();
        }
        catch (StatsKeyAuthenticationException e)
        {
            if (logger is not null && logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("User-provided stats key missing or invalid: {message}", e.Message);
            }

            return TypedResults.Unauthorized();
        }

        // Note that we surface hit statistics as a list so that:
        //
        // 1. we can easily introduce new stat types in the future
        // 2. we can return a list of stats from our /urls/{id}/stats
        //    subresource, better following RESTful conventions.
        return TypedResults.Ok<IList<Stat>>([
            new Stat {
                Type = StatType.Lifetime,
                Hits = url.LifetimeHits,
            }
        ]);
    }

    private static bool IsRedirectableUri(Uri targetUri) => targetUri.IsAbsoluteUri &&
            (targetUri.Scheme == Uri.UriSchemeHttp || targetUri.Scheme == Uri.UriSchemeHttps);
}
