namespace Yaurs.Tests;

using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;
using System.Net.Http.Json;
using System.Net;

public class FunctionalTestFixture : IAsyncLifetime
{
    private readonly IContainer postgres;
    private YaursWebApplicationFactory? application;

    public FunctionalTestFixture()
    {
        postgres = new ContainerBuilder("postgres:18.4")
            .WithPortBinding(5432, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
            .WithEnvironment("POSTGRES_HOST_AUTH_METHOD", "trust")
            .Build();
    }

    public HttpClient CreateClient()
    {
        if (application == null)
        {
            throw new Exception("Failed to create YaursWebApplicationFactory");
        }

        return application.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await postgres.StartAsync().ConfigureAwait(false);
        application = new YaursWebApplicationFactory(postgres.GetMappedPublicPort());
    }

    public async Task DisposeAsync()
    {
        await postgres.StopAsync();

        if (application != null)
        {
            await application.DisposeAsync();
        }
    }
}

public class YaursFunctionalTests(FunctionalTestFixture fixture) : IClassFixture<FunctionalTestFixture>
{
    [Fact]
    public async Task TestPostUrlsCreatesNewShortenedLink()
    {
        var client = fixture.CreateClient();

        var res = await client.PostAsync("/urls", JsonContent.Create(new CreateUrlDto
        {
            TargetUri = new Uri("https://foo"),
        }));

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var resBody = await res.Content.ReadFromJsonAsync<Url>();

        Assert.NotNull(resBody);
        Assert.IsType<Ulid>(resBody.Id);
        Assert.Equal(new Uri("https://foo"), resBody.TargetUri);
    }

    [Fact]
    public async Task TestGoProducesRedirect()
    {
        var client = fixture.CreateClient();

        var createUrlRes = await client.PostAsync("/urls", JsonContent.Create(new CreateUrlDto
        {
            TargetUri = new Uri("https://foo"),
        }));

        Assert.Equal(HttpStatusCode.Created, createUrlRes.StatusCode);

        var createUrlResBody = await createUrlRes.Content.ReadFromJsonAsync<Url>();

        Assert.NotNull(createUrlResBody);

        var goRes = await client.GetAsync($"/go/{createUrlResBody.Id}");

        Assert.Equal(HttpStatusCode.PermanentRedirect, goRes.StatusCode);
        Assert.Equal(createUrlResBody.TargetUri, goRes.Headers.Location);
    }
}
