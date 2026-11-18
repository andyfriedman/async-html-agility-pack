using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using Xunit;


namespace HtmlAgilityPack.Tests
{
    public class HtmlDocumentTests
    {
        private readonly string _contentDirectory = Path.Combine(AppContext.BaseDirectory, "files") + Path.DirectorySeparatorChar;

        private async Task<HtmlDocument> GetMshomeDocumentAsync()
        {
            var doc = new HtmlDocument();
            await doc.LoadAsync(Path.Combine(_contentDirectory, "mshome.htm"));
            return doc;
        }


        [Fact]
        public void SelectEventAttributesTest()
        {
	        String xpath = "//* [@onkeypress or @onkeydown or @onkeyup or @onclick or @ondblclick or @onmousedown or @onmouseup or @onmouseover or @onmousemove or @onmouseout or @onmouseenter or @onmouseleave or @onmousewheel or @oncontextmenu or @onabort or @onbeforeunload or @onerror or @onload or @onmove or @onresize or @onscroll or @onstop or @onunload or @onreset or @onsubmit or @onblur or @onchange or @onfocus or @onfocusin or @onfocusout or @oninput or @onbeforeactivate or @onactivate or @onbefordeactivate or @ondeactivate or @onbounce or @onfinish or @onstart or @onbeforecopy or @onbeforecut or @onbeforeeditfocus or @onbeforepaste or @onbeforeupdate or @oncopy or @oncut or @ondrag or @ondragdrop or @ondragend or @ondragenter or @ondragleave or @ondragover or @ondragstart or @ondrop or @onlosecapture or @onpaste or @onselect or @onselectstart or @oncontrolselect or @onmovestart or @onmoveend or @onafterupdate or @oncellchange or @ondataavailable or @ondatasetchanged or @ondatasetcomplete or @onerrorupdate or @onrowenter or @onrowexit or @onrowsdelete or @onrowsinserted or @onafterprint or @onbeforeprint or @onfilterchange or @onhelp or @onpropertychange or @onreadystatechange]";
	        var doc = new HtmlAgilityPack.HtmlDocument();
	        doc.LoadHtml(@"<html><body><div id='foo' onclick='bar'><span> some</span> text</div></body></html>");
	        for (int i = 0; i < 100000; i++)
	        {
		        doc.DocumentNode.SelectNodes(xpath)!.ToList();
	        }
        }


		[Fact]
        public void HtmlAgilityPack_AttributeCollectionBug()
        {
            { 
            const string firstAttrName = "first";
            const string secondAttrName = "second";
            const string value = "value";

            HtmlNode firstNode = HtmlNode.CreateNode("<div></div>");
            firstNode.Attributes.Add(firstAttrName, value);

            HtmlNode secondNode = HtmlNode.CreateNode("<div></div>");
            secondNode.Attributes.Add(secondAttrName, value);
       
            secondNode.Attributes[0] = firstNode.Attributes[0];

            Assert.NotNull(secondNode.Attributes[0]);
            Assert.Equal(firstAttrName, secondNode.Attributes[0].Name);

            Assert.NotNull(secondNode.Attributes[firstAttrName]);
            Assert.Equal(firstAttrName, secondNode.Attributes[firstAttrName]!.Name);

            Assert.Null(secondNode.Attributes   [secondAttrName]);
            }

            {
                const string firstAttrName = "first";
                const string secondAttrName = "second";
                const string value = "value";

                HtmlNode firstNode = HtmlNode.CreateNode("<div></div>");
                firstNode.Attributes.Add(firstAttrName, value);

                HtmlNode secondNode = HtmlNode.CreateNode("<div></div>");
                secondNode.Attributes.Add(secondAttrName, value);
                var a = secondNode.Attributes[secondAttrName];
                secondNode.Attributes[secondAttrName] = firstNode.Attributes[firstAttrName]!;

                Assert.NotNull(secondNode.Attributes[firstAttrName]);
                Assert.Equal(firstAttrName, secondNode.Attributes[firstAttrName]!.Name);

                Assert.NotNull(secondNode.Attributes[0]);
                Assert.Equal(firstAttrName, secondNode.Attributes[firstAttrName]!.Name);

                Assert.Null(secondNode.Attributes[secondAttrName]);
            }
        }

        [Fact]
        public void TextInsideScriptTagShouldHaveCorrectStreamPosition()
        {
            {
                var document = new HtmlDocument();
                document.LoadHtml(@"<scrapt>foo</scrapt>");
                var scraptText = document.DocumentNode.FirstChild!.FirstChild!;
                Assert.Equal(8, scraptText.StreamPosition);
                Assert.Equal(1, scraptText.Line);
                Assert.Equal(8, scraptText.LinePosition);
            }
            {
                var document = new HtmlDocument();
                document.LoadHtml(@"<script>foo</script>");
                var scriptText = document.DocumentNode.FirstChild!.FirstChild!;
                Assert.Equal(8, scriptText.StreamPosition);
                Assert.Equal(1, scriptText.Line);
                Assert.Equal(8, scriptText.LinePosition);
            }
            {
                var document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml("\r\n<scrapt>foo</scrapt>");
                var scraptText = document.DocumentNode.LastChild!.FirstChild!;
                Assert.Equal(10, scraptText.StreamPosition);
                Assert.Equal(2, scraptText.Line);
                Assert.Equal(8, scraptText.LinePosition);
            }


            {
                var document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml("\r\n<script>foo</script>");
                var scriptText = document.DocumentNode.LastChild!.FirstChild!;
                Assert.Equal(10, scriptText.StreamPosition);
                Assert.Equal(2, scriptText.Line);
                Assert.Equal(8, scriptText.LinePosition);
            }
        }

