using System.Net;
using System.Net.Sockets;
using System.Text;
using Xunit;

namespace HtmlAgilityPack.Tests;

/// <summary>
/// One or more tests for each public <see cref="HtmlDocument"/> method that is not exercised elsewhere.
/// </summary>
public sealed class HtmlDocumentApiTests : IDisposable
{
    private const string Html = "<html><head><title>T</title></head><body><p id='p'>café</p></body></html>";
    private readonly string _tempDir = Directory.CreateTempSubdirectory("hap-tests-").FullName;

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    private string WriteTempFile(string content, Encoding encoding)
    {
        var path = Path.Combine(_tempDir, Guid.NewGuid() + ".html");
        File.WriteAllText(path, content, encoding);
        return path;
    }

    private static MemoryStream ToStream(string content, Encoding encoding) =>
        new(encoding.GetPreamble().Concat(encoding.GetBytes(content)).ToArray());

    private static void AssertLoaded(HtmlDocument doc) =>
        Assert.Equal("café", doc.GetElementbyId("p")!.InnerText);

    // ---------- names / comments ----------

    [Theory]
    [InlineData("div", "div")]
    [InlineData("my-tag_1.x", "my-tag_1.x")]
    [InlineData("a:b", "_a3a_b")]
    [InlineData("a b", "_a20_b")]
    public void GetXmlName_ReplacesInvalidCharacters(string name, string expected)
    {
        Assert.Equal(expected, HtmlDocument.GetXmlName(name));
    }

    [Fact]
    public void UseAttributeOriginalName_OutputsOriginalCaseForTag()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml("<div DATA-Foo='1'></div><span Title='t'></span>");

        doc.UseAttributeOriginalName("div");

