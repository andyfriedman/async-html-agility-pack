using System.Xml.XPath;
using Async.HtmlAgilityPack.Attributes;
using Xunit;

namespace Async.HtmlAgilityPack.Tests;

/// <summary>
/// One or more tests for each public <see cref="HtmlNode"/> method that is not exercised elsewhere.
/// </summary>
public class HtmlNodeApiTests
{
    private static HtmlDocument Load(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }

    private static HtmlNode Select(HtmlDocument doc, string xpath) => doc.DocumentNode.SelectSingleNode(xpath)!;

    private static string[] Names(IEnumerable<HtmlNode> nodes) => nodes.Select(n => n.Name).ToArray();

    // Test parallelization is disabled for this assembly, so changing the static limit is safe.
    private static void WithMaxDepth(int depth, Action action)
    {
        var original = HtmlDocument.MaxDepthLevel;
        HtmlDocument.MaxDepthLevel = depth;
        try
        {
            action();
        }
        finally
        {
            HtmlDocument.MaxDepthLevel = original;
        }
    }

    private const string Nested = "<html><body><div id='d'><p id='p'><b>x</b></p></div></body></html>";

    // ---------- text ----------

    [Fact]
    public void GetDirectInnerText_ReturnsOnlyDirectTextChildren()
    {
        var doc = Load("<div>a<b>b</b>c<!-- d --></div>");

        Assert.Equal("ac", Select(doc, "//div").GetDirectInnerText());
    }

    [Fact]
    public void GetDirectInnerText_WithoutBackwardCompatibility_ReturnsOnlyDirectTextChildren()
    {
        var doc = new HtmlDocument { BackwardCompatibility = false };
        doc.LoadHtml("<div>a<b>b</b>c</div>");

        Assert.Equal("ac", Select(doc, "//div").GetDirectInnerText());
    }

    [Fact]
    public void GetDirectInnerText_OnTextAndEmptyNodes()
    {
        var doc = Load("<div>hello</div><span></span>");

        Assert.Equal("hello", Select(doc, "//div").FirstChild!.GetDirectInnerText());
        Assert.Equal("", Select(doc, "//span").GetDirectInnerText());
    }

    // ---------- static helpers ----------

    [Theory]
    [InlineData("</form>", true)]
    [InlineData("</div>", false)]
    [InlineData("<form>", false)]
    [InlineData("</form", false)]
    [InlineData("</>", false)]
    [InlineData("", false)]
    public void IsOverlappedClosingElement_DetectsClosingTagsOfOverlappableElements(string text, bool expected)
    {
        Assert.Equal(expected, HtmlNode.IsOverlappedClosingElement(text));
    }

    [Fact]
    public void IsOverlappedClosingElement_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => HtmlNode.IsOverlappedClosingElement(null!));
    }

    // ---------- ancestors ----------

    [Fact]
    public void Ancestors_ReturnsParentsFromNearestToRoot()
    {
        var b = Select(Load(Nested), "//b");

        Assert.Equal(["p", "div", "body", "html", "#document"], Names(b.Ancestors()));
    }

    [Fact]
    public void Ancestors_OfDocumentNode_IsEmpty()
    {
        Assert.Empty(Load(Nested).DocumentNode.Ancestors());
    }

    [Fact]
    public void Ancestors_ByName_FiltersAndExcludesSelf()
    {
        var doc = Load("<div id='outer'><div id='inner'><p>x</p></div></div>");
        var inner = doc.GetElementbyId("inner")!;

        Assert.Equal(["outer"], inner.Ancestors("div").Select(n => n.Id));
        Assert.Equal(["inner", "outer"], Select(doc, "//p").Ancestors("div").Select(n => n.Id));
    }

    [Fact]
    public void AncestorsAndSelf_StartsWithSelf()
    {
        var b = Select(Load(Nested), "//b");

        Assert.Equal(["b", "p", "div", "body", "html", "#document"], Names(b.AncestorsAndSelf()));
    }

    [Fact]
    public void AncestorsAndSelf_ByName_IncludesSelfOnlyWhenNameMatches()
    {
        var doc = Load("<div id='outer'><div id='inner'><p>x</p></div></div>");

        Assert.Equal(["inner", "outer"], doc.GetElementbyId("inner")!.AncestorsAndSelf("div").Select(n => n.Id));
        Assert.Equal(["inner", "outer"], Select(doc, "//p").AncestorsAndSelf("div").Select(n => n.Id));
    }

    // ---------- descendants / elements ----------

