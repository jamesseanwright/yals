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
        if (!createUrlDto.TargetUri.IsAbsoluteUri ||
            (createUrlDto.TargetUri.Scheme != Uri.UriSchemeHttp && createUrlDto.TargetUri.Scheme != Uri.UriSchemeHttps))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(CreateUrlDto.TargetUri)] = ["Must be an absolute HTTP or HTTPS URL."],
            });
        }

        var createdUrl = await urlService.CreateUrlAsync(createUrlDto.TargetUri);

        return TypedResults.Created($"http://TODO", createdUrl);
    }
}
