// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

using System.Xml;

namespace Async.HtmlAgilityPack;

internal sealed class HtmlNameTable : XmlNameTable
{
    private readonly NameTable _nametable = new();

    public override string Add(string array) => _nametable.Add(array);

    public override string Add(char[] array, int offset, int length) =>
        _nametable.Add(array, offset, length);

    public override string? Get(string array) => _nametable.Get(array);

    public override string? Get(char[] array, int offset, int length) =>
        _nametable.Get(array, offset, length);

    internal string GetOrAdd(string array) => Get(array) ?? Add(array);
}
