// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

using System.Xml.XPath;

namespace HtmlAgilityPack;

public partial class HtmlDocument : IXPathNavigable
{
    /// <summary>
    /// Creates a new XPathNavigator object for navigating this HTML document.
    /// </summary>
    /// <returns>An XPathNavigator object. The XPathNavigator is positioned on the root of the document.</returns>
    public XPathNavigator CreateNavigator() => new HtmlNodeNavigator(this, _documentnode);
}
