// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace HtmlAgilityPack.Attributes;

/// <summary>
/// Includes XPath and <see cref="NodeReturnType"/>. XPath for finding html tags and <see cref="NodeReturnType"/> for specify which part of <see cref="HtmlNode"/> you want to return.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class XPathAttribute : Attribute
{
    private ReturnType _nodeReturnType;
    internal bool IsNodeReturnTypeExplicitlySet { get; private set; }

    /// <summary>
    /// XPath Expression that is used to find related html node.
    /// </summary>
    public string XPath { get; }

    /// <summary>
    /// Html Attribute name
    /// </summary>
    public string? AttributeName { get; set; }

    /// <summary>
    /// The methode of output
    /// </summary>
    public ReturnType NodeReturnType
    {
        get => _nodeReturnType;
        set
        {
            _nodeReturnType = value;
            IsNodeReturnTypeExplicitlySet = true;
        }
    }

    /// <summary>
    /// Specify Xpath to find related Html Node.
    /// </summary>
    /// <param name="xpathString"></param>
    public XPathAttribute(string xpathString)
    {
        XPath = xpathString;
        _nodeReturnType = ReturnType.InnerText;
    }

    /// <summary>
    /// Specify Xpath to find related Html Node.
    /// </summary>
    /// <param name="xpathString"></param>
    /// <param name="nodeReturnType">Specify you want the output include html text too.</param>
    public XPathAttribute(string xpathString, ReturnType nodeReturnType)
    {
        XPath = xpathString;
        NodeReturnType = nodeReturnType;
    }

    /// <summary>
    /// Specify Xpath and Attribute to find related Html Node and its attribute value.
    /// </summary>
    /// <param name="xpathString"></param>
    /// <param name="attributeName"></param>
    public XPathAttribute(string xpathString, string attributeName)
    {
        XPath = xpathString;
        AttributeName = attributeName;
        _nodeReturnType = ReturnType.InnerText;
    }
}