        [Fact]
        public void CreateAttribute()
        {
            var doc = new HtmlDocument();
            var a = doc.CreateAttribute("href");
            Assert.Equal("href", a.Name);
        }

        [Fact]
        public void CreateAttributeWithEncodedText()
        {
            var doc = new HtmlDocument();
            var a = doc.CreateAttribute("href", "http://something.com\"&<>");
            Assert.Equal("href", a.Name);
            Assert.Equal("http://something.com\"&<>", a.Value);
        }

        [Fact]
        public void CreateAttributeWithText()
        {
            var doc = new HtmlDocument();
            var a = doc.CreateAttribute("href", "http://something.com");
            Assert.Equal("href", a.Name);
            Assert.Equal("http://something.com", a.Value);
        }

        [Fact]
        public void testTEXTAREA()
        {
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(@"<script><div>hello</div></script><TEXTAREA>Text in the <div>hello</div>area</TEXTAREA>");
                HtmlNodeCollection? divs = doc.DocumentNode.SelectNodes("//div");

                Assert.Null(divs);

                HtmlNode? ta = doc.DocumentNode.SelectSingleNode("//textarea");
                Assert.NotNull(ta);
                Assert.Contains("div", ta.InnerHtml);
            }
        }

        [Fact]
        public void CreateElement()
        {
            var doc = new HtmlDocument();
            var a = doc.CreateElement("a");
            Assert.Equal("a", a.Name);
            Assert.Equal(HtmlNodeType.Element, a.NodeType);
        }

        [Fact]
        public void TestBr()
        {
	        {
		        var html = @" </br>a</br>";

		        var doc = new HtmlAgilityPack.HtmlDocument();
		        doc.LoadHtml(html);
		        var docNode = doc.DocumentNode.InnerHtml;
                Assert.Equal(4, doc.DocumentNode.ChildNodes.Count);
	        }
	        {
		        var html = @" <br>a<br>";

		        var doc = new HtmlAgilityPack.HtmlDocument();
		        doc.LoadHtml(html);
		        var docNode = doc.DocumentNode.InnerHtml;
		        Assert.Equal(4, doc.DocumentNode.ChildNodes.Count);
            }
        }

        [Fact]
        public void CreateTextNodeWithText()
        {
            var doc = new HtmlDocument();
            var a = doc.CreateTextNode("something");
            Assert.Equal("something", a.InnerText);
            Assert.Equal(HtmlNodeType.Text, a.NodeType);
        }

        [Fact]
        public void HtmlEncode()
        {
            var result = HtmlDocument.HtmlEncode("http://something.com\"&<>");
            Assert.Equal("http://something.com&quot;&amp;&lt;&gt;", result);
        }

        [Fact]
        public async Task TestParseAsync()
        {
            var doc = await GetMshomeDocumentAsync();
            Assert.True(doc.DocumentNode.Descendants().Count() > 0);
        }

        [Fact]
        public async Task TestParseSaveParseAsync()
        {
            var doc = await GetMshomeDocumentAsync();
            var doc1desc =
                doc.DocumentNode.Descendants().Where(x => !string.IsNullOrWhiteSpace(x.InnerText)).ToList();
            var tempSavePath = Path.Combine(Path.GetTempPath(), $"testsaveparse_{Guid.NewGuid():N}.html");
            try
            {
            await doc.SaveAsync(tempSavePath);

            var doc2 = new HtmlDocument();
            await doc2.LoadAsync(tempSavePath);
            var doc2desc =
                doc2.DocumentNode.Descendants().Where(x => !string.IsNullOrWhiteSpace(x.InnerText)).ToList();
            Assert.Equal(doc1desc.Count, doc2desc.Count);
            }
            finally
            {
                if (File.Exists(tempSavePath)) File.Delete(tempSavePath);
            }
        }

        [Fact]
        public async Task TestRemoveUpdatesPreviousSiblingAsync()
        {
            var doc = await GetMshomeDocumentAsync();
            var docDesc = doc.DocumentNode.Descendants().ToList();
            var toRemove = docDesc[1200];
            var toRemovePrevSibling = toRemove.PreviousSibling;
            var toRemoveNextSibling = toRemove.NextSibling;
            toRemove.Remove();
            Assert.Same(toRemovePrevSibling, toRemoveNextSibling!.PreviousSibling);
        }

        [Fact]
        public async Task TestReplaceUpdatesSiblingsAsync()
        {
            var doc = await GetMshomeDocumentAsync();
            var docDesc = doc.DocumentNode.Descendants().ToList();
            var toReplace = docDesc[1200];
            var toReplacePrevSibling = toReplace.PreviousSibling;
            var toReplaceNextSibling = toReplace.NextSibling;
            var newNode = doc.CreateElement("tr");
            toReplace.ParentNode!.ReplaceChild(newNode, toReplace);
            Assert.Same(toReplacePrevSibling, newNode.PreviousSibling);
            Assert.Same(toReplaceNextSibling, newNode.NextSibling);
        }

