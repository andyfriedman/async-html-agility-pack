using Xunit;

namespace HtmlAgilityPack.Tests;

public class HtmlNodeCollectionTests
{
    private const string Html =
        "<div id='a'><p id='a1'><b id='a1b'>x</b></p><span id='a2'></span></div>" +
        "<div id='b'><p id='b1'></p></div>";

    private static HtmlDocument Load()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(Html);
        return doc;
    }

    private static HtmlNodeCollection Divs(HtmlDocument doc) => doc.DocumentNode.SelectNodes("//div")!;

    private static string[] Ids(IEnumerable<HtmlNode> nodes) =>
        nodes.Where(n => n.NodeType == HtmlNodeType.Element).Select(n => n.Id ?? "").ToArray();

    [Fact]
    public void Clear_EmptiesCollectionAndDetachesNodes()
    {
        var doc = Load();
        var a = doc.GetElementbyId("a")!;
        var children = a.ChildNodes.ToList();

        a.ChildNodes.Clear();

        Assert.Empty(a.ChildNodes);
        Assert.All(children, n =>
        {
            Assert.Null(n.ParentNode);
            Assert.Null(n.NextSibling);
            Assert.Null(n.PreviousSibling);
        });
    }

    [Fact]
    public void Contains_MatchesByReference()
    {
        var doc = Load();
        var a = doc.GetElementbyId("a")!;

#pragma warning disable xUnit2017 // the method under test is Contains itself
        Assert.True(a.ChildNodes.Contains(doc.GetElementbyId("a1")!));
        Assert.False(a.ChildNodes.Contains(doc.GetElementbyId("b1")!));
#pragma warning restore xUnit2017
    }

    [Fact]
    public void IndexOf_ReturnsPositionOrMinusOne()
    {
        var doc = Load();
        var a = doc.GetElementbyId("a")!;

        Assert.Equal(0, a.ChildNodes.IndexOf(doc.GetElementbyId("a1")!));
        Assert.Equal(1, a.ChildNodes.IndexOf(doc.GetElementbyId("a2")!));
        Assert.Equal(-1, a.ChildNodes.IndexOf(doc.GetElementbyId("b1")!));
    }

    [Fact]
    public void FindFirst_Static_SearchesDepthFirstCaseInsensitively()
    {
        var doc = Load();

        Assert.Equal("a1b", HtmlNodeCollection.FindFirst(doc.DocumentNode.ChildNodes, "B")!.Id);
        Assert.Equal("a1", HtmlNodeCollection.FindFirst(doc.DocumentNode.ChildNodes, "p")!.Id);
        Assert.Null(HtmlNodeCollection.FindFirst(doc.DocumentNode.ChildNodes, "table"));
    }

    [Fact]
    public void FindFirst_Instance_SearchesThisCollection()
    {
        var doc = Load();
        var b = doc.GetElementbyId("b")!;

        Assert.Equal("b1", b.ChildNodes.FindFirst("p")!.Id);
        Assert.Null(b.ChildNodes.FindFirst("span"));
    }

    [Fact]
    public void Descendants_ReturnsDescendantsOfEveryNode()
    {
        Assert.Equal(["a1", "a1b", "a2", "b1"], Ids(Divs(Load()).Descendants()));
    }

    [Fact]
    public void Descendants_ByName_FiltersDescendants()
    {
        Assert.Equal(["a1", "b1"], Ids(Divs(Load()).Descendants("p")));
    }

    [Fact]
    public void Elements_ReturnsDirectChildrenOfEveryNode()
    {
        Assert.Equal(["a1", "a2", "b1"], Ids(Divs(Load()).Elements()));
    }

    [Fact]
    public void Elements_ByName_FiltersDirectChildren()
    {
        Assert.Equal(["a2"], Ids(Divs(Load()).Elements("span")));
    }

    [Fact]
    public void Nodes_ReturnsAllChildNodesIncludingText()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml("<div>t1<p>p</p></div><div>t2</div>");

        var nodes = doc.DocumentNode.SelectNodes("//div")!.Nodes().ToList();

        Assert.Equal(["#text", "p", "#text"], nodes.Select(n => n.Name));
        Assert.Equal(["t1", "p", "t2"], nodes.Select(n => n.InnerText));
    }
}
