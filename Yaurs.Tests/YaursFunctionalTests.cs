namespace Yaurs.Tests;

using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;
using System.Net.Http.Json;
using System.Net;
using System.Net.Http.Headers;
using Yaurs.Stats;

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
    public async Task TestUrlCreationAndUtilisation()
    {
        var goRequestCount = 3;

        var client = fixture.CreateClient();

        var createUrlRes = await client.PostAsJsonAsync("/urls", new CreateUrlDto
        {
            TargetUri = new Uri("https://foo"),
        });

        Assert.Equal(HttpStatusCode.Created, createUrlRes.StatusCode);

        var createUrlResBody = await createUrlRes.Content.ReadFromJsonAsync<CreatedUrlDto>();

        if (createUrlResBody is null)
        {
            Assert.Fail("URL creation response has no body");
        }

        for (var i = 0; i < goRequestCount; i++)
        {
            var goRes = await client.GetAsync($"/go/{createUrlResBody.Id}");

            Assert.Equal(HttpStatusCode.PermanentRedirect, goRes.StatusCode);
            Assert.Equal(createUrlResBody.TargetUri, goRes.Headers.Location);
        }

        var getUrlStatsReq = new HttpRequestMessage
        {
            RequestUri = new Uri($"/urls/{createUrlResBody.Id}/stats"),
            Method = HttpMethod.Get,
        };

        getUrlStatsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", createUrlResBody.StatsKey);

        var getUrlStatsRes = await client.SendAsync(getUrlStatsReq);

        Assert.Equal(HttpStatusCode.OK, getUrlStatsRes.StatusCode);

        var getUrlStatsResBody = await getUrlStatsRes.Content.ReadFromJsonAsync<IList<Stat>>();

        if (getUrlStatsResBody is null)
        {
            Assert.Fail("URL stats response has no body");
        }

        var stat = Assert.Single(getUrlStatsResBody, x => x.Type == StatType.Lifetime);
        Assert.Equal(goRequestCount, stat.Hits);
    }

    [Fact]
    public async Task TestGetUrlStatsReturns401WhenInvalidAuthTokenProvided()
    {
        var client = fixture.CreateClient();

        var createUrlRes = await client.PostAsJsonAsync("/urls", new CreateUrlDto
        {
            TargetUri = new Uri("https://foo"),
        });

        Assert.Equal(HttpStatusCode.Created, createUrlRes.StatusCode);

        var createUrlResBody = await createUrlRes.Content.ReadFromJsonAsync<CreatedUrlDto>();

        if (createUrlResBody is null)
        {
            Assert.Fail("URL creation response has no body");
        }

        var getUrlStatsReq = new HttpRequestMessage
        {
            RequestUri = new Uri($"/urls/{createUrlResBody.Id}/stats"),
            Method = HttpMethod.Get,
        };

        getUrlStatsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "FF");

        var getUrlStatsRes = await client.SendAsync(getUrlStatsReq);

        Assert.Equal(HttpStatusCode.Unauthorized, getUrlStatsRes.StatusCode);
    }

    [Fact]
    public async Task TestPostUrlsCreatesNewShortenedLink()
    {
        var client = fixture.CreateClient();

        // TODO: refactor to use client.PostAsJsonAsync<T>
        var res = await client.PostAsync("/urls", JsonContent.Create(new CreateUrlDto
        {
            TargetUri = new Uri("https://foo"),
        }));

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var resBody = await res.Content.ReadFromJsonAsync<CreatedUrlDto>();

        Assert.NotNull(resBody);
        Assert.IsType<Ulid>(resBody.Id);
        Assert.Equal(new Uri("https://foo"), resBody.TargetUri);
        Assert.Equal(64, resBody.StatsKey.Length);
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

        var createUrlResBody = await createUrlRes.Content.ReadFromJsonAsync<CreatedUrlDto>();

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
