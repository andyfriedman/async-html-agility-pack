using System.Xml.XPath;
using Xunit;

namespace Async.HtmlAgilityPack.Tests;

public class HtmlDocumentPreserveOriginalTest
{
    [Fact]
    public void PreserveEmptyAttributesTest()
    {
        var d = new HtmlDocument
        {
            OptionEmptyCollection = true,
            GlobalAttributeValueQuote = AttributeValueQuote.Initial,
            OptionDefaultUseOriginalName = true
        };

        var html = @"
<form #editForm='ngForm' (ngSubmit)='OnSaveData()' name='globalForm' [dataSourceContext]='detailDataSourceContext'>
  <page editable (beginEdit)='OnBeginEdit()' (cancelEdit)='OnCancelEdit($event)'>

  <page-header pageHeaderIcon='_Lea_ mdi mdi-code-json'>

    <page-header-actions>
      <list-counter formErrorsCounter></list-counter>

      <button mat-button *hasEveryPermission='permissionConstants.LeaProdutos.edit' enterEditButton>
      </button>

      <button mat-button *hasEveryPermission='permissionConstants.LeaProdutos.edit' cancelEditButton>
      </button>

      <button mat-button *hasEveryPermission='permissionConstants.LeaProdutos.edit' type='submit' confirmEditButton>
      </button>

      <refresh-button></refresh-button>
    </page-header-actions>

  </page-header>

    <page-body>
    </page-body>

  </page>
</form>
";
        d.LoadHtml(html);

        var xpath = XPathExpression.Compile("/form/page/page-header/page-header-actions/button[@enterEditButton]");
        var nodes = d.DocumentNode.SelectNodes(xpath);

        Assert.Equal(1, nodes?.Count);
        Assert.Equal(html, d.DocumentNode.OuterHtml);
    }

    [Fact]
    public void PreserveOriginalCasingTest()
    {
        var d = new HtmlDocument
        {
            OptionEmptyCollection = true,
            GlobalAttributeValueQuote = AttributeValueQuote.Initial,
            OptionDefaultUseOriginalName = true
        };

        var html = @"
<form #editForm='ngForm' (ngSubmit)='OnSaveData()' name='globalForm' [dataSourceContext]='detailDataSourceContext'>
  <page editable (beginEdit)='OnBeginEdit()' (cancelEdit)='OnCancelEdit($event)'>

  <page-Header pageHeaderIcon='_Lea_ mdi mdi-code-json'>

    <page-Header-Actions>
      <list-counter formErrorsCounter></list-counter>

      <button mat-button *hasEveryPermission='permissionConstants.LeaProdutos.edit' enterEditButton>
      </button>

      <button mat-button *hasEveryPermission='permissionConstants.LeaProdutos.edit' cancelEditButton>
      </button>

      <button mat-button *hasEveryPermission='permissionConstants.LeaProdutos.edit' type='submit' confirmEditButton>
      </button>

      <refresh-button></refresh-button>
    </page-Header-Actions>

  </page-Header>

    <page-body>
    </page-body>

  </page>
</form>
";
        d.LoadHtml(html);

        var xpath = XPathExpression.Compile("/form/page/page-Header/page-Header-Actions/button[@enterEditButton]");
        var nodes = d.DocumentNode.SelectNodes(xpath);

        Assert.Equal(1, nodes?.Count);
        Assert.Equal(html, d.DocumentNode.OuterHtml);
    }

    [Fact]
    public async Task PreserveOriginalQuoteTestAsync()
    {
        var contentDirectory = Path.Combine(AppContext.BaseDirectory, "files");

        var d = new HtmlDocument
        {
            OptionEmptyCollection = true,
            GlobalAttributeValueQuote = AttributeValueQuote.Initial,
            OptionDefaultUseOriginalName = true
        };

        var filePath = Path.Combine(contentDirectory, "attr_quote.html");
        var filePathExpected = Path.Combine(contentDirectory, "attr_quote_expected.html");
        await d.LoadAsync(filePath);

        var xpath = XPathExpression.Compile("/form/page/page-body/card/data-group/mat-form-field[@*[local-name() = 'xdt:Transform' and . = \"InsertAfter(mat-form-field[mat-select/@name = 'tipoProdutoId'])\"]]/mat-select/mat-option");
        var nodes = d.DocumentNode.SelectNodes(xpath);

        Assert.NotNull(nodes);
        Assert.Single(nodes);

        nodes[0].ParentNode!.Attributes["required"]!.Value = "true";

        Assert.Equal(await File.ReadAllTextAsync(filePathExpected), d.DocumentNode.OuterHtml);

        var clone = nodes[0].CloneNode(true);

        var expectedOuterNode = @"<mat-option *ngFor=""let dimension of lea546CATBEM$  | async | filterDimensionByValue:( simulationRequest.tipoProdutoId |  commonData:lea508SUBPROD$:'id':'code')"" [value]=""dimension.dimensionId"">


                                {{ dimension.code }} - {{ dimension.description }}
                            </mat-option>";

        Assert.Equal(expectedOuterNode, clone.OuterHtml);
    }

    [Fact]
    public void PreserveClonedEmptyAttributesTest()
    {
        var d = new HtmlDocument
        {
            GlobalAttributeValueQuote = AttributeValueQuote.Initial,
            OptionDefaultUseOriginalName = true
        };

        var html = @"<list-counter formErrorsCounter></list-counter>";
        d.LoadHtml(html);

        var cloned = d.DocumentNode.CloneNode(true);

        Assert.Equal(@"<list-counter formErrorsCounter></list-counter>", cloned.OuterHtml);
    }

    [Fact]
    public void PreserveEmptyAttributesWithInitialTest()
    {
        var d = new HtmlDocument { GlobalAttributeValueQuote = AttributeValueQuote.InitialExceptWithoutValue };
        d.LoadHtml("<bar ng-app class='message'></bar>");

        var node = d.DocumentNode.SelectSingleNode("//bar");
        Assert.NotNull(node);
        Assert.Equal("<bar ng-app=\"\" class='message'></bar>", node.OuterHtml);
    }

    [Fact]
    public void PreserveEmptyAttributes()
    {
        var d = new HtmlDocument { GlobalAttributeValueQuote = AttributeValueQuote.InitialExceptWithoutValue };
        d.LoadHtml("<li ng>Nothing to show</li>");

        var node = d.DocumentNode.SelectSingleNode("//li");
        Assert.NotNull(node);
        Assert.Equal("<li ng=\"\">Nothing to show</li>", node.OuterHtml);
    }

    [Fact]
    public void PreserveQuoteTypeForLoadedAttributes()
    {
        var input = HtmlNode.CreateNode("<input checked></input>");
        var checkedAttribute = input.Attributes.First();

        Assert.Equal("", checkedAttribute.Value);
        Assert.Equal(AttributeValueQuote.WithoutValue, checkedAttribute.QuoteType);
    }

    [Fact]
    public void PreserveQuoteTypeForLoadedAttributes2()
    {
        var d = new HtmlDocument { GlobalAttributeValueQuote = AttributeValueQuote.InitialExceptWithoutValue };
        d.LoadHtml(@"<bar ng-app ng-app2='message'></bar>");

        var node = d.DocumentNode.SelectSingleNode("//bar");
        Assert.NotNull(node);
        Assert.Equal("<bar ng-app=\"\" ng-app2='message'></bar>", node.OuterHtml);
    }
}
