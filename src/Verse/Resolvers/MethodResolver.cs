using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Verse.Resolvers;

internal readonly struct MethodResolver
{
    private static readonly object NoCaller = new();

    public readonly MethodInfo Method;

    /// <Summary>
    /// Resolve method information from given compile-type expression.
    /// </Summary>
    public static MethodResolver Create<TMethod>(Expression<TMethod> lambda)
    {
        if (lambda.Body is not MethodCallExpression expression)
            throw new ArgumentException("can't get method information from expression", nameof(lambda));

        return new MethodResolver(expression.Method);
    }

    private MethodResolver(MethodInfo method)
    {
        Method = method;
    }

    /// <Summary>
    /// Invoke instance method with given caller instance and arguments.
    /// </Summary>
    public object? InvokeInstance(object caller, params object?[] arguments)
    {
        return Method.Invoke(caller, arguments);
    }

    /// <Summary>
    /// Invoke static method with given arguments.
    /// </Summary>
    public object? InvokeStatic(params object?[] arguments)
    {
        return Method.Invoke(NoCaller, arguments);
    }

    /// <Summary>
    /// Replace type arguments of given generic method by another set of type arguments.
    /// </Summary>
    public MethodResolver SetGenericArguments(params Type[] arguments)
    {
        // Change method generic parameters if requested
        if (!Method.IsGenericMethod)
            throw new InvalidOperationException("method is not generic");

        if (Method.GetGenericArguments().Length != arguments.Length)
            throw new InvalidOperationException($"method doesn't have {arguments.Length} generic argument(s)");

        return new MethodResolver(Method.GetGenericMethodDefinition().MakeGenericMethod(arguments));
    }
}