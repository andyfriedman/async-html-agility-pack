using System.Diagnostics;
using Xunit;

namespace HtmlAgilityPack.Tests;

public class EncapsulatorTests
{
    [Fact]
    public void EncapsulatedOuterHtml_Test()
    {
        var html = @"
<div>
<a href='3.html' class='single'>3</a>
<div>hello
<a href='1.html'>1</a>
<a href='2.html'>2</a>
</div>
<div>world</div>
</div>
";
        var document = new HtmlDocument();
        document.LoadHtml(html);
        var outerHtml = document.DocumentNode.GetEncapsulatedData<OuterHtml>();
        Assert.NotNull(outerHtml);
        Assert.Equal("3.html", outerHtml.Item3.Href);
        Assert.Equal("3", outerHtml.Item3.Name);

        Assert.Equal(3, outerHtml.Items.Count);
        Assert.True(outerHtml.Items.All(o => o.Href is not null));
    }

    [Fact]
    public void Encapsulation_Test2()
    {
        var document = new HtmlDocument();
        document.LoadHtml(EncapsulationTestHtml.Html);
        var testHtml = document.DocumentNode.GetEncapsulatedData<EncapsulationTestHtml>();

        Assert.NotNull(testHtml);
        Assert.Equal("Test", testHtml.Title);
        Assert.Equal("Test", testHtml.H1);
        Assert.NotEmpty(testHtml.Link1);
        Assert.NotNull(testHtml.Link2);
        Assert.NotEmpty(testHtml.Link2);
        Assert.NotNull(testHtml.Link3);

        Assert.NotNull(testHtml.Form);

        Assert.Equal(2, testHtml.Articles.Count);
        Assert.True(testHtml.Articles.All(o => o.Id is not null));
    }
}

[HasXPath]
public class OuterHtml
{
    [XPath("//a", ReturnType.OuterHtml)]
    public List<OuterHtmlItem> Items { get; set; } = [];

    [XPath("//a[@class='single']", ReturnType.OuterHtml)]
    public OuterHtmlItem Item3 { get; set; } = null!;

    [HasXPath]
    public class OuterHtmlItem
    {
        [XPath("a", "href")]
        [SkipNodeNotFound]
        public string? Href { get; set; }

        [XPath("a")]
        [SkipNodeNotFound]
        public string? Name { get; set; }
    }
}
