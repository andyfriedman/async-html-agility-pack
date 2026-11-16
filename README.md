# Html Agility Pack for .NET 11

Html Agility Pack (HAP) is an HTML parser that builds a read/write DOM and supports plain XPath or XSLT. It tolerates "real world" malformed HTML, and its object model is very similar to `System.Xml`, but for HTML documents and streams.

This repository is a completely modernized fork that targets **.NET 11 only**. It removes the messy conditional compilation needed to support many legacy frameworks, folds HTML downloading into `HtmlDocument`, and moves all I/O to async/await.

## Credits

Html Agility Pack was originally created by Simon Mourier and is maintained by ZZZ Projects Inc. This fork builds directly on their work, and the original copyright notices are kept in the source files. Thanks to them and to all upstream contributors for the parser, the DOM and the XPath support that this version depends on.

## Requirements

- .NET 11 SDK

## Building and testing

```bash
cd src
dotnet build HtmlAgilityPack.sln
dotnet test HtmlAgilityPack.sln
```

The solution has three projects:

| Project | Path |
|---|---|
| Library | `src/HtmlAgilityPack/HtmlAgilityPack.csproj` |
| Tests (xUnit) | `src/Tests/HtmlAgilityPack.Tests/HtmlAgilityPack.Tests.csproj` |
| Demo console app | `src/HtmlAgilityPack.Demo/HtmlAgilityPack.Demo.csproj` |

## Demo

The demo app downloads the first page of the World Wide Web project at CERN, which hasn't changed since 1992, and prints what it finds:

```bash
cd src
dotnet run --project HtmlAgilityPack.Demo
```

It shows:

- downloading a page with `LoadHtmlAsync` and a custom `HttpClient`
- XPath queries for the title and heading
- the parse errors the parser recovered from (the 1992 markup has a stray `</A>`)
- walking sibling nodes to pull out the introduction
- pairing each `<DT>` with its `<DD>`, even though neither tag is ever closed
- reading `href` attributes and resolving them to absolute URLs
- rewriting the links in the DOM and saving a copy with `SaveAsync`

Pass a different URL to try it on another page: `dotnet run --project HtmlAgilityPack.Demo -- https://example.com/`.

## Quick start

Parse HTML from a string. This is still synchronous:

```csharp
var doc = new HtmlDocument();
doc.LoadHtml("<html><body><h1>Hello</h1></body></html>");

string? title = doc.DocumentNode.SelectSingleNode("//h1")?.InnerText;
```

Download and parse a page:

```csharp
var doc = new HtmlDocument();
await doc.LoadHtmlAsync(new Uri("https://example.com/"));
```

Use your own `HttpClient` for headers, proxies, cookies, timeouts, or a stub handler in tests:

```csharp
using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
client.DefaultRequestHeaders.UserAgent.ParseAdd("MyCrawler/1.0");

var doc = new HtmlDocument();
await doc.LoadHtmlAsync(new Uri("https://example.com/"), client, cancellationToken);
```

Load from a file or stream, and save:

```csharp
var doc = new HtmlDocument();
await doc.LoadAsync("page.html", Encoding.UTF8);

// ... modify the DOM ...

await doc.SaveAsync("page-out.html");
```

## What changed in this version

### Platform and project structure

- Targets **.NET 11 only**. The 13 legacy projects are gone: .NET Framework 2.0–4.5, .NET Standard 1.3/1.6/2.0, .NET 8, NetCore45, UAP10 and the Windows Phone 8/8.1 portable libraries.
- The shared project (`HtmlAgilityPack.Shared`) and the per-framework projects are merged into one SDK-style project, `src/HtmlAgilityPack`.
- About 425 framework-specific `#if` blocks are removed, along with the platform polyfills (`Trace`, `IOLibrary`, `InvalidProgramException`, the Metro-specific code).
- One solution, `src/HtmlAgilityPack.sln`, with the library and one test project.

### Downloading HTML

- `HtmlWeb` is removed, along with its browser automation, file cache, MIME type map and `HttpWebRequest` code paths.
- Downloading is built into `HtmlDocument` through `LoadHtmlAsync(Uri, ...)` overloads, which accept an optional `HttpClient`, an optional `Encoding` and a `CancellationToken`.
- Only absolute `http` and `https` URIs are accepted. Non-success status codes throw `HttpRequestException`.
- With no `HttpClient`, a shared static `HttpClient` is used.

### Async I/O

- All file, stream and writer I/O is async, takes a `CancellationToken` and uses the `Async` suffix:
  - `HtmlDocument`: `LoadAsync`, `SaveAsync`, `DetectEncodingAsync`, `DetectEncodingAndLoadAsync`
  - `MixedCodeDocument`: `LoadAsync`, `SaveAsync`
  - `HtmlNode`: `WriteToAsync`, `WriteContentToAsync`
  - `HtmlNodeNavigator`: new `CreateAsync` factory methods
- Library code uses `ConfigureAwait(false)` throughout.
- Parsing from in-memory strings is still synchronous: `HtmlDocument.LoadHtml(string)`, `HtmlNode.WriteTo()`, `OuterHtml`, XPath queries.

### Language and code quality

