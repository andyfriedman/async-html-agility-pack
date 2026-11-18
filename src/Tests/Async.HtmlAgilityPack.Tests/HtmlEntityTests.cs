using Xunit;

namespace Async.HtmlAgilityPack.Tests;

public class HtmlEntityTests
{
    [Theory]
    [InlineData("&#foo;", "&#foo;")]
    [InlineData("&#xGG;", "&#xGG;")]
    [InlineData("&#;", "&#;")]
    [InlineData("prefix &#foo; suffix", "prefix &#foo; suffix")]
    [InlineData("&#65;", "A")]
    [InlineData("&#x41;", "A")]
    [InlineData("&amp;", "&")]
    [InlineData("&notreal;", "&notreal;")]
    public void DeEntitize_HandlesInvalidNumericEntitiesWithoutDuplicatingHash(string input, string expected)
    {
        Assert.Equal(expected, HtmlEntity.DeEntitize(input));
    }

    [Fact]
    public void Entitize_PassEmojiUnicode_ShouldCorrectlyEntitize()
    {
        try
        {
            HtmlEntity.UseWebUtility = true;
            string? result = HtmlEntity.Entitize("😂");

            Assert.Equal("&#128514;", result);
        }
        finally
        {
            HtmlEntity.UseWebUtility = false;
        }
    }

    [Fact]
    public void Entitize_PassSimpleText_ShouldCorrectlyEntitize()
    {
        try
        {
            HtmlEntity.UseWebUtility = true;
            string? result = HtmlEntity.Entitize("qwerty");

            Assert.Equal("qwerty", result);
        }
        finally
        {
            HtmlEntity.UseWebUtility = false;
        }
    }
}
