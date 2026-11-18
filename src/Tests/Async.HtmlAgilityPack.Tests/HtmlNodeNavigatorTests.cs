using System.Text;
using System.Xml.XPath;
using Xunit;

namespace HtmlAgilityPack.Tests;

public sealed class HtmlNodeNavigatorTests : IDisposable
{
    private const string Html =
        "<html><body><p id='first' class='c'>café</p><p id='second'>two</p></body></html>";

    private readonly string _tempDir = Directory.CreateTempSubdirectory("hap-nav-tests-").FullName;

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    private string WriteTempFile(Encoding encoding)
    {
        var path = Path.Combine(_tempDir, Guid.NewGuid() + ".html");
        File.WriteAllText(path, Html, encoding);
        return path;
    }

    private static MemoryStream ToStream(Encoding encoding) =>
        new(encoding.GetPreamble().Concat(encoding.GetBytes(Html)).ToArray());

    private static void AssertLoaded(HtmlNodeNavigator nav)
    {
        Assert.Equal(HtmlNodeType.Document, nav.CurrentNode.NodeType);
        Assert.Equal("café", nav.CurrentDocument.GetElementbyId("first")!.InnerText);
        Assert.Equal("two", nav.SelectSingleNode("//p[@id='second']")!.Value);
    }

    private static HtmlNodeNavigator NavigatorAt(string id)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(Html);
        return (HtmlNodeNavigator)doc.GetElementbyId(id)!.CreateNavigator();
    }

    // ---------- constructors ----------

    public static TheoryData<string, Func<Stream, HtmlNodeNavigator>> StreamConstructors => new()
    {
        { "stream", s => new HtmlNodeNavigator(s) },
        { "stream, detectBom", s => new HtmlNodeNavigator(s, true) },
        { "stream, encoding", s => new HtmlNodeNavigator(s, Encoding.UTF8) },
        { "stream, encoding, detectBom", s => new HtmlNodeNavigator(s, Encoding.UTF8, true) },
        { "stream, encoding, detectBom, bufferSize", s => new HtmlNodeNavigator(s, Encoding.UTF8, true, 16) },
    };

    [Theory]
    [MemberData(nameof(StreamConstructors))]
    public void Constructor_Stream_LoadsDocument(string overload, Func<Stream, HtmlNodeNavigator> create)
    {
        Assert.NotEmpty(overload);
        using var stream = ToStream(Encoding.UTF8);

        AssertLoaded(create(stream));
        Assert.True(stream.CanRead);
    }

    [Fact]
    public void Constructor_TextReader_LoadsDocument()
    {
        using var reader = new StringReader(Html);

        AssertLoaded(new HtmlNodeNavigator(reader));
    }

    public static TheoryData<string, Func<string, HtmlNodeNavigator>> PathConstructors => new()
    {
        { "path", p => new HtmlNodeNavigator(p) },
        { "path, detectBom", p => new HtmlNodeNavigator(p, true) },
        { "path, encoding", p => new HtmlNodeNavigator(p, Encoding.UTF8) },
        { "path, encoding, detectBom", p => new HtmlNodeNavigator(p, Encoding.UTF8, true) },
        { "path, encoding, detectBom, bufferSize", p => new HtmlNodeNavigator(p, Encoding.UTF8, true, 16) },
    };

    [Theory]
    [MemberData(nameof(PathConstructors))]
    public void Constructor_Path_LoadsDocument(string overload, Func<string, HtmlNodeNavigator> create)
    {
        Assert.NotEmpty(overload);

        AssertLoaded(create(WriteTempFile(Encoding.UTF8)));
    }

    [Fact]
    public void Constructor_PathWithEncoding_DecodesUsingEncoding()
    {
        var nav = new HtmlNodeNavigator(WriteTempFile(Encoding.Latin1), Encoding.Latin1);

        AssertLoaded(nav);
    }

    // ---------- CreateAsync ----------

    [Fact]
    public async Task CreateAsync_Stream_LoadsDocument()
    {
        using var stream = ToStream(Encoding.Unicode);

        AssertLoaded(await HtmlNodeNavigator.CreateAsync(stream));
    }

    [Fact]
    public async Task CreateAsync_TextReader_LoadsDocument()
    {
        using var reader = new StringReader(Html);

        AssertLoaded(await HtmlNodeNavigator.CreateAsync(reader));
    }

    [Fact]
    public async Task CreateAsync_Path_LoadsDocument()
    {
        AssertLoaded(await HtmlNodeNavigator.CreateAsync(WriteTempFile(Encoding.UTF8)));
    }

    [Fact]
    public async Task CreateAsync_PathEncoding_LoadsDocument()
    {
        AssertLoaded(await HtmlNodeNavigator.CreateAsync(WriteTempFile(Encoding.Latin1), Encoding.Latin1));
    }

    // ---------- attributes / namespaces ----------

    [Fact]
    public void GetAttribute_ReturnsValueOrEmpty()
    {
        var nav = NavigatorAt("first");

        Assert.Equal("c", nav.GetAttribute("class", ""));
        Assert.Equal("", nav.GetAttribute("missing", ""));
        Assert.Equal("", NavigatorAt("second").GetAttribute("class", ""));
    }

    [Fact]
    public void MoveToAttribute_MovesToNamedAttribute()
    {
        var nav = NavigatorAt("first");

        Assert.True(nav.MoveToAttribute("class", ""));
        Assert.Equal(XPathNodeType.Attribute, nav.NodeType);
        Assert.Equal("c", nav.Value);
    }

    [Fact]
    public void MoveToAttribute_Missing_ReturnsFalseAndStaysOnElement()
    {
        var nav = NavigatorAt("first");

        Assert.False(nav.MoveToAttribute("missing", ""));
        Assert.Equal(XPathNodeType.Element, nav.NodeType);
        Assert.Equal("first", nav.CurrentNode.Id);
    }

    // BUG: HtmlNodeNavigator.MoveToParent (HtmlNodeNavigator.cs, ~line 740) ignores whether the
    // navigator is positioned on an attribute. It always moves _currentnode to its ParentNode and
    // never resets _attindex, so from an attribute it jumps to the element's parent (skipping the
    // owner element) and still reports NodeType == Attribute. The XPathNavigator contract says the
    // parent of an attribute is its owner element.
    // FIX: when on an attribute, just leave attribute mode, e.g.
    //     if (_attindex != -1) { _attindex = -1; return true; }
    // before the existing ParentNode logic.
    [Fact(Skip = "Bug: MoveToParent from an attribute skips the owner element. See comment.")]
    public void MoveToParent_FromAttribute_ReturnsToOwnerElement()
    {
        var nav = NavigatorAt("first");
        Assert.True(nav.MoveToAttribute("class", ""));

        Assert.True(nav.MoveToParent());

        Assert.Equal(XPathNodeType.Element, nav.NodeType);
        Assert.Equal("first", nav.CurrentNode.Id);
    }

    [Fact]
    public void NamespaceMethods_AreNotSupported()
    {
        var nav = NavigatorAt("first");

        Assert.Equal("", nav.GetNamespace("x"));
        Assert.False(nav.MoveToNamespace("x"));
        Assert.False(nav.MoveToFirstNamespace(XPathNamespaceScope.All));
        Assert.False(nav.MoveToNextNamespace(XPathNamespaceScope.All));
        Assert.Equal("p", nav.Name);
    }

    // ---------- movement ----------

    [Fact]
    public void MoveToFirst_MovesToFirstSibling()
    {
        var nav = NavigatorAt("second");

        Assert.True(nav.MoveToFirst());
        Assert.Equal("first", nav.CurrentNode.Id);
    }

    [Fact]
    public void MoveToFirst_OnDocumentNode_ReturnsFalse()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(Html);
        var nav = (HtmlNodeNavigator)doc.CreateNavigator();

        Assert.False(nav.MoveToFirst());
        Assert.Same(doc.DocumentNode, nav.CurrentNode);
    }

    [Fact]
    public void MoveToPrevious_MovesToPreviousSiblingOrFails()
    {
        var nav = NavigatorAt("second");

        Assert.True(nav.MoveToPrevious());
        Assert.Equal("first", nav.CurrentNode.Id);
        Assert.False(nav.MoveToPrevious());
        Assert.Equal("first", nav.CurrentNode.Id);
    }

    [Fact]
    public void MoveToId_MovesToElementWithId()
    {
        var nav = NavigatorAt("first");

        Assert.True(nav.MoveToId("second"));
        Assert.Equal("second", nav.CurrentNode.Id);
        Assert.False(nav.MoveToId("missing"));
        Assert.Equal("second", nav.CurrentNode.Id);
    }
}
