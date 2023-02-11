using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Verse.Resolvers;

internal readonly struct ConstructorResolver
{
    public readonly ConstructorInfo Constructor;

    /// <Summary>
    /// Resolve constructor information from given compile-type expression.
    /// </Summary>
    public static ConstructorResolver Create<TConstructor>(Expression<TConstructor> lambda)
    {
        if (lambda.Body is not NewExpression expression || expression.Constructor is null)
            throw new ArgumentException("can't get constructor information from expression", nameof(lambda));

        return new ConstructorResolver(expression.Constructor);
    }

    private ConstructorResolver(ConstructorInfo constructor)
    {
        Constructor = constructor;
    }

    public ConstructorResolver SetTypeGenericArguments(params Type[] arguments)
    {
        var declaringType = Constructor.DeclaringType;

        if (declaringType is null || !declaringType.IsGenericType)
            throw new InvalidOperationException("declaring type is not generic");

        if (declaringType.GetGenericArguments().Length != arguments.Length)
            throw new InvalidOperationException($"declaring type doesn't have {arguments.Length} generic argument(s)");

        var genericType = declaringType.GetGenericTypeDefinition().MakeGenericType(arguments);
        var metadataToken = Constructor.MetadataToken;
        var constructor = genericType.GetConstructors().FirstOrDefault(c => c.MetadataToken == metadataToken);

        if (constructor is null)
            throw new InvalidOperationException("failed converting constructor to target generic type");

        return new ConstructorResolver(constructor);
    }
}