        [Fact]
        public async Task TestInsertUpdateSiblingsAsync()
        {
            var doc = await GetMshomeDocumentAsync();
            var newNode = doc.CreateElement("td");
            var toReplace = doc.DocumentNode.ChildNodes[2];
            var toReplacePrevSibling = toReplace.PreviousSibling;
            var toReplaceNextSibling = toReplace.NextSibling;
            doc.DocumentNode.ChildNodes.Insert(2, newNode);
            Assert.Same(newNode.NextSibling, toReplace);
            Assert.Same(newNode.PreviousSibling, toReplacePrevSibling);
            Assert.Same(toReplaceNextSibling, toReplace.NextSibling);
        }

        [Fact]
        public void TestCopyFromNode()
        {
            var html = @"<div attr='test'></div>";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var divNode = htmlDoc.DocumentNode.SelectSingleNode("//div");

            var newNode = HtmlAgilityPack.HtmlNode.CreateNode("<body></body>");
            Assert.NotNull(divNode);
            newNode.CopyFrom(divNode);

            var attribute1 = divNode.Attributes[0];

            var attribute2 = newNode.Attributes[0];

            Assert.Equal(divNode.Attributes.Count, newNode.Attributes.Count);
            Assert.Equal(attribute1.Value, attribute2.Value);
            Assert.Equal(attribute1.QuoteType, attribute2.QuoteType);
        }

        [Fact]
        public void TestCommentNode()
        {
            var html =
                @"<!DOCTYPE html>
<html>
<body>
<!--title='Title' >
<!--title='Title'--!>
<!--title = 'Title'-->
<!--title='Title'--!>
<h1>This is <b>bold</b> headddding</h1>
	<p>This is <u>underlinyed</u> paragraph</p>
	<h2>This is <i>italic</i> heading</h2>
</body>
</html> ";

            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(html);

            var h1 = htmlDoc.DocumentNode.SelectNodes("//h1");
            var comments = htmlDoc.DocumentNode.SelectNodes("//comment()");

            Assert.Single(h1!);
            Assert.Equal(4, comments!.Count);
        }

        [Fact]
        public void TestCloneNode()
        {
            var html = @"<div attr='test'></div>";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var divNode = htmlDoc.DocumentNode.SelectSingleNode("//div");

            var newNode = divNode!.Clone();

            var attribute1 = divNode.Attributes[0];

            var attribute2 = newNode.Attributes[0];

            Assert.Equal(divNode.Attributes.Count, newNode.Attributes.Count);
            Assert.Equal(attribute1.Value, attribute2.Value);
            Assert.Equal(attribute1.QuoteType, attribute2.QuoteType);
        }

        [Fact]
        public void TestEmptyTag_Single()
        {
            var html = "<img src=\"x\"/onerror=\"alert('onerror1')\"><img/src=\"x\"/onerror=\"alert('onerror2')\">";
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            Assert.Equal(@"<img src=""x"" onerror=""alert('onerror1')""><img src=""x"" onerror=""alert('onerror2')"">", doc.DocumentNode.OuterHtml);
        }

        [Fact]
        public void TestEmptyTag_Many()
        {
            {
                var html = "<img src=\"x\"//onerror=\"alert('onerror1')\"><img//src=\"x\"//onerror=\"alert('onerror2')\">";
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                Assert.Equal(@"<img src=""x"" onerror=""alert('onerror1')""><img src=""x"" onerror=""alert('onerror2')"">", doc.DocumentNode.OuterHtml);
            }

            {
                var html = "<img src=\"x\"//////onerror=\"alert('onerror1')\"><img//////////////src=\"x\"////////////////onerror=\"alert('onerror2')\">";
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                Assert.Equal(@"<img src=""x"" onerror=""alert('onerror1')""><img src=""x"" onerror=""alert('onerror2')"">", doc.DocumentNode.OuterHtml);
            }
        }

        [Fact]
        public void TestAddClass()
        {
            var html = @"<h1>This is new heading</h1>";

            string output = "<h1 class=\"input\">This is new heading</h1>";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var h1Node = htmlDoc.DocumentNode.SelectSingleNode("//h1");

            h1Node!.AddClass("input");

            Assert.Equal(h1Node.OuterHtml, output);
        }

        [Fact]
        public void TestRemoveClass()
        {
            var output = @"<h1>This is new heading</h1>";

            string html = "<h1 class=\"input\">This is new heading</h1>";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var h1Node = htmlDoc.DocumentNode.SelectSingleNode("//h1");

            h1Node!.RemoveClass("input");

            Assert.Equal(h1Node.OuterHtml, output);
        }

        [Fact]
        public void TestReplaceClass()
        {
            var output = "<h1 class=\"important\">This is new heading</h1>";

            string html = "<h1 class=\"input\">This is new heading</h1>";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var h1Node = htmlDoc.DocumentNode.SelectSingleNode("//h1");

            h1Node!.ReplaceClass("important", "input");

            Assert.Equal(h1Node.OuterHtml, output);
        }

        [Fact]

