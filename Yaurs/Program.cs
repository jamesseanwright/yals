using Microsoft.EntityFrameworkCore;
using Yaurs;
using Yaurs.Crypto;
using Yaurs.Stats;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UrlDb>(opt => opt.UseNpgsql(
    builder.Configuration.GetValue<string>("ConnectionStrings:Database")
));

builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddSingleton<IStatsKeyGenerator, StatsKeyGenerator>();
builder.Services.AddSingleton<IRandomNumberGenerator, CryptoRandomNumberGenerator>();
builder.Services.AddSingleton<IStatsAuthenticator, StatsAuthenticator>();

var app = builder.Build();

UrlsMapGroup.Register(app);
GoMapGroup.Register(app);

app.Run();
