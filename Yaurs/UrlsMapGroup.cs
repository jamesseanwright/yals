using Microsoft.AspNetCore.Http.HttpResults;

namespace Yaurs;

static class UrlsMapGroup
{
    public static void Register(WebApplication app)
    {
        var urls = app.MapGroup("/urls");

        urls.MapPost("/", HandleCreate);
    }

    internal static async Task<Results<Created<Url>, ValidationProblem>> HandleCreate(IUrlService urlService, CreateUrlDto createUrlDto)
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

    private static bool IsRedirectableUri(Uri targetUri) => targetUri.IsAbsoluteUri &&
            (targetUri.Scheme == Uri.UriSchemeHttp || targetUri.Scheme == Uri.UriSchemeHttps);
}
