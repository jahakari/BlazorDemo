﻿using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace BlazorDemo.Shared.Utility;

public static class TypeHelpers
{
    private static readonly ImmutableArray<Type> _numericTypes = new[]
    {
        typeof(byte),
        typeof(sbyte),
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(short),
        typeof(ushort)
    }.ToImmutableArray();

    private static readonly ImmutableArray<Type> _dateTypes = new[]
    {
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(DateOnly)
    }.ToImmutableArray();

    public static bool Is<T>(this Type type) => typeof(T) == GetType(type);

    public static bool IsNumeric(this Type type) => _numericTypes.Contains(GetType(type));

    public static bool IsDate(this Type type) => _dateTypes.Contains(GetType(type));

    public static Type GetType<T>() => GetType(typeof(T));

    public static Type GetType(Type type, [CallerArgumentExpression(nameof(type))] string expression = "")
    {
        ArgumentNullException.ThrowIfNull(type, expression);

        return Nullable.GetUnderlyingType(type) ?? type;
    }
}