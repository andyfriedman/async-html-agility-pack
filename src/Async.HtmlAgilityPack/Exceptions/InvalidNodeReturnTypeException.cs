// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace Async.HtmlAgilityPack.Exceptions;

/// <summary>
/// Exception that occurs when an XPathAttribute annotation has an invalid ReturnType specified.
/// </summary>
public class InvalidNodeReturnTypeException : Exception
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    public InvalidNodeReturnTypeException(string message)
        : base(message) { }

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public InvalidNodeReturnTypeException(string message, Exception inner) : base(message, inner) { }
}
