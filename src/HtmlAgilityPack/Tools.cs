// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

using HtmlAgilityPack.Attributes;
using HtmlAgilityPack.Exceptions;
using System.Collections;
using System.Reflection;

namespace HtmlAgilityPack;

/// <summary>
/// Includes tools that GetEncapsulatedData method uses them.
/// </summary>
internal static class Tools
{

    /// <summary>
    /// Determine if a type define an attribute or not , supporting both .NetStandard and .NetFramework2.0
    /// </summary>
    /// <param name="type">Type you want to test it.</param>
    /// <param name="attributeType">Attribute that type must have or not.</param>
    /// <returns>If true , The type parameter define attributeType parameter.</returns>
    internal static bool IsDefinedAttribute(this Type type, Type attributeType)
    {

        if (type == null)
        {
            throw new ArgumentNullException("Parameter type is null when checking type defined attributeType or not.");
        }

        if (attributeType == null)
        {
            throw new ArgumentNullException("Parameter attributeType is null when checking type defined attributeType or not.");
        }

        if (type.IsDefined(attributeType, false) == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// Retrive properties of type that defined <see cref="XPathAttribute"/>.
    /// </summary>
    /// <param name="type">Type that you want to find it's XPath-Defined properties.</param>
    /// <returns>IEnumerable of property infos of a type , that defined specific attribute.</returns>
    internal static IEnumerable<PropertyInfo> GetPropertiesDefinedXPath(this Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException("Parameter type is null while retrieving properties defined XPathAttribute of Type type.");
        }

        PropertyInfo[]? properties = null;


        properties = type.GetProperties();




        return properties.Where(x => x.IsDefined(typeof(XPathAttribute), false));
    }


    /// <summary>
    /// Determine if a <see cref="PropertyInfo"/> has implemented <see cref="IEnumerable"/> BUT <see cref="string"/> is considered as NONE-IEnumerable !
    /// </summary>
    /// <param name="propertyInfo">The property info you want to test.</param>
    /// <returns>True if property info is IEnumerable.</returns>
    internal static bool IsIEnumerable(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
        {
            throw new ArgumentNullException("Parameter propertyInfo is null while checking propertyInfo for being IEnumerable or not.");
        }


        if (propertyInfo.PropertyType == typeof(string))
        {
            return false;
        }
        else
        {
            return typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType);
        }
    }


    /// <summary>
    /// Returns T type(first generic type) of <see cref="IEnumerable{T}"/> or <see cref="List{T}"/>.
    /// </summary>
    /// <param name="propertyInfo">IEnumerable-Implemented property</param>
    /// <returns>List of generic types.</returns>
    internal static IEnumerable<Type> GetGenericTypes(this PropertyInfo propertyInfo)
    {

        if (propertyInfo == null)
        {
            throw new ArgumentNullException("Parameter propertyInfo is null while Getting generic types of Property.");
        }

        return propertyInfo.PropertyType.GetGenericArguments();
    }


    /// <summary>
    /// Find and Return a mehtod that defined in a class by it's name.
    /// </summary>
    /// <param name="type">Type of class include requested method.</param>
    /// <param name="methodName">Name of requested method as string.</param>
    /// <returns>Method info of requested method.</returns>
    internal static MethodInfo? GetMethodByItsName(this Type type, string methodName)
    {
        if (type == null)
        {
            throw new ArgumentNullException("Parameter type is null while Getting method from it.");
        }

        if (methodName == null || methodName == "")
        {
            throw new ArgumentNullException("Parameter methodName is null while Getting method from Type type.");
        }

        return type.GetMethod(methodName);
    }


    /// <summary>
    /// Create <see cref="IList"/> of given type.
    /// </summary>
    /// <param name="type">Type that you want to make a List of it.</param>
    /// <returns>Returns IList of given type.</returns>
    internal static IList CreateIListOfType(this Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException("Parameter type is null while creating List<type>.");
        }

        Type listType = typeof(List<>);
        Type constructedListType = listType.MakeGenericType(type);
        return (IList)Activator.CreateInstance(constructedListType)!;
    }





    /// <summary>
    /// Returns the part of value of <see cref="HtmlNode"/> you want as .
    /// </summary>
    /// <param name="htmlNode">A htmlNode instance.</param>
    /// <param name="xPathAttribute">Attribute that includes ReturnType</param>
    /// <returns>String that choosen from HtmlNode as result.</returns>
    internal static T GetNodeValueBasedOnXPathReturnType<T>(HtmlNode htmlNode, XPathAttribute xPathAttribute)
    {
        if (htmlNode == null)
        {
            throw new ArgumentNullException("parameter html node is null");
        }

        if (xPathAttribute == null)
        {
            throw new ArgumentNullException("parameter xpathAttribute is null");
        }

        return (T)Convert.ChangeType(GetHtmlForEncapsulation(htmlNode, xPathAttribute.NodeReturnType), typeof(T));
    }


    /// <summary>
    /// Returns parts of values of <see cref="HtmlNode"/> you want as <see cref="IList{T}"/>.
    /// </summary>
    /// <param name="htmlNodeCollection"><see cref="HtmlNodeCollection"/> that you want to retrive each <see cref="HtmlNode"/> value.</param>
    /// <param name="xPathAttribute">A <see cref="XPathAttribute"/> instnce incules <see cref="ReturnType"/>.</param>
    /// <param name="listGenericType">Type of IList generic you want.</param>
    /// <returns></returns>
    internal static IList GetNodesValuesBasedOnXPathReturnType(HtmlNodeCollection htmlNodeCollection, XPathAttribute xPathAttribute, Type listGenericType)
    {
        if (htmlNodeCollection == null || htmlNodeCollection.Count == 0)
        {
            throw new ArgumentNullException("parameter htmlNodeCollection is null or empty.");
        }

        if (xPathAttribute == null)
        {
            throw new ArgumentNullException("parameter xpathAttribute is null");
        }


        IList result = listGenericType.CreateIListOfType();
        foreach (HtmlNode node in htmlNodeCollection)
        {
            result.Add(Convert.ChangeType(GetHtmlForEncapsulation(node, xPathAttribute.NodeReturnType), listGenericType));
        }
        return result;
    }


    /// <summary>
    /// Check if the type can instantiated.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    internal static bool IsInstantiable(this Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException("type is null");
        }


        // checking for having parameterless constructor.
        if (type.GetConstructor(Type.EmptyTypes) == null)
        {
            return false;
        }
        else
        {
            return true;
        }


    }

    /// <summary>
    /// Return html part of <see cref="HtmlNode"/> based on <see cref="ReturnType"/>
    /// </summary>
    /// <param name="node">A htmlNode instance.</param>
    /// <param name="returnType"><see cref="ReturnType"/></param>
    /// <returns>Html part</returns>
    /// <exception cref="IndexOutOfRangeException">Out of range to the <see cref="ReturnType"/></exception>
    internal static string GetHtmlForEncapsulation(HtmlNode node, ReturnType returnType)
    {
        switch (returnType)
        {
            case ReturnType.InnerText:
                return node.InnerText;
            case ReturnType.InnerHtml:
                return node.InnerHtml;
            case ReturnType.OuterHtml:
                return node.OuterHtml;
            default:
                throw new InvalidNodeReturnTypeException(string.Format("Invalid ReturnType value {0}", returnType));
        };
    }
}
