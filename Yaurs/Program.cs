using Microsoft.EntityFrameworkCore;
using Yaurs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UrlDb>(opt => opt.UseNpgsql(
    builder.Configuration.GetValue<string>("ConnectionStrings:Database")
));

builder.Services.AddScoped<IUrlService, UrlService>();

var app = builder.Build();

UrlsMapGroup.Register(app);

app.Run();
