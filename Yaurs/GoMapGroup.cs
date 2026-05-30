using Microsoft.AspNetCore.Http.HttpResults;

namespace Yaurs;

static class GoMapGroup
{
    public static void Register(WebApplication app)
    {
        var urls = app.MapGroup("/go");

        urls.MapGet("/{id}", async Task<Results<RedirectHttpResult, NotFound>> (IUrlService urlService, Ulid id) =>
        {
            var url = await urlService.GetUrlAsync(id);

            if (url is not null)
            {
                await urlService.RegisterHit(url.Id);
                return TypedResults.Redirect(url.TargetUri.ToString(), permanent: true, preserveMethod: true);
            }


            if (app.Logger.IsEnabled(LogLevel.Information))
            {
                app.Logger.LogInformation("URL with ID {Id} not found", id);
            }

            return TypedResults.NotFound();
        });
    }
}
