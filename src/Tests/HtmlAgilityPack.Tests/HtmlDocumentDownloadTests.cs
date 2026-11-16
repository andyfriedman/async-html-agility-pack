using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using Xunit;

namespace HtmlAgilityPack.Tests;

public class HtmlDocumentDownloadTests
{
    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        public List<HttpRequestMessage> SentRequests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SentRequests.Add(request);
            return handler(request, cancellationToken);
        }
    }

    private static HttpClient CreateStubClient(
        string content,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        Encoding? encoding = null,
        string mediaType = "text/html")
    {
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, encoding ?? Encoding.UTF8, mediaType)
            };
            return Task.FromResult(response);
        });

        return new HttpClient(handler);
    }

    [Fact]
    public async Task LoadHtmlAsync_ValidHttpsUri_DownloadsAndParsesHtmlAsync()
    {
        const string html = """
            <!DOCTYPE html>
            <html>
            <head><title>Stubbed Page</title></head>
            <body>
                <h1 id="headline">Modern .NET 11</h1>
                <ul>
                    <li class="item">First</li>
                    <li class="item">Second</li>
                </ul>
            </body>
            </html>
            """;

        var handler = new StubHttpMessageHandler((req, _) =>
        {
            Assert.Equal(HttpMethod.Get, req.Method);
            Assert.Equal(new Uri("https://example.test/page"), req.RequestUri);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            });
        });

        using var httpClient = new HttpClient(handler);
        var doc = new HtmlDocument();

        await doc.LoadHtmlAsync(new Uri("https://example.test/page"), httpClient);

        Assert.Single(handler.SentRequests);
        Assert.Equal("Stubbed Page", doc.DocumentNode.SelectSingleNode("//title")?.InnerText);
        Assert.Equal("Modern .NET 11", doc.GetElementbyId("headline")?.InnerText);

        var items = doc.DocumentNode.SelectNodes("//li[@class='item']");
        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
        Assert.Equal(["First", "Second"], items.Select(n => n.InnerText).ToArray());
    }

    [Fact]
    public async Task LoadHtmlAsync_HttpSchemeAndContentTypeCharset_DecodesCorrectlyAsync()
    {
        const string html = "<html><body><span id='msg'>Grüß Gott</span></body></html>";
        using var httpClient = CreateStubClient(html, HttpStatusCode.OK, Encoding.Latin1);

        var doc = new HtmlDocument();
        await doc.LoadHtmlAsync(new Uri("http://example.test/charset"), httpClient);

        Assert.Equal("Grüß Gott", doc.GetElementbyId("msg")?.InnerText);
    }

    [Fact]
    public async Task LoadHtmlAsync_WithExplicitEncoding_DecodesBytesCorrectlyAsync()
    {
        const string html = "<html><body><p id='greeting'>Caf\u00e9 &amp; Na\u00efve</p></body></html>";
        byte[] latin1Bytes = Encoding.Latin1.GetBytes(html);

        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var byteContent = new ByteArrayContent(latin1Bytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = byteContent
            });
        });

        using var httpClient = new HttpClient(handler);
        var doc = new HtmlDocument();

        await doc.LoadHtmlAsync(new Uri("http://example.test/latin1"), httpClient, Encoding.Latin1);

        var paragraph = doc.GetElementbyId("greeting");
        Assert.NotNull(paragraph);
        Assert.Equal("Café & Naïve", HtmlEntity.DeEntitize(paragraph.InnerText));
        Assert.Equal(Encoding.Latin1.WebName, doc.StreamEncoding?.WebName);
    }

    [Fact]
    public async Task LoadHtmlAsync_RespectsDocumentOptionsAsync()
    {
        const string html = "<div single='1' double=\"2\" bare><span>text";
        using var httpClient = CreateStubClient(html);

        var doc = new HtmlDocument
        {
            GlobalAttributeValueQuote = AttributeValueQuote.Initial,
            OptionCheckSyntax = true
        };

        await doc.LoadHtmlAsync(new Uri("https://example.test/options"), httpClient);

        Assert.NotEmpty(doc.ParseErrors);
        Assert.Contains(doc.ParseErrors, e => e.Code == HtmlParseErrorCode.TagNotClosed);

        var div = doc.DocumentNode.SelectSingleNode("//div");
        Assert.NotNull(div);
        Assert.Equal(AttributeValueQuote.SingleQuote, div.Attributes["single"]?.QuoteType);
        Assert.Equal(AttributeValueQuote.DoubleQuote, div.Attributes["double"]?.QuoteType);
        Assert.Equal(AttributeValueQuote.WithoutValue, div.Attributes["bare"]?.QuoteType);
    }

    [Fact]
    public async Task LoadHtmlAsync_ReloadingIntoSameDocument_ReplacesPreviousContentAsync()
    {
        int callCount = 0;
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            callCount++;
            string body = callCount == 1
                ? "<html><body><div id='first'>One</div></body></html>"
                : "<html><body><div id='second'>Two</div></body></html>";

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "text/html")
            });
        });

        using var httpClient = new HttpClient(handler);
        var doc = new HtmlDocument();

        await doc.LoadHtmlAsync(new Uri("https://example.test/first"), httpClient);
        Assert.Equal("One", doc.GetElementbyId("first")?.InnerText);

        await doc.LoadHtmlAsync(new Uri("https://example.test/second"), httpClient);
        Assert.Null(doc.GetElementbyId("first"));
        Assert.Equal("Two", doc.GetElementbyId("second")?.InnerText);
        Assert.Equal(2, handler.SentRequests.Count);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task LoadHtmlAsync_NonSuccessStatusCode_ThrowsHttpRequestExceptionAsync(HttpStatusCode statusCode)
    {
        using var httpClient = CreateStubClient("<html>Error</html>", statusCode);
        var doc = new HtmlDocument();

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
            doc.LoadHtmlAsync(new Uri("https://example.test/fail"), httpClient));

        Assert.Equal(statusCode, ex.StatusCode);
    }

    [Fact]
    public async Task LoadHtmlAsync_CancelledToken_ThrowsOperationCanceledExceptionAsync()
    {
        var handler = new StubHttpMessageHandler(async (_, ct) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        using var httpClient = new HttpClient(handler);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var doc = new HtmlDocument();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            doc.LoadHtmlAsync(new Uri("https://example.test/cancel"), httpClient, cts.Token));
    }

    [Fact]
    public async Task LoadHtmlAsync_NullUri_ThrowsArgumentNullExceptionAsync()
    {
        var doc = new HtmlDocument();
        await Assert.ThrowsAsync<ArgumentNullException>(() => doc.LoadHtmlAsync(null!));
    }

    [Theory]
    [InlineData("/relative/path")]
    [InlineData("ftp://example.test/file.html")]
    [InlineData("file:///tmp/test.html")]
    public async Task LoadHtmlAsync_UnsupportedOrRelativeUri_ThrowsArgumentExceptionAsync(string uriString)
    {
        using var httpClient = CreateStubClient("<html></html>");
        var doc = new HtmlDocument();
        var uri = new Uri(uriString, UriKind.RelativeOrAbsolute);

        await Assert.ThrowsAsync<ArgumentException>(() => doc.LoadHtmlAsync(uri, httpClient));
    }

    [Fact]
    public async Task DetectEncodingAndLoadAsync_And_SaveAsync_XmlWriter_RoundTripAsync()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), $"hap_async_{Guid.NewGuid():N}.html");
        try
        {
            await File.WriteAllTextAsync(
                tempPath,
                "<html><head><meta charset=\"utf-8\" /></head><body><p id=\"p1\">Hello Async</p></body></html>",
                Encoding.UTF8);

            var doc = new HtmlDocument();
            await doc.DetectEncodingAndLoadAsync(tempPath);

            Assert.Equal("Hello Async", doc.GetElementbyId("p1")?.InnerText);

            var sb = new StringBuilder();
            await using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings { Async = true }))
            {
                await doc.SaveAsync(xmlWriter);
            }

            Assert.Contains("Hello Async", sb.ToString());
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public async Task MixedCodeDocument_And_HtmlNodeNavigator_AsyncOperations_WorkCorrectlyAsync()
    {
        const string mixed = "<div>Hello <% = userName %>!</div>";
        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        using var ms = new MemoryStream(utf8NoBom.GetBytes(mixed));

        var mixedDoc = new MixedCodeDocument();
        await mixedDoc.LoadAsync(ms, utf8NoBom);

        Assert.Single(mixedDoc.CodeFragments);
        Assert.Equal(2, mixedDoc.TextFragments.Count);

        using var outMs = new MemoryStream();
        await mixedDoc.SaveAsync(outMs, utf8NoBom);
        Assert.Equal(mixed, utf8NoBom.GetString(outMs.ToArray()));

        using var navStream = new MemoryStream("<html><body><span>NavText</span></body></html>"u8.ToArray());
        var navigator = await HtmlNodeNavigator.CreateAsync(navStream, Encoding.UTF8);
        var selected = navigator.SelectSingleNode("//span");
        Assert.NotNull(selected);
        Assert.Equal("NavText", selected.Value);
    }
}
