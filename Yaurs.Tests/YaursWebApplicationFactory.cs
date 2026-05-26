namespace Yaurs.Tests;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class YaursWebApplicationFactory(ushort dbPort) : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder) ?? throw new TestSetupException("Host could not be created");

        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<UrlDb>();
            db.Database.Migrate();
        }

        return host;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("ConnectionStrings:Database", $"Host=localhost;Port={dbPort};Username=postgres"),
            ]);
        });

        base.ConfigureWebHost(builder);
    }
}