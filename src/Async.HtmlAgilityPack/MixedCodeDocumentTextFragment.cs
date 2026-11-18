// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace Async.HtmlAgilityPack;

/// <summary>
/// Represents a fragment of text in a mixed code document.
/// </summary>
public class MixedCodeDocumentTextFragment : MixedCodeDocumentFragment
{
    internal MixedCodeDocumentTextFragment(MixedCodeDocument doc)
        : base(doc, MixedCodeDocumentFragmentType.Text)
    {
    }

    /// <summary>
    /// Gets or sets the fragment text.
    /// </summary>
    public string Text
    {
        get => FragmentText;
        set => FragmentText = value;
    }
}
