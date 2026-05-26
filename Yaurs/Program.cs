using Microsoft.EntityFrameworkCore;
using Yaurs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UrlDb>(opt => opt.UseNpgsql(
    builder.Configuration.GetValue<string>("ConnectionStrings:Database")
));

builder.Services.AddScoped<IUrlService, UrlService>();

var app = builder.Build();


// TODO: this should probably be moved (Routes.cs?)
var urls = app.MapGroup("/urls");

urls.MapPost("/", async (IUrlService urlService, CreateUrlDto createUrlDto) =>
{
    var createdUrl = await urlService.CreateUrlAsync(createUrlDto.TargetUri);

    return TypedResults.Created($"http://TODO", createdUrl);
});

app.Run();
