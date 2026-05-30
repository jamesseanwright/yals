namespace Yaurs.Tests;

using Microsoft.AspNetCore.Http.HttpResults;

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
}

class FakeUrlService : IUrlService
{
    public Uri? LastCreatedUri { get; private set; }

    public Task<GetUrlDto?> GetUrlAsync(Ulid id) => Task.FromResult<GetUrlDto?>(null);

    public Task<CreatedUrlDto> CreateUrlAsync(Uri targetUri)
    {
        LastCreatedUri = targetUri;
        return Task.FromResult(new CreatedUrlDto { Id = Ulid.NewUlid(), TargetUri = targetUri, StatsKey = "Stats key" });
    }
}
