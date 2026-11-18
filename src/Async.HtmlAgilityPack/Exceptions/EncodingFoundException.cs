// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

using System.Text;

namespace Async.HtmlAgilityPack.Exceptions;

internal sealed class EncodingFoundException(Encoding? encoding) : Exception
{
    internal Encoding? Encoding { get; } = encoding;
}
