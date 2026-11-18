# Async Html Agility Pack

An async-first fork of [Html Agility Pack](https://github.com/zzzprojects/html-agility-pack) (HAP), the HTML parser that builds a read/write DOM you can query with XPath or LINQ. Like upstream HAP, it tolerates real-world malformed HTML: unclosed tags, unquoted attributes and stray end tags.

This fork is built for server workloads:

- **No blocking I/O.** Every file, stream and network method is `async` and takes a `CancellationToken`.
- **Downloading is built in.** `HtmlDocument.LoadHtmlAsync(Uri)` uses `HttpClient`. Pass your own client to control headers, timeouts, proxies and cookies, or to use one from `IHttpClientFactory`.
- **Nullable annotations** on the whole public API.

It isn't affiliated with ZZZ Projects, the maintainers of upstream Html Agility Pack.

## Getting started

```csharp
using Async.HtmlAgilityPack;

var doc = new HtmlDocument();
doc.LoadHtml("<ul><li>Tea<li>Coffee</ul>");

foreach (HtmlNode li in doc.DocumentNode.Descendants("li"))
    Console.WriteLine(li.InnerText);

string? first = doc.DocumentNode.SelectSingleNode("//li")?.InnerText;
```

Parsing from a string is synchronous, because it does no I/O. Everything that reads or writes a file, stream or the network is async.

### Download a page

```csharp
var doc = new HtmlDocument();
await doc.LoadHtmlAsync(new Uri("https://example.com/"), cancellationToken);
```

With your own `HttpClient`, for example a typed client from `IHttpClientFactory`:

```csharp
await doc.LoadHtmlAsync(new Uri("https://example.com/"), httpClient, cancellationToken);
```

Only absolute `http` and `https` URIs are accepted. A non-success status code throws `HttpRequestException`.

### Files and streams

```csharp
var doc = new HtmlDocument();
await doc.LoadAsync("page.html", cancellationToken);

foreach (HtmlNode a in doc.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>())
    a.SetAttributeValue("rel", "noopener");

await doc.SaveAsync("page-out.html", cancellationToken);
```

## Migrating from Html Agility Pack 1.x

The type names and the DOM API are the same, so most query and manipulation code works unchanged once you update the `using` directives. The main differences:

| 1.x | This package |
|---|---|
| `using HtmlAgilityPack;` | `using Async.HtmlAgilityPack;` |
| `new HtmlWeb().Load(url)` | `await doc.LoadHtmlAsync(new Uri(url))` |
| `HtmlWeb` settings (user agent, timeout, proxy, cookies) | Configure an `HttpClient` and pass it to `LoadHtmlAsync` |
| `doc.Load(...)`, `doc.Save(...)` | `await doc.LoadAsync(...)`, `await doc.SaveAsync(...)` |
| `node.WriteTo(TextWriter)` | `await node.WriteToAsync(TextWriter)` |
| `HtmlWebException` | `HttpRequestException` |
| `[XPath]`, `[HasXPath]`, `[SkipNodeNotFound]` | Now in the `Async.HtmlAgilityPack.Attributes` namespace |
| Mapping exceptions (`NodeNotFoundException`, …) | Now in the `Async.HtmlAgilityPack.Exceptions` namespace |

These were removed with no replacement: `LoadFromBrowser`, the `HtmlWeb` file cache, and `HtmlCmdLine`/`HtmlConsoleListener`.

The package, assembly (`Async.HtmlAgilityPack.dll`) and namespaces (`Async.HtmlAgilityPack`) all have different names from upstream Html Agility Pack, so the two can be used in the same project, for example when another dependency brings in upstream `HtmlAgilityPack`.

## Documentation

The repository has [full documentation](https://github.com/andyfriedman/async-html-agility-pack/tree/main/docs), with guides and a complete API reference, plus the [list of breaking changes](https://github.com/andyfriedman/async-html-agility-pack#breaking-changes).

## License

MIT. Html Agility Pack was originally created by Simon Mourier and is maintained by ZZZ Projects Inc. Their copyright notices are kept alongside this fork's.
