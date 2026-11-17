// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace HtmlAgilityPack.Exceptions;

/// <summary>
/// Exception that often occures when there is no way to bind a XPath to a HtmlTag Attribute.
/// </summary>
public class NodeAttributeNotFoundException : Exception
{
    /// <summary>
    ///
    /// </summary>
    public NodeAttributeNotFoundException() { }

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    public NodeAttributeNotFoundException(string message) : base(message) { }

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public NodeAttributeNotFoundException(string message, Exception inner) : base(message, inner) { }
}
