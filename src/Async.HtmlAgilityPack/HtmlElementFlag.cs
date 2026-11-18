// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace HtmlAgilityPack;

/// <summary>
/// Flags that describe the behavior of an Element node.
/// </summary>
[Flags]
public enum HtmlElementFlag
{
    /// <summary>
    /// The node is a CDATA node.
    /// </summary>
    CData = 1,

    /// <summary>
    /// The node is empty. META or IMG are example of such nodes.
    /// </summary>
    Empty = 2,

    /// <summary>
    /// The node will automatically be closed during parsing.
    /// </summary>
    Closed = 4,

    /// <summary>
    /// The node can overlap.
    /// </summary>
    CanOverlap = 8
}
