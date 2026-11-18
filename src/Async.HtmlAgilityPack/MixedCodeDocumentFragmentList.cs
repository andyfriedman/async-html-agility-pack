// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

using System.Collections;

namespace Async.HtmlAgilityPack;

/// <summary>
/// Represents a list of mixed code fragments.
/// </summary>
public class MixedCodeDocumentFragmentList : IEnumerable<MixedCodeDocumentFragment>
{
    private readonly MixedCodeDocument _doc;
    private readonly List<MixedCodeDocumentFragment> _items = [];

    internal MixedCodeDocumentFragmentList(MixedCodeDocument doc)
    {
        _doc = doc;
    }

    /// <summary>
    /// Gets the Document.
    /// </summary>
    public MixedCodeDocument Doc => _doc;

    /// <summary>
    /// Gets the number of fragments contained in the list.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Gets a fragment from the list using its index.
    /// </summary>
    public MixedCodeDocumentFragment this[int index] => _items[index];

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<MixedCodeDocumentFragment> IEnumerable<MixedCodeDocumentFragment>.GetEnumerator() =>
        _items.GetEnumerator();

    /// <summary>
    /// Appends a fragment to the list of fragments.
    /// </summary>
    /// <param name="newFragment">The fragment to append. May not be null.</param>
    public void Append(MixedCodeDocumentFragment newFragment)
    {
        ArgumentNullException.ThrowIfNull(newFragment);
        _items.Add(newFragment);
    }

    /// <summary>
    /// Gets an enumerator that can iterate through the fragment list.
    /// </summary>
    public MixedCodeDocumentFragmentEnumerator GetEnumerator() => new(_items);

    /// <summary>
    /// Prepends a fragment to the list of fragments.
    /// </summary>
    /// <param name="newFragment">The fragment to prepend. May not be null.</param>
    public void Prepend(MixedCodeDocumentFragment newFragment)
    {
        ArgumentNullException.ThrowIfNull(newFragment);
        _items.Insert(0, newFragment);
    }

    /// <summary>
    /// Removes a fragment from the list of fragments. If this fragment was not in the list, an exception will be raised.
    /// </summary>
    /// <param name="fragment">The fragment to remove. May not be null.</param>
    public void Remove(MixedCodeDocumentFragment fragment)
    {
        ArgumentNullException.ThrowIfNull(fragment);

        int index = GetFragmentIndex(fragment);
        if (index == -1)
        {
            throw new IndexOutOfRangeException();
        }

        RemoveAt(index);
    }

    /// <summary>
    /// Removes all fragments from the list.
    /// </summary>
    public void RemoveAll() => _items.Clear();

    /// <summary>
    /// Removes a fragment from the list of fragments, using its index in the list.
    /// </summary>
    /// <param name="index">The index of the fragment to remove.</param>
    public void RemoveAt(int index) => _items.RemoveAt(index);

    internal void Clear() => _items.Clear();

    internal int GetFragmentIndex(MixedCodeDocumentFragment fragment)
    {
        ArgumentNullException.ThrowIfNull(fragment);

        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] == fragment)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Represents a fragment enumerator.
    /// </summary>
    public class MixedCodeDocumentFragmentEnumerator : IEnumerator<MixedCodeDocumentFragment>
    {
        private int _index = -1;
        private readonly IList<MixedCodeDocumentFragment> _items;

        internal MixedCodeDocumentFragmentEnumerator(IList<MixedCodeDocumentFragment> items)
        {
            _items = items;
        }

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        public MixedCodeDocumentFragment Current => _items[_index];

        object IEnumerator.Current => Current;

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
        public bool MoveNext()
        {
            _index++;
            return _index < _items.Count;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset() => _index = -1;

        /// <summary>
        /// Releases resources used by the enumerator.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
