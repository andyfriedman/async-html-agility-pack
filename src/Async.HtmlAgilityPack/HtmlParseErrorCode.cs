// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace Async.HtmlAgilityPack;

/// <summary>
/// Represents the type of parsing error.
/// </summary>
public enum HtmlParseErrorCode
{
    /// <summary>
    /// A tag was not closed.
    /// </summary>
    TagNotClosed,

    /// <summary>
    /// A tag was not opened.
    /// </summary>
    TagNotOpened,

    /// <summary>
    /// There is a charset mismatch between stream and declared (META) encoding.
    /// </summary>
    CharsetMismatch,

    /// <summary>
    /// An end tag was not required.
    /// </summary>
    EndTagNotRequired,

    /// <summary>
    /// An end tag is invalid at this position.
    /// </summary>
    EndTagInvalidHere
}
