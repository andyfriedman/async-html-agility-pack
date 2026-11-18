// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace Async.HtmlAgilityPack.Exceptions;

/// <summary>
/// Exception that often occures when there is no property that assigned with XPath Property in Class.
/// </summary>
public class MissingXPathException : Exception
{

    /// <summary>
    ///
    /// </summary>
    public MissingXPathException() { }

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    public MissingXPathException(string message) : base(message) { }

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public MissingXPathException(string message, Exception inner) : base(message, inner) { }
}
