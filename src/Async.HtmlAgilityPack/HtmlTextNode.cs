// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace Async.HtmlAgilityPack;

/// <summary>
/// Represents an HTML text node.
/// </summary>
public class HtmlTextNode : HtmlNode
{
    private string? _text;

    internal HtmlTextNode(HtmlDocument ownerdocument, int index)
        : base(HtmlNodeType.Text, ownerdocument, index)
    {
    }

    /// <summary>
    /// Gets or Sets the HTML between the start and end tags of the object. In the case of a text node, it is equals to OuterHtml.
    /// </summary>
    public override string InnerHtml
    {
        get => OuterHtml;
        set => _text = value;
    }

    /// <summary>
    /// Gets or Sets the object and its content in HTML.
    /// </summary>
    public override string OuterHtml => _text ?? base.OuterHtml;

    /// <summary>
    /// Gets or Sets the text of the node.
    /// </summary>
    public string Text
    {
        get => _text ?? base.OuterHtml;
        set
        {
            _text = value;
            SetChanged();
        }
    }
}
