// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace HtmlAgilityPack;

/// <summary>
/// Specify which part of <see cref="HtmlNode"/> is requested.
/// </summary>
public enum ReturnType
{
    /// <summary>
    /// The text between the start and end tags of the object.
    /// </summary>
    InnerText,

    /// <summary>
    /// The HTML between the start and end tags of the object
    /// </summary>
    InnerHtml,

    /// <summary>
    /// The object and its content in HTML
    /// </summary>
    OuterHtml
}