        Assert.Equal("<div DATA-Foo='1'></div>", doc.DocumentNode.SelectSingleNode("//div")!.OuterHtml);
        Assert.Equal("<span title='t'></span>", doc.DocumentNode.SelectSingleNode("//span")!.OuterHtml);
    }

    [Fact]
    public void UseAttributeOriginalName_NoMatchingTag_DoesNothing()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml("<div A='1'></div>");

        doc.UseAttributeOriginalName("table");

        Assert.Equal("<div a='1'></div>", doc.DocumentNode.InnerHtml);
    }

    [Fact]
    public void CreateComment_CreatesEmptyCommentNode()
    {
        var comment = new HtmlDocument().CreateComment();

        Assert.Equal(HtmlNodeType.Comment, comment.NodeType);
        Assert.Equal("#comment", comment.Name);
    }

    [Theory]
    [InlineData("note", "<!--note-->")]
    [InlineData("<!--note-->", "<!--note-->")]
    public void CreateComment_WrapsTextInCommentDelimiters(string text, string expected)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml("<div></div>");
        var div = doc.DocumentNode.SelectSingleNode("//div")!;

        var comment = doc.CreateComment(text);
        div.AppendChild(comment);

        Assert.Equal(expected, comment.Comment);
        Assert.Equal($"<div>{expected}</div>", div.OuterHtml);
        Assert.Throws<ArgumentNullException>(() => doc.CreateComment(null!));
    }

    // ---------- encoding detection ----------

    [Fact]
    public void DetectEncodingHtml_ReadsMetaCharset()
    {
        var doc = new HtmlDocument();

        var encoding = doc.DetectEncodingHtml("<html><head><meta charset='iso-8859-1'></head></html>");

        Assert.Equal("iso-8859-1", encoding!.WebName);
        Assert.Null(doc.DetectEncodingHtml("<html><body>no meta</body></html>"));
        Assert.Throws<ArgumentNullException>(() => doc.DetectEncodingHtml(null!));
    }

    [Fact]
    public async Task DetectEncodingAsync_Stream_ReadsByteOrderMark()
    {
        using var stream = ToStream("<p>x</p>", new UnicodeEncoding(bigEndian: false, byteOrderMark: true));

        var encoding = await new HtmlDocument().DetectEncodingAsync(stream);

        Assert.Equal(Encoding.Unicode.WebName, encoding!.WebName);
    }

    [Fact]
    public async Task DetectEncodingAsync_StreamCheckHtml_ReadsMetaCharset()
    {
        using var stream = ToStream("<html><head><meta charset='iso-8859-1'></head></html>", Encoding.ASCII);

        var encoding = await new HtmlDocument().DetectEncodingAsync(stream, checkHtml: true);

        Assert.Equal("iso-8859-1", encoding!.WebName);
        await Assert.ThrowsAsync<ArgumentNullException>(() => new HtmlDocument().DetectEncodingAsync((Stream)null!, true));
    }

    // ---------- loading from streams ----------

    [Fact]
    public async Task LoadAsync_Stream_DetectsBom()
    {
        var doc = new HtmlDocument();
        using var stream = ToStream(Html, Encoding.Unicode);

        await doc.LoadAsync(stream);

        AssertLoaded(doc);
        Assert.Equal(Encoding.Unicode.WebName, doc.StreamEncoding!.WebName);
    }

    [Fact]
    public async Task LoadAsync_StreamDetectBom_HonoursFlag()
    {
        var doc = new HtmlDocument();
        using var stream = ToStream(Html, Encoding.UTF8);

        await doc.LoadAsync(stream, detectEncodingFromByteOrderMarks: true);

        AssertLoaded(doc);
        await Assert.ThrowsAsync<ArgumentNullException>(() => doc.LoadAsync((Stream)null!, true));
    }

    [Fact]
    public async Task LoadAsync_StreamEncodingDetectBom_UsesEncoding()
    {
        var doc = new HtmlDocument();
        var latin1 = Encoding.Latin1;
        using var stream = new MemoryStream(latin1.GetBytes(Html));

        await doc.LoadAsync(stream, latin1, detectEncodingFromByteOrderMarks: false);

        AssertLoaded(doc);
        await Assert.ThrowsAsync<ArgumentNullException>(() => doc.LoadAsync(stream, null!, false));
    }

    [Fact]
    public async Task LoadAsync_StreamEncodingDetectBomBufferSize_UsesEncoding()
    {
        var doc = new HtmlDocument();
        var latin1 = Encoding.Latin1;
        using var stream = new MemoryStream(latin1.GetBytes(Html));

        await doc.LoadAsync(stream, latin1, false, 16);

        AssertLoaded(doc);
        await Assert.ThrowsAsync<ArgumentNullException>(() => doc.LoadAsync((Stream)null!, latin1, false, 16));
    }

    [Fact]
    public async Task LoadAsync_Stream_LeavesStreamOpen()
    {
        using var stream = ToStream(Html, Encoding.UTF8);

        await new HtmlDocument().LoadAsync(stream);

        Assert.True(stream.CanRead);
    }

    // ---------- loading / saving files ----------

    [Fact]
    public async Task LoadAsync_PathDetectBom_ReadsFile()
    {
        var path = WriteTempFile(Html, Encoding.Unicode);
        var doc = new HtmlDocument();

        await doc.LoadAsync(path, detectEncodingFromByteOrderMarks: true);

        AssertLoaded(doc);
        await Assert.ThrowsAsync<ArgumentNullException>(() => doc.LoadAsync((string)null!, true));
    }

    [Fact]
    public async Task LoadAsync_PathEncodingDetectBom_ReadsFile()
    {
        var path = WriteTempFile(Html, Encoding.Latin1);
        var doc = new HtmlDocument();

        await doc.LoadAsync(path, Encoding.Latin1, detectEncodingFromByteOrderMarks: false);

        AssertLoaded(doc);
        await Assert.ThrowsAsync<ArgumentNullException>(() => doc.LoadAsync(path, null!, false));
    }

    [Fact]
    public async Task LoadAsync_PathEncodingDetectBomBufferSize_ReadsFile()
    {
        var path = WriteTempFile(Html, Encoding.Latin1);
        var doc = new HtmlDocument();

        await doc.LoadAsync(path, Encoding.Latin1, false, 16);

        AssertLoaded(doc);
        await Assert.ThrowsAsync<ArgumentNullException>(() => doc.LoadAsync((string)null!, Encoding.Latin1, false, 16));
    }

    [Fact]
    public async Task SaveAsync_PathEncoding_WritesFileInEncoding()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(Html);
        var path = Path.Combine(_tempDir, "out.html");

        await doc.SaveAsync(path, Encoding.Latin1);

        Assert.Equal(Encoding.Latin1.GetBytes(Html), await File.ReadAllBytesAsync(path));
        await Assert.ThrowsAsync<ArgumentNullException>(() => doc.SaveAsync(path, null!));
    }

    // ---------- XPath ----------

    [Fact]
    public void CreateNavigator_IsPositionedOnDocumentNode()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(Html);

        var nav = doc.CreateNavigator();

        Assert.Same(doc.DocumentNode, ((HtmlNodeNavigator)nav).CurrentNode);
        Assert.Equal("T", nav.SelectSingleNode("//title")!.Value);
    }

    // ---------- download with the shared client ----------

    [Fact]
    public async Task LoadHtmlAsync_UriEncoding_DownloadsWithSharedClient()
    {
        using var server = new LoopbackHttpServer(Encoding.Latin1.GetBytes(Html), "text/html; charset=iso-8859-1");
        var doc = new HtmlDocument();

        await doc.LoadHtmlAsync(server.Uri, Encoding.Latin1);

        AssertLoaded(doc);
    }

    [Fact]
    public async Task LoadHtmlAsync_UriEncoding_RejectsNonHttpUri()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => new HtmlDocument().LoadHtmlAsync(new Uri("file:///tmp/x.html"), Encoding.UTF8));
    }

    /// <summary>Serves a single fixed response on a random loopback port.</summary>
    private sealed class LoopbackHttpServer : IDisposable
    {
        private readonly HttpListener _listener = new();
        private readonly Task _loop;

        public Uri Uri { get; }

        public LoopbackHttpServer(byte[] body, string contentType)
        {
            int port;
            using (var probe = new TcpListener(IPAddress.Loopback, 0))
            {
                probe.Start();
                port = ((IPEndPoint)probe.LocalEndpoint).Port;
            }

            Uri = new Uri($"http://127.0.0.1:{port}/");
            _listener.Prefixes.Add(Uri.ToString());
            _listener.Start();
            _loop = Task.Run(async () =>
            {
                var ctx = await _listener.GetContextAsync();
                ctx.Response.ContentType = contentType;
                ctx.Response.ContentLength64 = body.Length;
                await ctx.Response.OutputStream.WriteAsync(body);
                ctx.Response.Close();
            });
        }

        public void Dispose()
        {
            _listener.Stop();
            _listener.Close();
            try { _loop.Wait(TimeSpan.FromSeconds(5)); } catch (AggregateException) { }
        }
    }
}

public class HtmlEntityNodeTests
{
    [Fact]
    public void Entitize_Node_ReturnsEntitizedCloneAndLeavesOriginal()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml("<p title='café'>café <b>©</b></p>");
        var p = doc.DocumentNode.SelectSingleNode("//p")!;

        var result = HtmlEntity.Entitize(p);

        Assert.NotSame(p, result);
        Assert.Equal("<p title='caf&eacute;'>caf&eacute; <b>&copy;</b></p>", result.OuterHtml);
        Assert.Equal("<p title='café'>café <b>©</b></p>", p.OuterHtml);
    }

    [Fact]
    public void Entitize_TextNode_EntitizesText()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml("<p>é</p>");

        var result = HtmlEntity.Entitize(doc.DocumentNode.SelectSingleNode("//p")!.FirstChild!);

        Assert.Equal("&eacute;", result.OuterHtml);
        Assert.Throws<ArgumentNullException>(() => HtmlEntity.Entitize((HtmlNode)null!));
    }
}
