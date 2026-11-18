// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace HtmlAgilityPack;

internal sealed class NameValuePairList
{
    internal readonly string? Text;
    private readonly List<KeyValuePair<string, string>> _allPairs = [];
    private readonly Dictionary<string, List<KeyValuePair<string, string>>> _pairsWithName = new();

    internal NameValuePairList()
        : this(null)
    {
    }

    internal NameValuePairList(string? text)
    {
        Text = text;
        Parse(text);
    }

    internal static string GetNameValuePairsValue(string? text, string name)
    {
        var list = new NameValuePairList(text);
        return list.GetNameValuePairValue(name);
    }

    internal List<KeyValuePair<string, string>> GetNameValuePairs(string? name)
    {
        if (name is null)
        {
            return _allPairs;
        }

        return _pairsWithName.TryGetValue(name, out List<KeyValuePair<string, string>>? value)
            ? value
            : [];
    }

    internal string GetNameValuePairValue(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        List<KeyValuePair<string, string>> al = GetNameValuePairs(name);
        if (al.Count == 0)
        {
            return string.Empty;
        }

        return al[0].Value.Trim();
    }

    private void Parse(string? text)
    {
        _allPairs.Clear();
        _pairsWithName.Clear();
        if (text is null)
        {
            return;
        }

        string[] parts = text.Split(';');
        foreach (string pv in parts)
        {
            if (pv.Length == 0)
            {
                continue;
            }

            string[] onep = pv.Split('=', 2);
            if (onep.Length == 0)
            {
                continue;
            }

            var nvp = new KeyValuePair<string, string>(
                onep[0].Trim().ToLowerInvariant(),
                onep.Length < 2 ? string.Empty : onep[1]);

            _allPairs.Add(nvp);

            if (!_pairsWithName.TryGetValue(nvp.Key, out List<KeyValuePair<string, string>>? al))
            {
                al = [];
                _pairsWithName.Add(nvp.Key, al);
            }

            al.Add(nvp);
        }
    }
}