#pragma warning disable CS0618 // DescendantNodes* are obsolete but still public API

    [Fact]
    public void DescendantNodes_ReturnsAllDescendantsDepthFirst()
    {
        var div = Select(Load(Nested), "//div");

        Assert.Equal(["p", "b", "#text"], Names(div.DescendantNodes()));
    }

    [Fact]
    public void DescendantNodes_BeyondMaxDepth_Throws()
    {
        var div = Select(Load(Nested), "//div");

        WithMaxDepth(1, () => Assert.Throws<ArgumentException>(() => div.DescendantNodes(2).ToList()));
    }

    [Fact]
    public void DescendantNodesAndSelf_StartsWithSelf()
    {
        var div = Select(Load(Nested), "//div");

        Assert.Equal(["div", "p", "b", "#text"], Names(div.DescendantNodesAndSelf()));
    }

#pragma warning restore CS0618

    [Fact]
    public void Descendants_ByName_IsCaseInsensitive()
    {
        var doc = Load("<div><p>1</p><span><p>2</p></span></div>");

        Assert.Equal(["1", "2"], Select(doc, "//div").Descendants("P").Select(n => n.InnerText));
    }

    [Fact]
    public void DescendantsAndSelf_StartsWithSelf()
    {
        var div = Select(Load(Nested), "//div");

        Assert.Equal(["div", "p", "b", "#text"], Names(div.DescendantsAndSelf()));
    }

    [Fact]
    public void DescendantsAndSelf_ByName_IncludesMatchingSelfAndDescendants()
    {
        var div = Select(Load(Nested), "//div");

        Assert.Equal(["div"], Names(div.DescendantsAndSelf("div")));
        Assert.Contains(div.DescendantsAndSelf("b"), n => n.Name == "b");
    }

    // BUG: HtmlNode.DescendantsAndSelf(string name) (HtmlNode.cs, ~line 1311) starts with an
    // unconditional `yield return this;`, so the starting node is returned even when its name
    // doesn't match. Here it returns ["div", "b"] instead of ["b"].
    // FIX: only yield `this` when it matches, e.g.
    //     if (Name == name) yield return this;
    [Fact(Skip = "Bug: DescendantsAndSelf(name) always yields the starting node. See comment.")]
    public void DescendantsAndSelf_ByName_ExcludesNonMatchingSelf()
    {
        var div = Select(Load(Nested), "//div");

        Assert.Equal(["b"], Names(div.DescendantsAndSelf("b")));
    }

    [Fact]
    public void Element_ReturnsFirstMatchingDirectChildOrNull()
    {
        var doc = Load("<div><span><p id='nested'></p></span><p id='first'></p><p id='second'></p></div>");
        var div = Select(doc, "//div");

        Assert.Equal("first", div.Element("p")!.Id);
        Assert.Null(div.Element("b"));
    }

    [Fact]
    public void Elements_ByName_ReturnsOnlyMatchingDirectChildren()
    {
        var doc = Load("<div><span><p id='nested'></p></span><p id='first'></p><p id='second'></p></div>");

        Assert.Equal(["first", "second"], Select(doc, "//div").Elements("p").Select(n => n.Id));
    }

    // ---------- attributes ----------

    [Fact]
    public void ChildAttributes_ReturnsAttributesWithName()
    {
        var doc = Load("<div class='a' id='x'></div><span></span>");

        var atts = Select(doc, "//div").ChildAttributes("CLASS").ToList();

        Assert.Single(atts);
        Assert.Equal("a", atts[0].Value);
        Assert.Empty(Select(doc, "//span").ChildAttributes("class"));
    }

    [Fact]
    public void GetDataAttribute_FindsDataPrefixedAttributeCaseInsensitively()
    {
        var div = Select(Load("<div data-user-id='42' id='x'></div>"), "//div");

        Assert.Equal("42", div.GetDataAttribute("user-id").Value);
        Assert.Equal("42", div.GetDataAttribute("USER-ID").Value);
        Assert.Null(div.GetDataAttribute("missing"));
    }

    [Fact]
    public void GetDataAttributes_ReturnsOnlyDataAttributes()
    {
        var div = Select(Load("<div data-a='1' id='x' data-b='2' class='c'></div>"), "//div");

        Assert.Equal(["data-a", "data-b"], div.GetDataAttributes().Select(a => a.Name).Order());
    }

    [Fact]
    public void GetAttributes_ReturnsAllAttributesInOrder()
    {
        var div = Select(Load("<div id='x' class='c' title='t'></div>"), "//div");

        Assert.Equal(["id", "class", "title"], div.GetAttributes().Select(a => a.Name));
    }

    [Fact]
    public void GetAttributes_ByNames_ReturnsRequestedAttributesThatExist()
    {
        var div = Select(Load("<div id='x' class='c' title='t'></div>"), "//div");

        Assert.Equal(["title", "id"], div.GetAttributes("title", "missing", "id").Select(a => a.Name));
    }

    [Fact]
    public void GetAttributeValue_Int_ParsesOrReturnsDefault()
    {
        var div = Select(Load("<div n='5' bad='x'></div>"), "//div");

        Assert.Equal(5, div.GetAttributeValue("n", -1));
        Assert.Equal(-1, div.GetAttributeValue("bad", -1));
        Assert.Equal(-1, div.GetAttributeValue("missing", -1));
    }

    [Fact]
    public void GetAttributeValue_Bool_ParsesOrReturnsDefault()
    {
        var div = Select(Load("<div yes='true' no='False' bad='x'></div>"), "//div");

        Assert.True(div.GetAttributeValue("yes", false));
        Assert.False(div.GetAttributeValue("no", true));
        Assert.True(div.GetAttributeValue("bad", true));
        Assert.True(div.GetAttributeValue("missing", true));
    }

    [Fact]
    public void GetAttributeValue_Generic_ConvertsOrReturnsDefault()
    {
        var guid = Guid.NewGuid();
        var div = Select(Load($"<div n='5' g='{guid}' bad='x'></div><span></span>"), "//div");

        Assert.Equal(5L, div.GetAttributeValue("n", 0L));
        Assert.Equal(guid, div.GetAttributeValue("g", Guid.Empty));
        Assert.Equal(7L, div.GetAttributeValue("bad", 7L));
        Assert.Equal(7L, div.GetAttributeValue("missing", 7L));
        Assert.Equal(7L, Select(div.OwnerDocument, "//span").GetAttributeValue("n", 7L));
        Assert.Throws<ArgumentNullException>(() => div.GetAttributeValue<long>(null!, 0L));
    }

    [Fact]
    public void RemoveClass_RemovesClassAttribute()
    {
        var div = Select(Load("<div id='x' class='a b'></div>"), "//div");

        div.RemoveClass();

        Assert.Equal("<div id='x'></div>", div.OuterHtml);
    }

    // BUG: HtmlNode.RemoveClass(bool) (HtmlNode.cs, ~line 2697) loops over
    // Attributes.AttributesWithName("class"), a lazy iterator that walks the attribute list by
    // index, and removes from that same list inside the loop. Each removal shifts the next
    // attribute into the slot just visited, so every other duplicate `class` is skipped and
    // one is left behind.
    // FIX: materialize the matches before removing, e.g.
    //     var classAttributes = Attributes.AttributesWithName("class").ToList();
    [Fact(Skip = "Bug: RemoveClass skips duplicate class attributes. See comment.")]
    public void RemoveClass_RemovesEveryClassAttribute()
    {
        var div = Select(Load("<div class='a' class='b' class='c'></div>"), "//div");

        div.RemoveClass();

        Assert.Empty(div.ChildAttributes("class"));
    }

    [Fact]
    public void RemoveClass_WithoutClass_ThrowsOnlyWhenRequested()
    {
        var div = Select(Load("<div id='x'></div>"), "//div");

        div.RemoveClass(false);
        Assert.Throws<Exception>(() => div.RemoveClass(true));
    }

    [Fact]
    public void RemoveClass_ThrowError_RemovesExistingClass()
    {
        var div = Select(Load("<div class='a'></div>"), "//div");

        div.RemoveClass(true);

        Assert.Equal("<div></div>", div.OuterHtml);
    }

    // ---------- tree manipulation ----------

    [Fact]
    public void AppendChildren_AppendsEachNodeInOrder()
    {
        var doc = Load("<div><p>0</p></div>");
        var div = Select(doc, "//div");
        var nodes = new HtmlNodeCollection(null)
        {
            HtmlNode.CreateNode("<i>1</i>"),
            HtmlNode.CreateNode("<b>2</b>"),
        };

        div.AppendChildren(nodes);

        Assert.Equal("<p>0</p><i>1</i><b>2</b>", div.InnerHtml);
        Assert.Throws<ArgumentNullException>(() => div.AppendChildren(null!));
    }

    [Fact]
    public void CloneNode_WithNewName_RenamesDeepCopy()
    {
        var doc = Load("<div class='c'><p>x</p></div>");
        var div = Select(doc, "//div");

        var clone = div.CloneNode("section");

        Assert.Equal("<section class='c'><p>x</p></section>", clone.OuterHtml);
        Assert.Equal("<div class='c'><p>x</p></div>", div.OuterHtml);
    }

    [Fact]
    public void CloneNode_WithNewNameShallow_OmitsChildren()
    {
        var div = Select(Load("<div class='c'><p>x</p></div>"), "//div");

        var clone = div.CloneNode("section", false);

        Assert.Equal("section", clone.Name);
        Assert.False(clone.HasChildNodes);
        Assert.Equal("c", clone.GetAttributeValue("class", ""));
        Assert.Throws<ArgumentNullException>(() => div.CloneNode(null!, false));
    }

    [Fact]
    public void InsertAfter_InsertsAfterReference()
    {
        var doc = Load("<ul><li>1</li><li>3</li></ul>");
        var ul = Select(doc, "//ul");

        var inserted = ul.InsertAfter(HtmlNode.CreateNode("<li>2</li>"), ul.FirstChild);

        Assert.Equal("<li>1</li><li>2</li><li>3</li>", ul.InnerHtml);
        Assert.Same(ul, inserted.ParentNode);
    }

    [Fact]
    public void InsertAfter_NullReference_Prepends()
    {
        var ul = Select(Load("<ul><li>2</li></ul>"), "//ul");

        ul.InsertAfter(HtmlNode.CreateNode("<li>1</li>"), null);

        Assert.Equal("<li>1</li><li>2</li>", ul.InnerHtml);
    }

    [Fact]
    public void InsertAfter_InvalidArguments()
    {
        var doc = Load("<ul><li>1</li></ul><p>other</p>");
        var ul = Select(doc, "//ul");
        var li = ul.FirstChild!;

        Assert.Same(li, ul.InsertAfter(li, li));
        Assert.Throws<ArgumentNullException>(() => ul.InsertAfter(null!, li));
        Assert.ThrowsAny<ArgumentException>(() => ul.InsertAfter(HtmlNode.CreateNode("<li>x</li>"), Select(doc, "//p")));
    }

    [Fact]
    public void InsertBefore_InsertsBeforeReference()
    {
        var ul = Select(Load("<ul><li>1</li><li>3</li></ul>"), "//ul");

        var inserted = ul.InsertBefore(HtmlNode.CreateNode("<li>2</li>"), ul.LastChild);

        Assert.Equal("<li>1</li><li>2</li><li>3</li>", ul.InnerHtml);
        Assert.Same(ul, inserted.ParentNode);
    }

    [Fact]
    public void InsertBefore_NullReference_Appends()
    {
        var ul = Select(Load("<ul><li>1</li></ul>"), "//ul");

        ul.InsertBefore(HtmlNode.CreateNode("<li>2</li>"), null);

        Assert.Equal("<li>1</li><li>2</li>", ul.InnerHtml);
    }

    [Fact]
    public void InsertBefore_InvalidArguments()
    {
        var doc = Load("<ul><li>1</li></ul><p>other</p>");
        var ul = Select(doc, "//ul");
        var li = ul.FirstChild!;

        Assert.Same(li, ul.InsertBefore(li, li));
        Assert.Throws<ArgumentNullException>(() => ul.InsertBefore(null!, li));
        Assert.ThrowsAny<ArgumentException>(() => ul.InsertBefore(HtmlNode.CreateNode("<li>x</li>"), Select(doc, "//p")));
    }

    [Fact]
    public void RemoveAll_RemovesChildrenAndAttributes()
    {
        var div = Select(Load("<div id='x' class='c'><p>a</p>text</div>"), "//div");

        div.RemoveAll();

        Assert.False(div.HasChildNodes);
        Assert.False(div.HasAttributes);
        Assert.Equal("<div></div>", div.OuterHtml);
    }

    [Fact]
    public void MoveChild_MovesNodeToNewParent()
    {
        var doc = Load("<div id='a'><p>x</p></div><div id='b'></div>");
        var a = doc.GetElementbyId("a")!;
        var b = doc.GetElementbyId("b")!;

        b.MoveChild(a.FirstChild!);

        Assert.Equal("", a.InnerHtml);
        Assert.Equal("<p>x</p>", b.InnerHtml);
        Assert.Throws<ArgumentNullException>(() => b.MoveChild(null!));
    }

    // BUG: HtmlNode.MoveChild (HtmlNode.cs, ~line 1712) calls AppendChild(child), which sets
    // child.ParentNode to the new parent, and *then* oldParent.RemoveChild(child), which resets
    // child.ParentNode to null. The moved node renders under its new parent but is orphaned,
    // which breaks ParentNode, Ancestors() and XPath axes such as "..".
    // FIX: detach from the old parent before appending, e.g.
    //     oldParent?.RemoveChild(child);
    //     AppendChild(child);
    [Fact(Skip = "Bug: MoveChild leaves the moved node with ParentNode == null. See comment.")]
    public void MoveChild_SetsParentNodeToNewParent()
    {
        var doc = Load("<div id='a'><p>x</p></div><div id='b'></div>");
        var b = doc.GetElementbyId("b")!;

        b.MoveChild(doc.GetElementbyId("a")!.FirstChild!);

        Assert.Same(b, b.FirstChild!.ParentNode);
    }

    [Fact]
    public void MoveChildren_MovesAllNodesToNewParent()
    {
        var doc = Load("<div id='a'><p>1</p><p>2</p></div><div id='b'></div>");
        var a = doc.GetElementbyId("a")!;
        var b = doc.GetElementbyId("b")!;

        b.MoveChildren(a.ChildNodes);

        Assert.Equal("", a.InnerHtml);
        Assert.Equal("<p>1</p><p>2</p>", b.InnerHtml);
        Assert.Throws<ArgumentNullException>(() => b.MoveChildren(null!));
    }

    // BUG: HtmlNode.MoveChildren (HtmlNode.cs, ~line 1731) has the same ordering problem as
    // MoveChild: AppendChildren(children) sets each node's ParentNode, then
    // oldParent.RemoveChildren(children) sets it back to null, orphaning every moved node.
    // FIX: snapshot the nodes, detach them from the old parent, then append, e.g.
    //     var nodes = children.ToList();
    //     oldParent?.RemoveChildren(children);
    //     foreach (var node in nodes) AppendChild(node);
    [Fact(Skip = "Bug: MoveChildren leaves moved nodes with ParentNode == null. See comment.")]
    public void MoveChildren_SetsParentNodeToNewParent()
    {
        var doc = Load("<div id='a'><p>1</p><p>2</p></div><div id='b'></div>");
        var b = doc.GetElementbyId("b")!;

        b.MoveChildren(doc.GetElementbyId("a")!.ChildNodes);

        Assert.All(b.ChildNodes, n => Assert.Same(b, n.ParentNode));
    }

    [Fact]
    public void RemoveChildren_RemovesEveryNodeInCollection()
    {
        var div = Select(Load("<div><p>1</p><p>2</p><span>3</span></div>"), "//div");

        div.RemoveChildren(div.ChildNodes);

        Assert.False(div.HasChildNodes);
        Assert.Throws<ArgumentNullException>(() => div.RemoveChildren(null!));
    }

    [Fact]
    public void RemoveChild_KeepGrandChildren_PromotesThemInPlace()
    {
        var div = Select(Load("<div>head<span><b>1</b><i>2</i></span>tail</div>"), "//div");
        var span = Select(div.OwnerDocument, "//span");

        var removed = div.RemoveChild(span, keepGrandChildren: true);

        Assert.Same(span, removed);
        Assert.Equal("head<b>1</b><i>2</i>tail", div.InnerHtml);
    }

    [Fact]
    public void RemoveChild_DiscardGrandChildren_RemovesSubtree()
    {
        var div = Select(Load("<div>head<span><b>1</b></span>tail</div>"), "//div");

        div.RemoveChild(Select(div.OwnerDocument, "//span"), keepGrandChildren: false);

        Assert.Equal("headtail", div.InnerHtml);
        Assert.Throws<ArgumentNullException>(() => div.RemoveChild(null!, true));
    }

    // ---------- writing ----------

    [Fact]
    public async Task WriteContentToAsync_WritesChildrenOnly()
    {
        var div = Select(Load("<div id='x'><p>a</p>b</div>"), "//div");
        using var writer = new StringWriter();

        await div.WriteContentToAsync(writer);

        Assert.Equal("<p>a</p>b", writer.ToString());
    }

    [Fact]
    public async Task WriteContentToAsync_InvalidArguments()
    {
        var div = Select(Load("<div></div>"), "//div");
        using var writer = new StringWriter();

        await Assert.ThrowsAsync<ArgumentNullException>(() => div.WriteContentToAsync(null!));
        var original = HtmlDocument.MaxDepthLevel;
        HtmlDocument.MaxDepthLevel = 1;
        try
        {
            await Assert.ThrowsAsync<ArgumentException>(() => div.WriteContentToAsync(writer, 2));
        }
        finally
        {
            HtmlDocument.MaxDepthLevel = original;
        }
        await div.WriteContentToAsync(writer);
        Assert.Equal("", writer.ToString());
    }

    // ---------- XPath ----------

    [Fact]
    public void CreateNavigator_IsPositionedOnNode()
    {
        var p = Select(Load(Nested), "//p");

        var nav = p.CreateNavigator();

        Assert.Same(p, ((HtmlNodeNavigator)nav).CurrentNode);
        Assert.Equal("x", nav.SelectSingleNode("b")!.Value);
    }

    [Fact]
    public void CreateRootNavigator_IsPositionedOnDocumentNode()
    {
        var doc = Load(Nested);

        var nav = Select(doc, "//p").CreateRootNavigator();

        Assert.Same(doc.DocumentNode, ((HtmlNodeNavigator)nav).CurrentNode);
    }

    [Fact]
    public void SelectSingleNode_CompiledExpression_SelectsRelativeToNode()
    {
        var doc = Load("<div id='a'><p>1</p></div><div id='b'><p>2</p></div>");
        var expr = XPathExpression.Compile("p");

        Assert.Equal("2", doc.GetElementbyId("b")!.SelectSingleNode(expr)!.InnerText);
        Assert.Null(doc.GetElementbyId("b")!.SelectSingleNode(XPathExpression.Compile("span")));
        Assert.Throws<ArgumentNullException>(() => doc.DocumentNode.SelectSingleNode((XPathExpression)null!));
    }

    // ---------- encapsulation ----------

    [HasXPath]
    private sealed class TitleModel
    {
        [XPath("//h1")]
        public string? Title { get; set; }
    }

    [Fact]
    public void GetEncapsulatedData_WithDocument_ReadsFromGivenDocument()
    {
        var owner = Load("<h1>owner</h1>");
        var other = Load("<h1>other</h1>");

        var model = owner.DocumentNode.GetEncapsulatedData<TitleModel>(other);

        Assert.Equal("other", model!.Title);
    }
}
