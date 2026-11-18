// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace Async.HtmlAgilityPack;

/// <summary>
/// Represents the type of a node.
/// </summary>
public enum HtmlNodeType
{
    /// <summary>
    /// The root of a document.
    /// </summary>
    Document,

    /// <summary>
    /// An HTML element.
    /// </summary>
    Element,

    /// <summary>
    /// An HTML comment.
    /// </summary>
    Comment,

    /// <summary>
    /// A text node is always the child of an element or a document node.
    /// </summary>
    Text,
}
