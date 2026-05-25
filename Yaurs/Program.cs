using Microsoft.EntityFrameworkCore;
using Yaurs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UrlDb>(opt => opt.UseNpgsql(
    builder.Configuration.GetValue<string>("ConnectionStrings:Database")
));

builder.Services.AddSingleton<IUrlService, UrlService>();

var app = builder.Build();

var urls = app.MapGroup("/urls");

urls.MapPost("/", ctx => throw new NotImplementedException());

app.Run();
