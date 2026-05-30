using Microsoft.EntityFrameworkCore;
using Yaurs;
using Yaurs.Crypto;
using Yaurs.Stats;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UrlDb>(opt => opt.UseNpgsql(
    builder.Configuration.GetValue<string>("ConnectionStrings:Database")
));

builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddScoped<IStatsKeyGenerator, StatsKeyGenerator>(); // TODO: can this dependency be a singleton?
builder.Services.AddScoped<IRandomNumberGenerator, CryptoRandomNumberGenerator>(); // TODO: can this dependency be a singleton?

var app = builder.Build();

UrlsMapGroup.Register(app);
GoMapGroup.Register(app);

app.Run();
