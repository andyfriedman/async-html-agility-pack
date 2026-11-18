// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace Async.HtmlAgilityPack;

/// <summary>
/// Represents an HTML comment.
/// </summary>
public class HtmlCommentNode : HtmlNode
{
    private string? _comment;

    internal HtmlCommentNode(HtmlDocument ownerdocument, int index)
        : base(HtmlNodeType.Comment, ownerdocument, index)
    {
    }

    /// <summary>
    /// Gets or Sets the comment text of the node.
    /// </summary>
    public string Comment
    {
        get => _comment ?? base.InnerHtml;
        set => _comment = value;
    }

    /// <summary>
    /// Gets or Sets the HTML between the start and end tags of the object. In the case of a text node, it is equals to OuterHtml.
    /// </summary>
    public override string InnerHtml
    {
        get => _comment ?? base.InnerHtml;
        set => _comment = value;
    }

    /// <summary>
    /// Gets or Sets the object and its content in HTML.
    /// </summary>
    public override string OuterHtml
    {
        get
        {
            if (_comment is null)
            {
                return base.OuterHtml;
            }

            if (_comment.StartsWith("<!--", StringComparison.Ordinal) && _comment.EndsWith("-->", StringComparison.Ordinal))
            {
                return _comment;
            }

            return $"<!--{_comment}-->";
        }
    }
}
