namespace Yaurs.Tests.Urls;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Yaurs.Stats;
using Yaurs.Urls;

public class UrlsMapGroupTests
{
    private readonly FakeUrlService urlService = new();

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com")]
    [InlineData("https://example.com/path?q=1")]
    public async Task HandleCreate_AcceptsHttpAndHttps(string uri)
    {
        var dto = new CreateUrlDto { TargetUri = new Uri(uri) };

        var result = await UrlsMapGroup.HandleCreate(urlService, dto);

        Assert.IsType<Created<CreatedUrlDto>>(result.Result);
        Assert.Equal(new Uri(uri), urlService.LastCreatedUri);
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("file:///etc/passwd")]
    [InlineData("mailto:user@example.com")]
    public async Task HandleCreate_RejectsNonHttpSchemes(string uri)
    {
        var dto = new CreateUrlDto { TargetUri = new Uri(uri) };

        var result = await UrlsMapGroup.HandleCreate(urlService, dto);

        var problem = Assert.IsType<ValidationProblem>(result.Result);
        Assert.Contains(nameof(CreateUrlDto.TargetUri), problem.ProblemDetails.Errors.Keys);
        Assert.Null(urlService.LastCreatedUri);
    }

    [Fact]
    public async Task HandleCreate_RejectsRelativeUri()
    {
        var dto = new CreateUrlDto { TargetUri = new Uri("/relative/path", UriKind.Relative) };

        var result = await UrlsMapGroup.HandleCreate(urlService, dto);

        var problem = Assert.IsType<ValidationProblem>(result.Result);
        Assert.Contains(nameof(CreateUrlDto.TargetUri), problem.ProblemDetails.Errors.Keys);
        Assert.Null(urlService.LastCreatedUri);
    }

    [Fact]
    public async Task HandleGetStats_ReturnsNotFound_WhenUrlDoesNotExist()
    {
        var result = await UrlsMapGroup.HandleGetStats(urlService, new StatsAuthenticator(), new DefaultHttpContext(), Ulid.NewUlid());

        Assert.IsType<NotFound>(result.Result);
    }
}

class FakeUrlService : IUrlService
{
    public Uri? LastCreatedUri { get; private set; }

    public Task<Url?> GetUrlAsync(Ulid id) => Task.FromResult<Url?>(null);

    public Task<CreatedUrlDto> CreateUrlAsync(Uri targetUri)
    {
        LastCreatedUri = targetUri;
        return Task.FromResult(new CreatedUrlDto { Id = Ulid.NewUlid(), TargetUri = targetUri, StatsKey = "Stats key" });
    }

    public Task RegisterHit(Ulid Id)
    {
        return Task.CompletedTask;
    }
}