- Nullable reference types are enabled across the library and tests, with annotated public APIs.
- Uses modern C#: file-scoped namespaces in most files, `ArgumentNullException.ThrowIfNull`, pattern matching (`is null` / `is not null`), implicit usings, and `await using`.
- Builds with zero compiler warnings, and `TreatWarningsAsErrors` is on in both projects.
- Helper types that were never meant to be public are now internal, such as `Crc32`.

### Tests

- The two old test projects (NUnit on .NET Framework 4.5, and xUnit on .NET Standard 2.0) are merged into one xUnit project, `src/Tests/HtmlAgilityPack.Tests`.
- Every original test with real assertions was kept, and the NUnit tests were converted to xUnit. Two placeholders were dropped: an empty MSTest `TestMethod1`, and `TestLoadWithUri`, whose body was commented out. Tests that use the now-async APIs are async and have the `Async` suffix.
- New tests cover the download API, async load/save, `MixedCodeDocument`, `HtmlNodeNavigator.CreateAsync` and `XmlWriter` output. HTTP is stubbed with a fake `HttpMessageHandler`, so no test needs network access.
- 142 tests, all passing.

## Breaking changes

This version isn't a drop-in replacement for upstream Html Agility Pack 1.x.

### Removed: `HtmlWeb` and `HtmlWebException`

Use `HtmlDocument.LoadHtmlAsync`. Anything that was configured on `HtmlWeb` is now configured on the `HttpClient` you pass in:

| Before (`HtmlWeb`) | Now |
|---|---|
| `new HtmlWeb().Load(url)` | `await doc.LoadHtmlAsync(new Uri(url))` |
| `LoadFromWebAsync(url, encoding)` | `await doc.LoadHtmlAsync(uri, encoding)` |
| `UserAgent`, `Timeout`, `UseCookies`, proxy and credentials overloads | Configure an `HttpClient` / `HttpClientHandler` and pass it to `LoadHtmlAsync` |
| `PreRequest` / `PostResponse` handlers | A `DelegatingHandler` on your `HttpClient` |
| `AutomaticDecompression`, `MaxAutoRedirects`, `CaptureRedirect` | Properties of your `HttpClientHandler` / `SocketsHttpHandler` |
| `StatusCode`, `ResponseUri`, `RequestDuration` | Not exposed. Make the request yourself and call `doc.LoadHtml(html)` if you need them |
| `LoadFromBrowser(...)`, `BrowserTimeout`, `BrowserDelay` | Removed, no replacement |
| `CachePath`, `UsingCache`, `CacheOnly`, `FromCache` | Removed, no replacement |
| `LoadHtmlAsXml(...)`, `CreateInstance(...)` (XSLT) | Removed. Load the document, then use `HtmlNodeNavigator` with `System.Xml.Xsl` yourself |
| `Get(...)` | Removed |
| `GetContentTypeForExtension` / `GetExtensionForContentType` | Removed, along with the MIME type map |
| `HtmlWebException` | `HttpRequestException` from `HttpClient` |
| `Load("file:///...")` | Only `http` / `https` URIs are accepted. For files, use `LoadAsync(path)` |

### Removed: synchronous I/O methods

Each synchronous overload was replaced by an async overload with the same parameters plus a `CancellationToken`:

| Type | Removed | Replacement |
|---|---|---|
| `HtmlDocument` | `Load(...)` (all 11 stream, reader and path overloads) | `LoadAsync(...)` |
| `HtmlDocument` | `Save(...)` (all 7 overloads, including `XmlWriter`) | `SaveAsync(...)` |
| `HtmlDocument` | `DetectEncoding(...)`, `DetectEncodingAndLoad(...)` | `DetectEncodingAsync(...)`, `DetectEncodingAndLoadAsync(...)` |
| `MixedCodeDocument` | `Load(...)`, `Save(...)` | `LoadAsync(...)`, `SaveAsync(...)` |
| `HtmlNode` | `WriteTo(TextWriter, int)`, `WriteTo(XmlWriter)`, `WriteContentTo(TextWriter, int)` | `WriteToAsync(...)`, `WriteContentToAsync(...)` |

These are unchanged: `HtmlDocument.LoadHtml(string)`, `MixedCodeDocument.LoadHtml(string)`, `HtmlNode.WriteTo()` and `WriteContentTo()` (the string-returning versions), and the `HtmlNodeNavigator` constructors.

### Other changes

- **Target framework:** .NET 11 is required. Older .NET, .NET Framework, .NET Standard, UWP and Windows Phone aren't supported.
- **Nullable annotations:** public APIs now declare which values can be null. Code with nullable warnings turned on may see new warnings where it uses possibly-null results, such as `SelectSingleNode` or `GetAttributeValue`.
- **`Crc32` is internal.**
- **Download encoding:** when no `Encoding` is passed to `LoadHtmlAsync`, the response is decoded using the `Content-Type` header's charset, falling back to UTF-8. A `<meta charset>` in the page isn't used, unlike `HtmlWeb`'s automatic detection. Pass an `Encoding` explicitly for pages that only declare their encoding in markup.
- **Command-line and trace helpers removed:** `HtmlCmdLine`, `HtmlConsoleListener` and the `Trace` wrappers.

## License

MIT. See [LICENSE](LICENSE). The original copyright notices are kept in each source file alongside the notices for this version's changes.
