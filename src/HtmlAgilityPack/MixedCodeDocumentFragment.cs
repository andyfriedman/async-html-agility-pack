// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace HtmlAgilityPack;

/// <summary>
/// Represents a base class for fragments in a mixed code document.
/// </summary>
public abstract class MixedCodeDocumentFragment
{
    internal MixedCodeDocument Doc;
    private string? _fragmentText;
    internal int Index;
    internal int Length;
    private int _line;
    internal int _lineposition;
    internal MixedCodeDocumentFragmentType _type;

    internal MixedCodeDocumentFragment(MixedCodeDocument doc, MixedCodeDocumentFragmentType type)
    {
        Doc = doc;
        _type = type;
        switch (type)
        {
            case MixedCodeDocumentFragmentType.Text:
                Doc._textfragments.Append(this);
                break;

            case MixedCodeDocumentFragmentType.Code:
                Doc._codefragments.Append(this);
                break;
        }

        Doc._fragments.Append(this);
    }

    /// <summary>
    /// Gets the fragment text.
    /// </summary>
    public string FragmentText
    {
        get => _fragmentText ??= Doc._text.Substring(Index, Length);
        internal set => _fragmentText = value;
    }

    /// <summary>
    /// Gets the type of fragment.
    /// </summary>
    public MixedCodeDocumentFragmentType FragmentType => _type;

    /// <summary>
    /// Gets the line number of the fragment.
    /// </summary>
    public int Line
    {
        get => _line;
        internal set => _line = value;
    }

    /// <summary>
    /// Gets the line position (column) of the fragment.
    /// </summary>
    public int LinePosition => _lineposition;

    /// <summary>
    /// Gets the fragment position in the document's stream.
    /// </summary>
    public int StreamPosition => Index;
}
