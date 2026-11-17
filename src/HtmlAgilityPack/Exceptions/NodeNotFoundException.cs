// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace HtmlAgilityPack.Exceptions;

/// <summary>
/// Exception that often occures when there is no way to bind a XPath to a Html Tag.
/// </summary>
public class NodeNotFoundException : Exception
{
    /// <summary>
    ///
    /// </summary>
    public NodeNotFoundException() { }

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    public NodeNotFoundException(string message) : base(message) { }

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public NodeNotFoundException(string message, Exception inner) : base(message, inner) { }
}
