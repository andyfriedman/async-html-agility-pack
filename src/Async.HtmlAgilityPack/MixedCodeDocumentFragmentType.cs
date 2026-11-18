// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace Async.HtmlAgilityPack;

/// <summary>
/// Represents the type of fragment in a mixed code document.
/// </summary>
public enum MixedCodeDocumentFragmentType
{
    /// <summary>
    /// The fragment contains code.
    /// </summary>
    Code,

    /// <summary>
    /// The fragment contains text.
    /// </summary>
    Text,
}
