using Xunit;

namespace Async.HtmlAgilityPack.Tests;

public class HtmlNodeTests
{
    [Fact]
    public void ScriptingText()
    {
        var html = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\r\n" +
                   "<html xmlns=\"http://www.w3.org/1999/xhtml\">\r\n" +
                   "<head>\r\n" +
                   "    <title>SEE title</title>\r\n" +
                   "\t<script>SEE script </script>\r\n" +
                   "\t<style>SEE style</style>\r\n" +
                   "</head>\r\n" +
                   "<body>\r\n" +
                   "<script>NOTSEE script</script>\r\n" +
                   "<div>222<script>NOTSEE script</script>\r\n" +
                   "<style>NOTSEE style</style></div>\r\n" +
                   "</body>\r\n" +
                   "</html>";

        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var content1 = htmlDocument.DocumentNode.SelectSingleNode("//head")!.InnerText;
            var content2 = htmlDocument.DocumentNode.SelectSingleNode("//script")!.InnerText;
            var content3 = htmlDocument.DocumentNode.SelectSingleNode("//style")!.InnerText;
            var content4 = htmlDocument.DocumentNode.SelectSingleNode("//body")!.InnerText;
            var content5 = htmlDocument.DocumentNode.SelectSingleNode("//html")!.InnerText;
            var content6 = htmlDocument.DocumentNode.SelectSingleNode("//body/script")!.InnerText;

            Assert.Equal("\r\n    SEE title\r\n\tSEE script \r\n\tSEE style\r\n", content1);
            Assert.Equal("SEE script ", content2);
            Assert.Equal("SEE style", content3);
            Assert.Equal("\r\n\r\n222\r\n\r\n", content4);
            Assert.Equal("\r\n\r\n    SEE title\r\n\t\r\n\t\r\n\r\n\r\n\r\n222\r\n\r\n\r\n", content5);
            Assert.Equal("NOTSEE script", content6);
        }

        {
            var htmlDocument = new HtmlDocument
            {
                BackwardCompatibility = false
            };
            htmlDocument.LoadHtml(html);

            var content1 = htmlDocument.DocumentNode.SelectSingleNode("//head")!.InnerText;
            var content2 = htmlDocument.DocumentNode.SelectSingleNode("//script")!.InnerText;
            var content3 = htmlDocument.DocumentNode.SelectSingleNode("//style")!.InnerText;
            var content4 = htmlDocument.DocumentNode.SelectSingleNode("//body")!.InnerText;
            var content5 = htmlDocument.DocumentNode.SelectSingleNode("//html")!.InnerText;
            var content6 = htmlDocument.DocumentNode.SelectSingleNode("//body/script")!.InnerText;

            Assert.Equal("    SEE titleSEE script SEE style", content1);
            Assert.Equal("SEE script ", content2);
            Assert.Equal("SEE style", content3);
            Assert.Equal("222", content4);
            Assert.Equal("    SEE title222", content5);
            Assert.Equal("NOTSEE script", content6);
        }
    }

    [Fact]
    public void ReadNotCloseTag()
    {
        var document = new HtmlDocument();
        document.LoadHtml("<ul><li>item<span></li></ul>");
        var span = document.DocumentNode.SelectSingleNode("//span");
        Assert.NotNull(span);

        Assert.NotNull(span.OuterHtml);
        Assert.NotNull(span.InnerHtml);
        Assert.NotNull(span.InnerText);
    }

    [Fact]
    public void CheckAttributeForTextComment()
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(@"<html><body><div id='foo'><span> some</span> text</div></body></html>");
        var div = doc.GetElementbyId("foo");
        Assert.NotNull(div);

        int count = 0;
        Exception? exception = null;
        foreach (var textNode in div.ChildNodes)
        {
            try
            {
                textNode.Id = "1";
                count++;
            }
            catch (Exception e)
            {
                exception = e;
            }
        }

        Assert.Equal(1, count);
        Assert.NotNull(exception);
    }

    [Fact]
    public void Prepend_CheckOrder()
    {
        HtmlNode source = HtmlNode.CreateNode(@"
<ul>
  <li> Alpha   </li>
  <li> Bravo   </li>
  <li> Charlie </li>
</ul>
");
        HtmlNode destination = HtmlNode.CreateNode(@"
<ul>
  <li> Delta   </li>
  <li> Echo    </li>
  <li> Foxtrot </li>
</ul>
");

        destination.PrependChildren(source.ChildNodes);

        Assert.Equal(@"<ul>
  <li> Alpha   </li>
  <li> Bravo   </li>
  <li> Charlie </li>

  <li> Delta   </li>
  <li> Echo    </li>
  <li> Foxtrot </li>
</ul>", destination.WriteTo());
    }

    [Fact]
    public void OptionMaxNestedChildNodes_NotSet_IsNotEnforced()
    {
        var html = "<html><body><div>1<div>2</div>3</div></body></html>";
        var doc = new HtmlDocument();

        doc.LoadHtml(html);

        Assert.NotNull(doc);
        Assert.Equal(html, doc.Text);
    }

    [Fact]
    public void OptionMaxNestedChildNodes_SetToZero_IsNotEnforced()
    {
        var html = "<html><body><div>1<div>2</div>3</div></body></html>";
        var doc = new HtmlDocument
        {
            OptionMaxNestedChildNodes = 0
        };

        doc.LoadHtml(html);

        Assert.NotNull(doc);
        Assert.Equal(html, doc.Text);
    }

    [Fact]
    public void OptionMaxNestedChildNodes_WithinMax_NoException()
    {
        var html = "<div id='1'><div id='2'><div id='3'><div id='4'><div id='5'><div id='6'><div id='7'><div id='8'></div></div></div></div></div></div></div></div>";
        var doc = new HtmlDocument
        {
            OptionMaxNestedChildNodes = 8
        };

        doc.LoadHtml(html);
    }

    [Fact]
    public void OptionMaxNestedChildNodes_AboveMax()
    {
        var html = "<div id='1'><div id='2'><div id='3'><div id='4'><div id='5'><div id='6'><div id='7'><div id='8'></div></div></div></div></div></div></div></div>";
        var doc = new HtmlDocument
        {
            OptionMaxNestedChildNodes = 7
        };

        var ex = Assert.Throws<Exception>(() => doc.LoadHtml(html));
        Assert.Equal("Document has more than 7 nested tags. This is likely due to the page not closing tags properly.", ex.Message);
    }
}
