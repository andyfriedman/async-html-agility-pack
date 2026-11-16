// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

using System.Text;

namespace HtmlAgilityPack;

/// <summary>
/// Represents a document with mixed code and text. ASP, ASPX, JSP, are good example of such documents.
/// </summary>
public class MixedCodeDocument
{
    private MixedCodeDocumentFragment _currentfragment = null!;
    private Encoding? _streamencoding;
    internal string _text = string.Empty;
    private int _c;
    internal MixedCodeDocumentFragmentList _codefragments;
    internal MixedCodeDocumentFragmentList _fragments;
    private int _index;
    private int _line;
    private int _lineposition;
    private ParseState _state;
    internal MixedCodeDocumentFragmentList _textfragments;

    /// <summary>
    /// Gets or sets the token representing code end.
    /// </summary>
    public string TokenCodeEnd = "%>";

    /// <summary>
    /// Gets or sets the token representing code start.
    /// </summary>
    public string TokenCodeStart = "<%";

    /// <summary>
    /// Gets or sets the token representing code directive.
    /// </summary>
    public string TokenDirective = "@";

    /// <summary>
    /// Gets or sets the token representing response write directive.
    /// </summary>
    public string TokenResponseWrite = "Response.Write ";

    private const string TokenTextBlock = "TextBlock({0})";

    /// <summary>
    /// Creates a mixed code document instance.
    /// </summary>
    public MixedCodeDocument()
    {
        _codefragments = new MixedCodeDocumentFragmentList(this);
        _textfragments = new MixedCodeDocumentFragmentList(this);
        _fragments = new MixedCodeDocumentFragmentList(this);
    }

