// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace HtmlAgilityPack;

/// <summary>
/// Represents a fragment of code in a mixed code document.
/// </summary>
public class MixedCodeDocumentCodeFragment : MixedCodeDocumentFragment
{
    private string? _code;

    internal MixedCodeDocumentCodeFragment(MixedCodeDocument doc)
        : base(doc, MixedCodeDocumentFragmentType.Code)
    {
    }

    /// <summary>
    /// Gets or sets the fragment code text.
    /// </summary>
    public string Code
    {
        get
        {
            if (_code is null)
            {
                _code = FragmentText.Substring(
                    Doc.TokenCodeStart.Length,
                    FragmentText.Length - Doc.TokenCodeEnd.Length - Doc.TokenCodeStart.Length - 1).Trim();
                if (_code.StartsWith('='))
                {
                    _code = Doc.TokenResponseWrite + _code[1..];
                }
            }

            return _code;
        }
        set => _code = value;
    }
}
