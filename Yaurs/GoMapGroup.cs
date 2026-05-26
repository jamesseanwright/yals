using Microsoft.AspNetCore.Http.HttpResults;

namespace Yaurs;

static class GoMapGroup
{
    public static void Register(WebApplication app)
    {
        var urls = app.MapGroup("/go");

        urls.MapGet("/{id}", async Task<Results<RedirectHttpResult, NotFound>> (IUrlService urlService, Ulid id) =>
            await urlService.GetUrlAsync(id)
                is Url url
                    ? TypedResults.Redirect(url.TargetUri.ToString(), permanent: true, preserveMethod: true)
                    : TypedResults.NotFound()
        );
    }
}
