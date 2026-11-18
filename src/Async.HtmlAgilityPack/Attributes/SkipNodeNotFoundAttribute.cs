// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

namespace Async.HtmlAgilityPack.Attributes;

/// <summary>
/// Tagging a property with this Attribute make Encapsulator to ignore that property if it causes an error.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class SkipNodeNotFoundAttribute : Attribute
{

}
