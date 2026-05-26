namespace Yaurs;

static class GoMapGroup
{
    public static void Register(WebApplication app)
    {
        var urls = app.MapGroup("/go");

        urls.MapGet("/{id}", async (IUrlService urlService, Ulid id) =>
        {
            var url = await urlService.GetUrlAsync(id);

            // TODO: handle missing URL as 404

            return TypedResults.Redirect(url.TargetUri.ToString(), permanent: true, preserveMethod: true);
        });
    }
}
