namespace Yaurs;

static class UrlsMapGroup
{
    public static void Register(WebApplication app)
    {
        var urls = app.MapGroup("/urls");

        urls.MapPost("/", async (IUrlService urlService, CreateUrlDto createUrlDto) =>
        {
            var createdUrl = await urlService.CreateUrlAsync(createUrlDto.TargetUri);

            return TypedResults.Created($"http://TODO", createdUrl);
        });
    }
}
