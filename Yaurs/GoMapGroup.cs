namespace Yaurs;

static class GoMapGroup
{
    public static void Register(WebApplication app)
    {
        var urls = app.MapGroup("/go");

        urls.MapGet("/{id}", async (IUrlService urlService, Ulid id) =>
        {
            throw new NotImplementedException();
        });
    }
}
