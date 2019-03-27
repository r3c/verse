using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Verse.Tools
{
	static class Resolver
	{
	    public static MethodInfo Method<TCaller>(Expression<TCaller> lambda, Type[] callerParameters,
	        Type[] methodParameters)
	    {
	        var expression = lambda.Body as MethodCallExpression;

	        if (expression == null)
	            throw new ArgumentException("can't get method information from expression", nameof(lambda));

	        var method = expression.Method;

	        // Change method generic parameters if requested
	        if (methodParameters != null && method.IsGenericMethod)
	        {
	            method = method.GetGenericMethodDefinition();

	            if (methodParameters.Length > 0)
	                method = method.MakeGenericMethod(methodParameters);
	        }

	        // Change target generic parameters if requested
	        if (callerParameters != null && method.DeclaringType.IsGenericType)
	        {
	            var type = method.DeclaringType.GetGenericTypeDefinition();

	            if (callerParameters.Length > 0)
	                type = type.MakeGenericType(callerParameters);

	            method = Array.Find(
	                type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
	                                BindingFlags.Static), (m) => m.MetadataToken == method.MetadataToken);
	        }

	        return method;
	    }
	}
}