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
        if (application is null)
        {
            throw new Exception("Failed to create YaursWebApplicationFactory");
        }

        return application.CreateClient(new()
        {
            AllowAutoRedirect = false,
        });
    }

    public async Task InitializeAsync()
    {
        await postgres.StartAsync().ConfigureAwait(false);
        application = new YaursWebApplicationFactory(postgres.GetMappedPublicPort());
    }

    public async Task DisposeAsync()
    {
        await postgres.StopAsync();

        if (application is not null)
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

        var resBody = await res.Content.ReadFromJsonAsync<CreatedUrlDto>();

        Assert.NotNull(resBody);
        Assert.IsType<Ulid>(resBody.Id);
        Assert.Equal(new Uri("https://foo"), resBody.TargetUri);
        Assert.True(resBody.StatsKey.Length > 64, "resBody.StatsKey is an unexpected length");
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

    [Fact]
    public async Task TestGoReturns404WhenUrlNotFound()
    {
        var client = fixture.CreateClient();

        var res = await client.GetAsync($"/go/{Ulid.NewUlid()}");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }
}