        public async Task TestDetectEncodingAsync()
        {
            string html = Path.Combine(_contentDirectory, "test.html");

            var htmlDoc = new HtmlDocument();

            var encoding = await htmlDoc.DetectEncodingAsync(html);

            Assert.Equal(System.Text.Encoding.UTF8, encoding);
        }

        [Fact]
        public void OuterHtmlHasBeenCalled_RemoveCalled_SubsequentOuterHtmlCallsAreBroken()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml("<html><head></head><body><div>SOme text here</div><div>some bolded<b>text</b></div></body></html>");
            var resultList = doc.DocumentNode.SelectNodes("//div");
            Assert.Equal(2, resultList!.Count);
            resultList.First().Remove();
            Assert.Equal("<html><head></head><body><div>some bolded<b>text</b></div></body></html>", doc.DocumentNode.OuterHtml);
            var resultList2 = doc.DocumentNode.SelectNodes("//div");
            Assert.NotNull(resultList2);
            Assert.Single(resultList2);
            resultList2.First().Remove();
            // <div>some bolded<b>text</b></div> should have been removed
            Assert.Equal("<html><head></head><body></body></html>", doc.DocumentNode.OuterHtml);
        }

        [Fact]
        public void TestRemoveAttribute()
        {
            var output = @"<h1>This is new heading</h1>";

            string html = "<h1 a=\"foo\" b=\"bar\" A=\"baz\">This is new heading</h1>";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var h1Node = htmlDoc.DocumentNode.SelectSingleNode("//h1");

            h1Node!.Attributes.Remove("a");
            h1Node.Attributes.Remove("b");

            Assert.Equal(h1Node.OuterHtml, output);
        }

        [Fact]
        public void TestAttributeDeEntitizeValue()
        {
            var html =
                "<!DOCTYPE html><html><body data-foo=&quot;Hello&quot;><p>This is <u>underlined</u> paragraph</p></body></html>";

            string output = "\"Hello\"";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var body = htmlDoc.DocumentNode.SelectSingleNode("//body");

            var val = body!.Attributes["data-foo"]!.DeEntitizeValue;

            Assert.Equal(output, val);
        }

        [Fact]
        public void TestAttributeDeEntitizeValue2()
        {
            var html =
                "<!DOCTYPE html><html><body data-foo=&quot;\"Hello\"&quot;><p>This is <u>underlined</u> paragraph</p></body></html>";

            string output = "\"\"Hello\"\"";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var body = htmlDoc.DocumentNode.SelectSingleNode("//body");

            var val = body!.Attributes["data-foo"]!.DeEntitizeValue;

            Assert.Equal(output, val);
        }

        [Fact]
        public void TestAttributeDeEntitizeValue3()
        {
            var doc = new HtmlDocument();
            var a = doc.CreateAttribute("href", "\"bad_href\"");
            Assert.Equal("href", a.Name);
            Assert.Equal("\"bad_href\"", a.DeEntitizeValue);
        }

        [Fact]
        public void TestAttributeDeEntitizeValue4()
        {
            var html = "<!DOCTYPE html><html><body data-foo='\"Hello\"'><p>This is <u>underlined</u> paragraph</p></body></html>";

            string output = "\"Hello\"";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var body = htmlDoc.DocumentNode.SelectSingleNode("//body");

            var val = body!.Attributes["data-foo"]!.DeEntitizeValue;

            Assert.Equal(output, val);
        }

        [Fact]
        public void TestAttributeQuote1()
        {
	        var html = @"
<html>
	<body>
		<div id='foo'  myprop=""@GetMyProp(""myKey"")"">text</div> 
	</body>
</html>
";

            var doc = new HtmlDocument();
	        doc.LoadHtml(html);
	        var node = doc.DocumentNode.SelectNodes("//div");
	        var attr = node![0].Attributes["myprop"]!; 
	        attr.Value = "@GetMyProp(\"myKey2\")"; 
	        var t = doc.DocumentNode.OuterHtml;
            Assert.DoesNotContain("&quot", t);
        }

        [Fact]
        public void TestAttributeQuote2()
        {
	        var html = @"
<html>
	<body> 
		<div id='foo2'  myprop2=""(""myKey"")"">text</div>
	</body>
</html>
";
            var doc = new HtmlDocument();
	        doc.LoadHtml(html);
	        var node = doc.DocumentNode.SelectNodes("//div"); 
	        var attr = node![0].Attributes["myprop2"]!;
	        attr.Value = "(\"myKey\")"; 
	        var t = doc.DocumentNode.OuterHtml;
	        Assert.Contains("&quot", t);
        }


        [Fact]
        public void TestAttributeValue()
        {
            var html = @"<!DOCTYPE html><html><body data-foo='http://example.com/path?productId=9788762505032&amp;title=something'><p>This is <u>underlined</u> paragraph</p></body></html>";

            string output = "http://example.com/path?productId=9788762505032&amp;title=something";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var body = htmlDoc.DocumentNode.SelectSingleNode("//body");

            var val = body!.Attributes["data-foo"]!.Value;

            Assert.Equal(output, val);
        }

        [Fact]

        public void TestSingleNodesEmptyCollection()
        {
            var html =
                @"<!DOCTYPE html>
<html>
<body>
	<h1>This is <b>bold</b> heading</h1>
	<p>This is <u>underlined</u> paragraph</p>
	<h2>This is <i>italic</i> heading</h2>
	<h2>This is new heading</h2>
</body>
</html> ";

            var output = 0;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            htmlDoc.OptionEmptyCollection = true;

            var divNodes = htmlDoc.DocumentNode.SelectNodes("//div");

            Assert.Equal(output, divNodes!.Count);
        }

        [Fact]
        public void TestCreateNode()
        {
            string output = "<p>text</p>";
            HtmlNode node1 = HtmlNode.CreateNode(@"
            <p>text</p>
            ");

            HtmlNode node2 = HtmlNode.CreateNode(@"
                         
            <p>text</p>            

            ");

            HtmlNode node3 = HtmlNode.CreateNode(@"   <p>text</p>   ");

            Assert.Equal(output, node1.OuterHtml);
            Assert.Equal(output, node2.OuterHtml);
            Assert.Equal(output, node3.OuterHtml);

            try
            {
                HtmlNode node4 = HtmlNode.CreateNode("<p>a<p/><p>b</p>");
            }
            catch (Exception e)
            {
                Assert.Equal("Multiple node elements can't be created.", e.Message);
            }

            HtmlNode node5 = HtmlNode.CreateNode(@"/r/n");
            Assert.Equal("/r/n", node5.OuterHtml);
        }

        [Fact]
        public async Task TestParseListInListAsync()
        {
            var html = "<ol><li><ol><li>x</li></ol></li></ol>";

            var doc = new HtmlDocument();
            doc.OptionFixNestedTags = true;
            await doc.LoadAsync(new StringReader(html));

            Assert.Equal(doc.DocumentNode.OuterHtml, html);
        }

        

        [Fact]
        public void TestFormTag()
        {
            var html = @"<form></form>";
            var document = new HtmlDocument();
            document.LoadHtml(html);
            var result = document.DocumentNode.Descendants().Select(dn => new {dn.NodeType, dn.Name, dn.OuterHtml}).ToArray();
            Assert.Equal(html, document.DocumentNode.OuterHtml);
            Assert.Single(result);
        }

        [Fact]
        public void TestNumericTag()
        {
            var html = @"<div><1</div>";
            var document = new HtmlDocument();
            document.LoadHtml(html);
            var result = document.DocumentNode.Descendants().Select(dn => new {dn.NodeType, dn.Name, dn.OuterHtml}).ToArray();
            Assert.Equal(html, document.DocumentNode.OuterHtml);
        }

        [Fact]
        public void ImplicitTag()
        {
            {
                var html = @"<dt>a<dd>b<dd>c<dt>a<dd>b<dd>c";
                var document = new HtmlDocument();
                document.LoadHtml(html);
                var result = document.DocumentNode.Descendants().Select(dn => new {dn.NodeType, dn.Name, dn.OuterHtml}).ToArray();

                // TODO: Fix issue with last "dd"
                Assert.Equal(html + "</dd>", document.DocumentNode.OuterHtml);
            }


            {
                var html = @"<dt>a</dt><dd>b</dd><dd>c</dd><dt>a</dt><dd>b</dd><dd>c</dd>";
                var document = new HtmlDocument();
                document.LoadHtml(html);
                var result = document.DocumentNode.Descendants().Select(dn => new {dn.NodeType, dn.Name, dn.OuterHtml}).ToArray();

                Assert.Equal(html, document.DocumentNode.OuterHtml);
            }
        }

        [Fact]
        public void DeEntitize()
        {

            Assert.Equal("mouse's house", HtmlEntity.DeEntitize("mouse&apos;s house"));
        }

        [Fact]
        public void InnerText_Comment()
        {
            var document = new HtmlDocument();
            document.LoadHtml("<p><!-- comment 1 -->Expected <!-- comment 2 -->value</p>");

            Assert.Equal("Expected value", document.DocumentNode.FirstChild!.InnerText);
        }

        [Fact]
        public void TestFixNestedAnchors()
        {
            var inHtml = "<html><body><a href='...'> Here's a great link! <a href='...'>Here's another one!</a>Here's some unrelated text.</body></html>";
            var expectedHtml = "<html><body><a href='...'> Here's a great link! <a href='...'>Here's another one!</a>Here's some unrelated text.</body></html>";

            var doc = new HtmlDocument();
            doc.LoadHtml(inHtml);

            Assert.Equal(expectedHtml, doc.DocumentNode.OuterHtml);
        }

        [Fact]
        public void TestHandleNestedAnchors()
        {
            var inHtml = "<a><a>";
            var expectedHtml = "<a><a></a>";
            var doc = new HtmlDocument();
            doc.LoadHtml(inHtml);
            Assert.Equal(expectedHtml, doc.DocumentNode.OuterHtml);
        }

        [Fact]
        public void TestInnerText()
        {
            var inHtml = @"<html>
	<head>
		<title>
			InnerText bug Demo
		</title>
	</head>
	<body>
		<div>
			This demonstration should show that the HAP currently parses div tags incorrectly, parsing carriage returns, new lines and tabular indents as text.
		</div>
	</body>
</html>";
            var expectedHtml = "InnerText bug DemoThis demonstration should show that the HAP currently parses div tags incorrectly, parsing carriage returns, new lines and tabular indents as text.";

            var doc = new HtmlDocument() {BackwardCompatibility = false};
            doc.LoadHtml(inHtml);

            Assert.Equal(expectedHtml, doc.DocumentNode.InnerText);
        }

        [Fact]
        public void TestOptionTag()
        {
            var html = "<select><option>Select a cell</option><option>C1</option><option value='\"c2\"'></select>";

            string output = "<select><option>Select a cell</option><option>C1</option><option value='\"c2\"'></option></select>";
            var document = new HtmlDocument();
            document.LoadHtml(html);
            Assert.Equal(output, document.DocumentNode.OuterHtml);
        }

        [Fact]
        public void VerifyChildDivParent()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml("<html><body></body></html>");

            var div = HtmlNode.CreateNode("<div class='1'/>");
            var div2 = HtmlNode.CreateNode("<div class='2'/>");

            doc.DocumentNode.ChildNodes.Add(div);
            div.ChildNodes.Add(div2);

            Assert.Equal(div!.Name, div2!.ParentNode!.Name);

        }


        [Fact]
        public void ChildIsRemovedFromParent()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml("<html><body></body></html>");

            var div = HtmlNode.CreateNode("<div class='1'/>");
            var div2 = HtmlNode.CreateNode("<div class='2'/>");

            div.ChildNodes.Add(div2);
            doc.DocumentNode.ChildNodes.Add(div);


            div!.FirstChild!.Remove();

            Assert.Empty(div.ChildNodes);

        }

        [Fact]
        public async Task CompareLowerCultureAsync()
        {
            string html = await File.ReadAllTextAsync(_contentDirectory + "regression.html");
            HtmlNode? node1 = null;
            // Test 1
            CultureInfo cul1 = CultureInfo.CreateSpecificCulture("en-US");
            Thread.CurrentThread.CurrentCulture = cul1;
            HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
            doc1.LoadHtml(html);

            node1 = doc1.DocumentNode.SelectSingleNode("//div[@id='mainContents']/h2");

            CultureInfo previousCulture = CultureInfo.CurrentCulture;
            try
            {
            CultureInfo cul2 = CultureInfo.CreateSpecificCulture("tr-TR");
            CultureInfo.CurrentCulture = cul2;
            HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
            doc2.LoadHtml(html);
            var s = doc2.DocumentNode.OuterHtml;

            HtmlNode? node2 = doc2.DocumentNode.SelectSingleNode("//div[@id='mainContents']/h2");
            if (node1?.InnerHtml == node2?.InnerHtml)

                 
            Assert.Equal(node1?.InnerHtml, node2?.InnerHtml);
            Assert.Empty(doc2.DocumentNode.OwnerDocument.ParseErrors);
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }
        }


        [Fact]
        public async Task OverFlowNotEndTagAsync()
        {
            string html = await File.ReadAllTextAsync(_contentDirectory + "overflow.html");
            // Test 1
             
            HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
            doc1.LoadHtml(html);

            Assert.Equal(15, doc1.DocumentNode.ChildNodes[4].ChildNodes.Count);

            Assert.Empty(doc1.DocumentNode.OwnerDocument.ParseErrors);
        }

        [Fact]
        public void SanitizeXmlElementNameWithColon()
        {
            var input = @"<RootElement xmlns:MyNamespace=""value"">
  <value:element />
</RootElement>";
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(input);
            htmlDoc.OptionDefaultStreamEncoding = System.Text.Encoding.UTF8;
            htmlDoc.OptionOutputAsXml = true;
            htmlDoc.OptionOutputOriginalCase = true;
            var xmlDoc = htmlDoc.DocumentNode.WriteTo();

            var expected = @"<?xml version=""1.0"" encoding=""utf-8""?>" +
                           @"<RootElement xmlns:MyNamespace=""value"">
  <_value3a_element></_value3a_element>
</RootElement>";

            Assert.Equal(expected, xmlDoc);
        }

        [Fact]
        public void HasClass_WhereClassWithWhitespacePassed_ShouldReturnTrue()
        {
            var input = @"<a class="" disabled""></a>";
            var htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(input);

            var aTag = htmlDoc.DocumentNode.SelectSingleNode("//a");
            Assert.True(aTag!.HasClass("disabled"));
        }

        [Fact]
        public void GetClasses_WhereClassWithWhitespacePassed_ShouldNotBeEmpty()
        {
            var input = @"<a class="" disabled""></a>";
            var htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(input);

            var aTag = htmlDoc.DocumentNode.SelectSingleNode("//a");
            Assert.NotEmpty(aTag!.GetClasses());
        }

        [Fact]
        public void DoesNotSanitizeXmlElementNameWithColonWhenConfiguredToPreserveXmlNamespaces()
        {
            var input = @"<RootElement xmlns:MyNamespace=""value"">
  <value:element />
</RootElement>";
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(input);
            htmlDoc.OptionDefaultStreamEncoding = System.Text.Encoding.UTF8;
            htmlDoc.OptionOutputAsXml = true;
            htmlDoc.OptionOutputOriginalCase = true;
            htmlDoc.OptionPreserveXmlNamespaces = true;
            var xmlDoc = htmlDoc.DocumentNode.WriteTo();

            var expected = @"<?xml version=""1.0"" encoding=""utf-8""?>" +
                           @"<RootElement xmlns:MyNamespace=""value"">
  <value:element></value:element>
</RootElement>";

            Assert.Equal(expected, xmlDoc);
        }

        [Fact]
        public void ChangesToPHandling()
        {
            var input = "<p>Begin<div>Inner</div>End</p>";
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(input);
            var docNode = doc.DocumentNode;

            // Check tree is correct
            
            Assert.Equal(3, docNode.ChildNodes.Count);
            Assert.Equal("p", docNode.ChildNodes[0].Name);
            Assert.Equal("div", docNode.ChildNodes[1].Name);
            Assert.Equal("#text", docNode.ChildNodes[2].Name);
            Assert.Equal("Begin", docNode.ChildNodes[0].InnerText);
            Assert.Equal("Inner", docNode.ChildNodes[1].InnerText);
            Assert.Equal("End", docNode.ChildNodes[2].InnerText);
        }

        [Fact]
        public void ChangesToPHandlingFalse()
        {
            HtmlDocument.DisableBehaviorTagP = false;

            var input = "<p>Begin<div>Inner</div>End</p>";
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(input);
            var docNode = doc.DocumentNode;

            try
            {
                // Check tree is correct
                Assert.Single(docNode.ChildNodes);
                Assert.Equal("p", docNode.ChildNodes[0].Name);

                var pRootNode = docNode.ChildNodes[0];
                Assert.Equal(3, pRootNode.ChildNodes.Count);
                Assert.Equal("Begin", pRootNode.ChildNodes[0].InnerText);
                Assert.Equal("Inner", pRootNode.ChildNodes[1].InnerText);
                Assert.Equal("End", pRootNode.ChildNodes[2].InnerText);
            }
            finally
            {
                HtmlDocument.DisableBehaviorTagP = true;
            }
        }

        [Fact]
        public void AttributeValue()
        {
            {
                Assert.Equal("&quot;&#39;", WebUtility.HtmlEncode("\"'"));
            }
            {
                var input = "<div>z</div>";
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.BackwardCompatibility = true;
                doc.LoadHtml(input);
                var divNode = doc.DocumentNode.ChildNodes[0];

                divNode.Attributes.Add("name", "value1value2");
                Assert.Equal("value1value2", divNode.Attributes[0].Value);
                Assert.Equal("<div name=\"value1value2\">z</div>", doc.DocumentNode.InnerHtml);
            }

            {
                var input = "<div>z</div>";
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.BackwardCompatibility = true;
                doc.LoadHtml(input);
                var divNode = doc.DocumentNode.ChildNodes[0];

                divNode.Attributes.Add("name", "value1\"value2");
                Assert.Equal("value1\"value2", divNode.Attributes[0].Value);
                Assert.Equal("<div name=\"value1&quot;value2\">z</div>", doc.DocumentNode.InnerHtml);
            }

            {
                var input = "<div>z</div>";
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.BackwardCompatibility = true;
                doc.LoadHtml(input);
                var divNode = doc.DocumentNode.ChildNodes[0];

                divNode.Attributes.Add("name", "value1&quot;value2");
                Assert.Equal("value1&quot;value2", divNode.Attributes[0].Value);
                Assert.Equal("<div name=\"value1&quot;value2\">z</div>", doc.DocumentNode.InnerHtml);
            }

            {
                var input = "<div>z</div>";
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.BackwardCompatibility = true;
                doc.LoadHtml(input);
                var divNode = doc.DocumentNode.ChildNodes[0];

                divNode.Attributes.Add("name", "value1'value2");
                divNode.Attributes[0].QuoteType = AttributeValueQuote.SingleQuote;
                Assert.Equal("value1'value2", divNode.Attributes[0].Value);
                Assert.Equal("<div name='value1&#39;value2'>z</div>", doc.DocumentNode.InnerHtml);
            }

            {
                var input = "<div>z</div>";
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.BackwardCompatibility = true;
                doc.LoadHtml(input);
                var divNode = doc.DocumentNode.ChildNodes[0];

                divNode.Attributes.Add("name", "value1&#39;value2");
                divNode.Attributes[0].QuoteType = AttributeValueQuote.SingleQuote;
                Assert.Equal("value1&#39;value2", divNode.Attributes[0].Value);
                Assert.Equal("<div name='value1&#39;value2'>z</div>", doc.DocumentNode.InnerHtml);
            }

            {
                var input = "<div>z</div>";
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.BackwardCompatibility = false;
                doc.LoadHtml(input);
                var divNode = doc.DocumentNode.ChildNodes[0];

                divNode.Attributes.Add("name", "value1value2");
                Assert.Equal("value1value2", divNode.Attributes[0].Value);
                Assert.Equal("<div name=\"value1value2\">z</div>", doc.DocumentNode.InnerHtml);
            }

            {
                var input = "<div>z</div>";
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.BackwardCompatibility = false;
                doc.LoadHtml(input);
                var divNode = doc.DocumentNode.ChildNodes[0];

                divNode.Attributes.Add("name", "value1\"value2");
                Assert.Equal("value1\"value2", divNode.Attributes[0].Value);
                Assert.Equal("<div name=\"value1&quot;value2\">z</div>", doc.DocumentNode.InnerHtml);
            }

            {
                var input = "<div>z</div>";
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.BackwardCompatibility = false;
                doc.LoadHtml(input);
                var divNode = doc.DocumentNode.ChildNodes[0];

                divNode.Attributes.Add("name", "value1&quot;value2");
                Assert.Equal("value1&quot;value2", divNode.Attributes[0].Value);
                Assert.Equal("<div name=\"value1&quot;value2\">z</div>", doc.DocumentNode.InnerHtml);
            }

            {
                var input = "<div>z</div>";
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.BackwardCompatibility = false;
                doc.LoadHtml(input);
                var divNode = doc.DocumentNode.ChildNodes[0];

                divNode.Attributes.Add("name", "value1'value2");
                divNode.Attributes[0].QuoteType = AttributeValueQuote.SingleQuote;
                Assert.Equal("value1'value2", divNode.Attributes[0].Value);
                Assert.Equal("<div name='value1&#39;value2'>z</div>", doc.DocumentNode.InnerHtml);
            }

            {
                var input = "<div>z</div>";
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.BackwardCompatibility = false;
                doc.LoadHtml(input);
                var divNode = doc.DocumentNode.ChildNodes[0];

                divNode.Attributes.Add("name", "value1&#39;value2");
                divNode.Attributes[0].QuoteType = AttributeValueQuote.SingleQuote;
                Assert.Equal("value1&#39;value2", divNode.Attributes[0].Value);
                Assert.Equal("<div name='value1&#39;value2'>z</div>", doc.DocumentNode.InnerHtml);
            }

        }

        [Fact]
        public async Task AttributeSerializationAsync()
        {
            HtmlDocument doc1 = new HtmlDocument()
            {
                OptionEmptyCollection = true,
                OptionAutoCloseOnEnd = true,
                OptionWriteEmptyNodes = true,
                // Disable backward compatibility to enable automatic html decoding
                BackwardCompatibility = false
            };

            HtmlDocument doc2 = new HtmlDocument()
            {
                OptionEmptyCollection = true,
                OptionAutoCloseOnEnd = true,
                OptionWriteEmptyNodes = true,
                // Disable backward compatibility to enable automatic html decoding
                BackwardCompatibility = false
            };



            string html = @"
<html>
<body>
Hello Button: <input type=""button"" value=""ClickMe"" onClick=""var a=&gt;&quot;&#39;&gt;&lt;img src=&#x6a;&#x61;&#x76;&#x61;&#x73;&#x63;&#x72;&#x69;&#x70;&#x74;&#x3a;alert(29)&gt; alert('');"">
</body>
</html>";
            doc1.LoadHtml(html);
            int beforeSerializtionSrcElementsCount = doc1.DocumentNode.SelectNodes("//* [@src]")!.Count;
            StringWriter stringWriter = new StringWriter();
            await doc1.SaveAsync(stringWriter);
            String htmlAfterSerialization = stringWriter.ToString();
            doc2.LoadHtml(htmlAfterSerialization);
            Assert.Equal(beforeSerializtionSrcElementsCount, doc2.DocumentNode.SelectNodes("//* [@src]")!.Count);

        }

        [Fact]
        public async Task LoadHtml_WhenHtmlHasUnclosedTags_AndOptionCheckSyntaxFalse_ShouldNotThrowExceptionAsync()
        {
            var html = "<div>Some simple <p> html</div>";
            var htmlDoc = new HtmlDocument { OptionCheckSyntax = false };

            htmlDoc.LoadHtml(html);

            using (var memoryStream = new MemoryStream())
            {
                await htmlDoc.SaveAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var streamReader = new StreamReader(memoryStream))
                {
                    string parsedHtml = await streamReader.ReadToEndAsync();
                    Assert.NotEmpty(parsedHtml);
                }
            }
            Assert.Empty(htmlDoc.ParseErrors);
        }

        [Theory]
        [InlineData("<table><caption><colgroup></colgroup></table>", "<table><caption></caption><colgroup></colgroup></table>")]
        [InlineData("<table><caption><thead></thead></table>", "<table><caption></caption><thead></thead></table>")]
        [InlineData("<table><caption><tbody></tbody></table>", "<table><caption></caption><tbody></tbody></table>")]
        [InlineData("<table><caption><tfoot></tfoot></table>", "<table><caption></caption><tfoot></tfoot></table>")]
        [InlineData("<table><caption><tr></tr></table>", "<table><caption></caption><tr></tr></table>")]

        public void CaptionTagShouldBeClosedIfClosedTagIsMissing(string inputHtml, string expectedHtml)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(inputHtml);

            Assert.Equal(expectedHtml, doc.DocumentNode.OuterHtml);
        }

        [Theory]
        [InlineData("<table><colgroup><thead></thead></table>", "<table><colgroup></colgroup><thead></thead></table>")]
        [InlineData("<table><colgroup><tbody></tbody></table>", "<table><colgroup></colgroup><tbody></tbody></table>")]
        [InlineData("<table><colgroup><tfoot></tfoot></table>", "<table><colgroup></colgroup><tfoot></tfoot></table>")]
        [InlineData("<table><colgroup><tr></tr></table>", "<table><colgroup></colgroup><tr></tr></table>")]

        public void ColgroupTagShouldBeClosedIfClosedTagIsMissing(string inputHtml, string expectedHtml)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(inputHtml);

            Assert.Equal(expectedHtml, doc.DocumentNode.OuterHtml);
        }

    }
}
