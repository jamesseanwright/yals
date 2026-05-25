namespace Yaurs.Tests;

using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;

public class PostgresContainerFixture : IAsyncLifetime
{
    private readonly IContainer postgres;

    public PostgresContainerFixture()
    {
        postgres = new ContainerBuilder("postgres:18.4")
            .WithPortBinding(5432, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(5432))
            .WithEnvironment("POSTGRES_HOST_AUTH_METHOD", "trust")
            .Build();
    }

    public ushort Port { get => postgres.GetMappedPublicPort(); }

    public async Task InitializeAsync()
    {
        await postgres.StartAsync().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await postgres.StopAsync();
    }
}

public class YaursApiIntegrationTests(PostgresContainerFixture postgres) : IClassFixture<PostgresContainerFixture>
{
    [Fact]
    public async Task TestPostUrlsCreatesNewShortenedLink()
    {
        await using var application = new YaursWebApplicationFactory(postgres.Port);

        var client = application.CreateClient();

        var res = await client.PostAsync("/urls", JsonContent.Create(new CreateUrlDto
        {
            TargetUrl = new Uri("https://foo"),
        }));

        res.EnsureSuccessStatusCode();

        var resBody = await res.Content.ReadFromJsonAsync<Url>();

        Assert.NotNull(resBody);
        Assert.IsType<Ulid>(resBody.Id);
        Assert.Equal(new Uri("https://foo"), resBody.TargetUri);
    }
}
