using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Verse.Resolvers;

internal readonly struct PropertyResolver
{
    private readonly PropertyInfo _property;

    /// <Summary>
    /// Resolve method information from given compile-type expression.
    /// </Summary>
    public static PropertyResolver Create<TProperty>(Expression<TProperty> lambda)
    {
        if (lambda.Body is not MemberExpression expression)
            throw new ArgumentException("can't get property information from expression", nameof(lambda));

        if (expression.Member is not PropertyInfo property)
            throw new ArgumentException("member expression doesn't target a property", nameof(lambda));

        return new PropertyResolver(property);
    }

    private PropertyResolver(PropertyInfo property)
    {
        _property = property;
    }

    public object? GetGetter(object instance)
    {
        var method = _property.GetMethod;

        if (method == null)
            throw new InvalidOperationException("property has no getter");

        return method.Invoke(instance, []);
    }

    public PropertyResolver SetCallerGenericArguments(params Type[] arguments)
    {
        var callerType = _property.DeclaringType ??
                         throw new InvalidOperationException("property has no declaring type");

        if (!callerType.IsGenericType)
            throw new InvalidOperationException("property caller type is not generic");

        if (callerType.GetGenericArguments().Length != arguments.Length)
            throw new InvalidOperationException(
                $"property caller type doesn't have {arguments.Length} generic argument(s)");

        var metadataToken = _property.MetadataToken;
        var property = callerType.GetGenericTypeDefinition().MakeGenericType(arguments)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(p => p.MetadataToken == metadataToken);

        if (property == null)
            throw new InvalidOperationException("could not find property in modified caller type");

        return new PropertyResolver(property);
    }
}