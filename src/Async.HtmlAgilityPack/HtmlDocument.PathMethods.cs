// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

using System.Text;

namespace Async.HtmlAgilityPack;

public partial class HtmlDocument
{
    /// <summary>
    /// Asynchronously detects the encoding of an HTML document from a file first, and then loads the file.
    /// </summary>
    /// <param name="path">The complete file path to be read.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public Task DetectEncodingAndLoadAsync(string path, CancellationToken cancellationToken = default) =>
        DetectEncodingAndLoadAsync(path, detectEncoding: true, cancellationToken);

    /// <summary>
    /// Asynchronously detects the encoding of an HTML document from a file first, and then loads the file.
    /// </summary>
    /// <param name="path">The complete file path to be read. May not be null.</param>
    /// <param name="detectEncoding">true to detect encoding, false otherwise.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task DetectEncodingAndLoadAsync(string path, bool detectEncoding, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);

        Encoding? enc = detectEncoding
            ? await DetectEncodingAsync(path, cancellationToken).ConfigureAwait(false)
            : null;

        if (enc is null)
        {
            await LoadAsync(path, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await LoadAsync(path, enc, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously detects the encoding of an HTML file.
    /// </summary>
    /// <param name="path">Path for the file containing the HTML document to detect. May not be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The detected encoding.</returns>
    public async Task<Encoding?> DetectEncodingAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);

        using var sr = new StreamReader(path, OptionDefaultStreamEncoding);
        return await DetectEncodingAsync(sr, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously loads an HTML document from a file.
    /// </summary>
    /// <param name="path">The complete file path to be read. May not be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);

        using var sr = new StreamReader(path, OptionDefaultStreamEncoding);
        await LoadAsync(sr, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously loads an HTML document from a file.
    /// </summary>
    /// <param name="path">The complete file path to be read. May not be null.</param>
    /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task LoadAsync(string path, bool detectEncodingFromByteOrderMarks, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);

        using var sr = new StreamReader(path, detectEncodingFromByteOrderMarks);
        await LoadAsync(sr, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously loads an HTML document from a file.
    /// </summary>
    /// <param name="path">The complete file path to be read. May not be null.</param>
    /// <param name="encoding">The character encoding to use. May not be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task LoadAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(encoding);

        using var sr = new StreamReader(path, encoding);
        await LoadAsync(sr, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously loads an HTML document from a file.
    /// </summary>
    /// <param name="path">The complete file path to be read. May not be null.</param>
    /// <param name="encoding">The character encoding to use. May not be null.</param>
    /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task LoadAsync(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(encoding);

        using var sr = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks);
        await LoadAsync(sr, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously loads an HTML document from a file.
    /// </summary>
    /// <param name="path">The complete file path to be read. May not be null.</param>
    /// <param name="encoding">The character encoding to use. May not be null.</param>
    /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
    /// <param name="buffersize">The minimum buffer size.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task LoadAsync(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(encoding);

        using var sr = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks, buffersize);
        await LoadAsync(sr, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously saves the HTML document to the specified file.
    /// </summary>
    /// <param name="filename">The location of the file where you want to save the document. May not be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task SaveAsync(string filename, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filename);

        var sw = new StreamWriter(filename, append: false, GetOutEncoding());
        await using (sw.ConfigureAwait(false))
        {
            await SaveAsync(sw, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously saves the HTML document to the specified file.
    /// </summary>
    /// <param name="filename">The location of the file where you want to save the document. May not be null.</param>
    /// <param name="encoding">The character encoding to use. May not be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task SaveAsync(string filename, Encoding encoding, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filename);
        ArgumentNullException.ThrowIfNull(encoding);

        var sw = new StreamWriter(filename, append: false, encoding);
        await using (sw.ConfigureAwait(false))
        {
            await SaveAsync(sw, cancellationToken).ConfigureAwait(false);
        }
    }
}
