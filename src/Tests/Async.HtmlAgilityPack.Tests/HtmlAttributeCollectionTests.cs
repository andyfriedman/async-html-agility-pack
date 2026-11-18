using Xunit;

namespace HtmlAgilityPack.Tests;

public class HtmlAttributeCollectionTests
{
    private static HtmlNode LoadDiv(string html = "<div a='1' b='2'></div>")
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc.DocumentNode.SelectSingleNode("//div")!;
    }

    private static string[] Names(HtmlNode node) => node.Attributes.Select(a => a.Name).ToArray();

    [Fact]
    public void Add_AppendsAttribute()
    {
        var div = LoadDiv();
        var att = div.OwnerDocument.CreateAttribute("c", "3");

        div.Attributes.Add(att);

        Assert.Equal(["a", "b", "c"], Names(div));
        Assert.Same(att, div.Attributes["c"]);
        Assert.Same(div, att.OwnerNode);
    }

    [Fact]
    public void AddRange_Attributes_AppendsAllInOrder()
    {
        var div = LoadDiv();
        var doc = div.OwnerDocument;

        div.Attributes.AddRange([doc.CreateAttribute("c", "3"), doc.CreateAttribute("d", "4")]);

        Assert.Equal(["a", "b", "c", "d"], Names(div));
        Assert.Equal("4", div.Attributes["d"]!.Value);
    }

    [Fact]
    public void AddRange_Dictionary_AppendsNameValuePairs()
    {
        var div = LoadDiv("<div></div>");

        div.Attributes.AddRange(new Dictionary<string, string> { ["x"] = "1", ["y"] = "2" });

        Assert.Equal("1", div.GetAttributeValue("x", ""));
        Assert.Equal("2", div.GetAttributeValue("y", ""));
        Assert.Equal("<div x=\"1\" y=\"2\"></div>", div.OuterHtml);
    }

    [Fact]
    public void Append_Name_CreatesAttributeWithEmptyValue()
    {
        var div = LoadDiv("<div></div>");

        var att = div.Attributes.Append("hidden");

        Assert.Equal("hidden", att.Name);
        Assert.Same(att, div.Attributes["hidden"]);
        Assert.Same(div, att.OwnerNode);
    }

    [Fact]
    public void Contains_Attribute_MatchesByReference()
    {
        var div = LoadDiv();
        var other = div.OwnerDocument.CreateAttribute("a", "1");

#pragma warning disable xUnit2017 // the method under test is Contains itself
        Assert.True(div.Attributes.Contains(div.Attributes["a"]!));
        Assert.False(div.Attributes.Contains(other));
#pragma warning restore xUnit2017
    }

    [Fact]
    public void Contains_Name_IsCaseInsensitive()
    {
        var div = LoadDiv();

        Assert.True(div.Attributes.Contains("a"));
        Assert.True(div.Attributes.Contains("B"));
        Assert.False(div.Attributes.Contains("c"));
    }

    [Fact]
    public void CopyTo_CopiesAttributesAtOffset()
    {
        var div = LoadDiv();
        var array = new HtmlAttribute[3];

        div.Attributes.CopyTo(array, 1);

        Assert.Null(array[0]);
        Assert.Equal("a", array[1].Name);
        Assert.Equal("b", array[2].Name);
    }

    [Fact]
    public void IndexOf_ReturnsPositionOrMinusOne()
    {
        var div = LoadDiv();

        Assert.Equal(0, div.Attributes.IndexOf(div.Attributes["a"]!));
        Assert.Equal(1, div.Attributes.IndexOf(div.Attributes["b"]!));
        Assert.Equal(-1, div.Attributes.IndexOf(div.OwnerDocument.CreateAttribute("a")));
    }

    [Fact]
    public void Insert_PlacesAttributeAtIndex()
    {
        var div = LoadDiv();
        var att = div.OwnerDocument.CreateAttribute("mid", "m");

        div.Attributes.Insert(1, att);

        Assert.Equal(["a", "mid", "b"], Names(div));
        Assert.Same(att, div.Attributes["mid"]);
        Assert.Same(div, att.OwnerNode);
    }

    [Fact]
    public void Insert_Null_Throws()
    {
        var div = LoadDiv();

        Assert.Throws<ArgumentNullException>(() => div.Attributes.Insert(0, null!));
    }

    [Fact]
    public void Prepend_InsertsAtStartAndReturnsAttribute()
    {
        var div = LoadDiv();
        var att = div.OwnerDocument.CreateAttribute("first", "0");

        var result = div.Attributes.Prepend(att);

        Assert.Same(att, result);
        Assert.Equal(["first", "a", "b"], Names(div));
    }

    [Fact]
    public void Remove_RemovesAllAttributes()
    {
        var div = LoadDiv();

        div.Attributes.Remove();

        Assert.Empty(div.Attributes);
        Assert.False(div.Attributes.Contains("a"));
        Assert.Null(div.Attributes["a"]);
        Assert.Equal("<div></div>", div.OuterHtml);
    }
}

public class HtmlAttributeTests
{
    [Fact]
    public void CompareTo_OrdersByNameOrdinal()
    {
        var doc = new HtmlDocument();
        var a = doc.CreateAttribute("alpha");
        var b = doc.CreateAttribute("beta");
        var a2 = doc.CreateAttribute("alpha");

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a2));
    }

    [Fact]
    public void CompareTo_NonAttribute_Throws()
    {
        var att = new HtmlDocument().CreateAttribute("x");

        Assert.Throws<ArgumentException>(() => att.CompareTo("x"));
        Assert.Throws<ArgumentException>(() => att.CompareTo(null));
    }

    [Fact]
    public void Remove_DetachesAttributeFromOwnerNode()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml("<p id='x' class='y'></p>");
        var p = doc.DocumentNode.SelectSingleNode("//p")!;

        p.Attributes["class"]!.Remove();

        Assert.Null(p.Attributes["class"]);
        Assert.Equal("<p id='x'></p>", p.OuterHtml);
    }

    [Fact]
    public void Remove_WithoutOwnerNode_DoesNothing()
    {
        var att = new HtmlDocument().CreateAttribute("x");

        att.Remove();

        Assert.Null(att.OwnerNode);
    }
}