    /// <summary>
    /// Gets the code represented by the mixed code document seen as a template.
    /// </summary>
    public string Code
    {
        get
        {
            var sb = new StringBuilder();
            int i = 0;
            foreach (MixedCodeDocumentFragment frag in _fragments)
            {
                switch (frag._type)
                {
                    case MixedCodeDocumentFragmentType.Text:
                        sb.Append(TokenResponseWrite).AppendFormat(TokenTextBlock, i).Append('\n');
                        i++;
                        break;

                    case MixedCodeDocumentFragmentType.Code:
                        sb.Append(((MixedCodeDocumentCodeFragment)frag).Code).Append('\n');
                        break;
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Gets the list of code fragments in the document.
    /// </summary>
    public MixedCodeDocumentFragmentList CodeFragments => _codefragments;

    /// <summary>
    /// Gets the list of all fragments in the document.
    /// </summary>
    public MixedCodeDocumentFragmentList Fragments => _fragments;

    /// <summary>
    /// Gets the encoding of the stream used to read the document.
    /// </summary>
    public Encoding? StreamEncoding => _streamencoding;

    /// <summary>
    /// Gets the list of text fragments in the document.
    /// </summary>
    public MixedCodeDocumentFragmentList TextFragments => _textfragments;

    /// <summary>
    /// Creates a code fragment instance.
    /// </summary>
    /// <returns>The newly created code fragment instance.</returns>
    public MixedCodeDocumentCodeFragment CreateCodeFragment() =>
        (MixedCodeDocumentCodeFragment)CreateFragment(MixedCodeDocumentFragmentType.Code);

    /// <summary>
    /// Creates a text fragment instance.
    /// </summary>
    /// <returns>The newly created text fragment instance.</returns>
    public MixedCodeDocumentTextFragment CreateTextFragment() =>
        (MixedCodeDocumentTextFragment)CreateFragment(MixedCodeDocumentFragmentType.Text);

    /// <summary>
    /// Asynchronously loads a mixed code document from a stream.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task LoadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var sr = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: true);
        await LoadAsync(sr, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously loads a mixed code document from a stream.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the stream.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task LoadAsync(Stream stream, bool detectEncodingFromByteOrderMarks, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var sr = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks, bufferSize: -1, leaveOpen: true);
        await LoadAsync(sr, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously loads a mixed code document from a stream.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task LoadAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoding);
        using var sr = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: true);
        await LoadAsync(sr, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously loads a mixed code document from a stream.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the stream.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task LoadAsync(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoding);
        using var sr = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize: -1, leaveOpen: true);
        await LoadAsync(sr, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously loads a mixed code document from a stream.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the stream.</param>
    /// <param name="buffersize">The minimum buffer size.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task LoadAsync(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoding);
        using var sr = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks, buffersize, leaveOpen: true);
        await LoadAsync(sr, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously loads a mixed code document from a file.
    /// </summary>
    /// <param name="path">The complete file path to be read.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);
        using var sr = new StreamReader(path);
        await LoadAsync(sr, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously loads a mixed code document from a file.
    /// </summary>
    /// <param name="path">The complete file path to be read.</param>
    /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task LoadAsync(string path, bool detectEncodingFromByteOrderMarks, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);
        using var sr = new StreamReader(path, detectEncodingFromByteOrderMarks);
        await LoadAsync(sr, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously loads a mixed code document from a file.
    /// </summary>
    /// <param name="path">The complete file path to be read.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task LoadAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(encoding);
        using var sr = new StreamReader(path, encoding);
        await LoadAsync(sr, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously loads a mixed code document from a file.
    /// </summary>
    /// <param name="path">The complete file path to be read.</param>
    /// <param name="encoding">The character encoding to use.</param>
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
    /// Asynchronously loads a mixed code document from a file.
    /// </summary>
    /// <param name="path">The complete file path to be read.</param>
    /// <param name="encoding">The character encoding to use.</param>
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
    /// Asynchronously loads the mixed code document from the specified TextReader.
    /// </summary>
    /// <param name="reader">The TextReader used to feed the HTML data into the document.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task LoadAsync(TextReader reader, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);

        _codefragments.Clear();
        _textfragments.Clear();
        _fragments.Clear();

        _text = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        _streamencoding = (reader as StreamReader)?.CurrentEncoding;

        Parse();
    }

    /// <summary>
    /// Loads a mixed document from an HTML string.
    /// </summary>
    /// <param name="html">The text to load.</param>
    public void LoadHtml(string html)
    {
        ArgumentNullException.ThrowIfNull(html);

        _codefragments.Clear();
        _textfragments.Clear();
        _fragments.Clear();
        _streamencoding = null;
        _text = html;

        Parse();
    }

    /// <summary>
    /// Asynchronously saves the mixed document to the specified stream.
    /// </summary>
    /// <param name="outStream">The stream to which you want to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task SaveAsync(Stream outStream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outStream);
        await SaveAsync(outStream, GetOutEncoding(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously saves the mixed document to the specified stream.
    /// </summary>
    /// <param name="outStream">The stream to which you want to save.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task SaveAsync(Stream outStream, Encoding encoding, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outStream);
        ArgumentNullException.ThrowIfNull(encoding);

        var sw = new StreamWriter(outStream, encoding, bufferSize: -1, leaveOpen: true);
        await using (sw.ConfigureAwait(false))
        {
            await SaveAsync(sw, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously saves the mixed document to the specified file.
    /// </summary>
    /// <param name="filename">The location of the file where you want to save the document.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task SaveAsync(string filename, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filename);
        await SaveAsync(filename, GetOutEncoding(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously saves the mixed document to the specified file.
    /// </summary>
    /// <param name="filename">The location of the file where you want to save the document.</param>
    /// <param name="encoding">The character encoding to use.</param>
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

    /// <summary>
    /// Asynchronously saves the mixed document to the specified StreamWriter.
    /// </summary>
    /// <param name="writer">The StreamWriter to which you want to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public Task SaveAsync(StreamWriter writer, CancellationToken cancellationToken = default) =>
        SaveAsync((TextWriter)writer, cancellationToken);

    /// <summary>
    /// Asynchronously saves the mixed document to the specified TextWriter.
    /// </summary>
    /// <param name="writer">The TextWriter to which you want to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task SaveAsync(TextWriter writer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(writer);

        await writer.WriteAsync(_text.AsMemory(), cancellationToken).ConfigureAwait(false);

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    internal MixedCodeDocumentFragment CreateFragment(MixedCodeDocumentFragmentType type) =>
        type switch
        {
            MixedCodeDocumentFragmentType.Text => new MixedCodeDocumentTextFragment(this),
            MixedCodeDocumentFragmentType.Code => new MixedCodeDocumentCodeFragment(this),
            _ => throw new NotSupportedException()
        };

    internal Encoding GetOutEncoding() => _streamencoding ?? Encoding.UTF8;

    private void IncrementPosition()
    {
        _index++;
        if (_c == 10)
        {
            _lineposition = 1;
            _line++;
        }
        else
        {
            _lineposition++;
        }
    }

    private void Parse()
    {
        _state = ParseState.Text;
        _index = 0;
        _line = 1;
        _lineposition = 1;
        _currentfragment = CreateFragment(MixedCodeDocumentFragmentType.Text);

        while (_index < _text.Length)
        {
            _c = _text[_index];
            IncrementPosition();

            switch (_state)
            {
                case ParseState.Text:
                    if (_index + TokenCodeStart.Length < _text.Length)
                    {
                        if (string.CompareOrdinal(_text, _index - 1, TokenCodeStart, 0, TokenCodeStart.Length) == 0)
                        {
                            _state = ParseState.Code;
                            _currentfragment.Length = _index - 1 - _currentfragment.Index;
                            _currentfragment = CreateFragment(MixedCodeDocumentFragmentType.Code);
                            SetPosition();
                            continue;
                        }
                    }

                    break;

                case ParseState.Code:
                    if (_index + TokenCodeEnd.Length < _text.Length)
                    {
                        if (string.CompareOrdinal(_text, _index - 1, TokenCodeEnd, 0, TokenCodeEnd.Length) == 0)
                        {
                            _state = ParseState.Text;
                            _currentfragment.Length = _index + TokenCodeEnd.Length - _currentfragment.Index;
                            _index += TokenCodeEnd.Length;
                            _lineposition += TokenCodeEnd.Length;
                            _currentfragment = CreateFragment(MixedCodeDocumentFragmentType.Text);
                            SetPosition();
                            continue;
                        }
                    }

                    break;
            }
        }

        _currentfragment.Length = _index - _currentfragment.Index;
    }

    private void SetPosition()
    {
        _currentfragment.Line = _line;
        _currentfragment._lineposition = _lineposition;
        _currentfragment.Index = _index - 1;
        _currentfragment.Length = 0;
    }

    private enum ParseState
    {
        Text,
        Code
    }
}
