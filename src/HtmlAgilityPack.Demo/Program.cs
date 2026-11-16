// A small tour of Html Agility Pack for .NET 11.
//
// By default it downloads the first web page of the World Wide Web project at CERN
// (last modified in December 1992), which is ideal for a demo: it never changes, and its
// early markup (<HEADER> instead of <HEAD>, unquoted attributes, no closing <P>/<DT>/<DD>
// tags) shows how forgiving the parser is.
//
// Usage: dotnet run --project HtmlAgilityPack.Demo [url]

using System.Text.RegularExpressions;
using HtmlAgilityPack;

var url = new Uri(args.Length > 0 ? args[0] : "https://info.cern.ch/hypertext/WWW/TheProject.html");

// 1. Download and parse the page. Pass your own HttpClient to control headers and timeouts.
using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
http.DefaultRequestHeaders.UserAgent.ParseAdd("HtmlAgilityPack.Demo/2.0");

var doc = new HtmlDocument();
try
{
    await doc.LoadHtmlAsync(url, http);
}
catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
{
    Console.Error.WriteLine($"Could not download {url}: {ex.Message}");
    return 1;
}

HtmlNode root = doc.DocumentNode;

// 2. Simple XPath queries.
Heading("Page");
Console.WriteLine($"URL:          {url}");
Console.WriteLine($"Title:        {Clean(root.SelectSingleNode("//title")?.InnerText)}");
Console.WriteLine($"Heading:      {Clean(root.SelectSingleNode("//h1")?.InnerText)}");
Console.WriteLine($"Elements:     {root.Descendants().Count(n => n.NodeType == HtmlNodeType.Element)}");
Console.WriteLine($"Parse errors: {doc.ParseErrors?.Count() ?? 0}");

// The parser recovers from broken markup and reports what it fixed.
foreach (HtmlParseError error in doc.ParseErrors ?? [])
{
    Console.WriteLine($"  line {error.Line}, col {error.LinePosition}: {error.Code} - {error.Reason}");
}

// 3. Walk sibling nodes: the introduction is everything between the <H1> and the first <P>.
if (root.SelectSingleNode("//h1") is { } h1)
{
    var intro = new List<string>();
    for (HtmlNode? node = h1.NextSibling; node is not null && node.Name != "p"; node = node.NextSibling)
    {
        intro.Add(node.InnerText);
    }

    Heading("Introduction");
    Console.WriteLine(Clean(string.Concat(intro)));
}

// 4. Pair each <DT> with the <DD> that follows it, even though neither tag is ever closed.
if (root.SelectNodes("//dl/dt") is { } terms)
{
    Heading("Contents");
    foreach (HtmlNode term in terms)
    {
        string description = Clean(term.SelectSingleNode("following-sibling::dd[1]")?.InnerText);
        Console.WriteLine($"{Clean(term.InnerText),-20} {description}");
    }
}

// 5. Read attributes and resolve relative links against the page URL.
var links = root.Descendants("a")
    .Where(a => a.GetAttributeValue("href", "").Length > 0)
    .Select(a => (Text: Clean(a.InnerText), Href: new Uri(url, a.GetAttributeValue("href", ""))))
    .ToList();

Heading($"Links ({links.Count})");
foreach (var (text, href) in links)
{
    Console.WriteLine($"{text,-30} {href}");
}

// 6. Change the DOM and save it: make every link absolute so the copy works offline.
foreach (HtmlNode anchor in root.Descendants("a").Where(a => a.Attributes.Contains("href")))
{
    anchor.SetAttributeValue("href", new Uri(url, anchor.GetAttributeValue("href", "")).AbsoluteUri);
}

string outputPath = Path.Combine(Path.GetTempPath(), "TheProject.html");
await doc.SaveAsync(outputPath);

Heading("Saved");
Console.WriteLine($"Copy with absolute links written to {outputPath}");
return 0;

static void Heading(string text)
{
    Console.WriteLine();
    Console.WriteLine(text);
    Console.WriteLine(new string('-', text.Length));
}

// Decode entities such as &amp; and collapse the page's hard line breaks into single spaces.
static string Clean(string? text) =>
    Regex.Replace(HtmlEntity.DeEntitize(text ?? ""), @"\s+", " ").Trim();
