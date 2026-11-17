using HtmlAgilityPack.Exceptions;
using Xunit;

namespace HtmlAgilityPack.Tests;

public class ExceptionTests
{
    public static TheoryData<Func<Exception>> DefaultConstructors => new()
    {
        () => new NodeNotFoundException(),
        () => new NodeAttributeNotFoundException(),
        () => new MissingXPathException(),
    };

    [Theory]
    [MemberData(nameof(DefaultConstructors))]
    public void DefaultConstructor_HasNoInnerException(Func<Exception> create)
    {
        var ex = create();

        Assert.Null(ex.InnerException);
        Assert.False(string.IsNullOrEmpty(ex.Message));
    }

    public static TheoryData<Func<string, Exception>> MessageConstructors => new()
    {
        m => new NodeNotFoundException(m),
        m => new NodeAttributeNotFoundException(m),
        m => new MissingXPathException(m),
        m => new InvalidNodeReturnTypeException(m),
    };

    [Theory]
    [MemberData(nameof(MessageConstructors))]
    public void MessageConstructor_SetsMessage(Func<string, Exception> create)
    {
        var ex = create("custom message");

        Assert.Equal("custom message", ex.Message);
        Assert.Null(ex.InnerException);
    }

    public static TheoryData<Func<string, Exception, Exception>> InnerExceptionConstructors => new()
    {
        (m, i) => new NodeNotFoundException(m, i),
        (m, i) => new NodeAttributeNotFoundException(m, i),
        (m, i) => new MissingXPathException(m, i),
        (m, i) => new InvalidNodeReturnTypeException(m, i),
    };

    [Theory]
    [MemberData(nameof(InnerExceptionConstructors))]
    public void InnerExceptionConstructor_SetsMessageAndInnerException(Func<string, Exception, Exception> create)
    {
        var inner = new InvalidOperationException("inner");

        var ex = create("outer", inner);

        Assert.Equal("outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}
