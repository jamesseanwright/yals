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

        return TypedResults.Created($"http://TODO", createdUrl);
    }

    internal static async Task<Results<Ok<IList<Stat>>, NotFound>> HandleGetStats(IUrlService urlService, Ulid id)
    {
        var url = await urlService.GetUrlAsync(id);

        if (url is not null)
        {
            return TypedResults.Ok(url.Stats);
        }

        if (logger is not null && logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("URL with ID {Id} not found", id);
        }

        return TypedResults.NotFound();
    }

    private static bool IsRedirectableUri(Uri targetUri) => targetUri.IsAbsoluteUri &&
            (targetUri.Scheme == Uri.UriSchemeHttp || targetUri.Scheme == Uri.UriSchemeHttps);
}
