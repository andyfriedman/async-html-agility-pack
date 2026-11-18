// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// Copyright © ZZZ Projects Inc. All rights reserved.
// Copyright © 2026 Andy Friedman. Applies to the .NET 11 modernization changes.

using System.ComponentModel;

namespace HtmlAgilityPack;

internal static class Utilities
{
    public static TValue? GetDictionaryValueOrDefault<TKey, TValue>(
        Dictionary<TKey, TValue> dict,
        TKey key,
        TValue? defaultValue = default)
        where TKey : class
    {
        return dict.TryGetValue(key, out TValue? value) ? value : defaultValue;
    }

    internal static object? To(this object? @this, Type type)
    {
        if (@this is not null)
        {
            if (@this.GetType() == type)
            {
                return @this;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(@this);
            if (converter.CanConvertTo(type))
            {
                return converter.ConvertTo(@this, type);
            }

            converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(@this.GetType()))
            {
                return converter.ConvertFrom(@this);
            }

            if (@this == DBNull.Value)
            {
                return null;
            }
        }

        return @this;
    }
}
