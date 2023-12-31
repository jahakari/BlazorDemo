﻿using System.Reflection;
using System.Runtime.CompilerServices;

namespace BlazorDemo.Shared.Utility;

public static class AttributeHelpers
{
    public static AttributeValue<TValue?> GetAttributeValueOrDefault<TAttribute, TValue>(this MemberInfo info, Func<TAttribute, TValue?> valueSelector)
        where TAttribute : Attribute
        => GetAttributeValueOrDefault(info, valueSelector, default);

    public static AttributeValue<TValue?> GetAttributeValueOrDefault<TAttribute, TValue>(this MemberInfo info, Func<TAttribute, TValue?> valueSelector,
        TValue? defaultValue) where TAttribute : Attribute
    {
        (bool hasAttribute, TValue? value) = GetAttributeValueOrDefaultInternal(info, valueSelector);
        TValue? result = hasAttribute ? value : defaultValue;

        return new AttributeValue<TValue?>(result);
    }

    private static (bool hasAttribute, TValue? value) GetAttributeValueOrDefaultInternal<TAttribute, TValue>(
        MemberInfo info,
        Func<TAttribute, TValue?> valueSelector,
        [CallerArgumentExpression(nameof(info))] string infoExpression = "",
        [CallerArgumentExpression(nameof(valueSelector))] string valueSelectorExpression = "") where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(info, infoExpression);
        ArgumentNullException.ThrowIfNull(valueSelector, valueSelectorExpression);

        TAttribute? attribute = info.GetCustomAttribute<TAttribute>(true);

        if (attribute is null) {
            return default;
        }

        return (true, valueSelector(attribute));
    }
}
