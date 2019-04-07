using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Verse.Tools
{
	static class Resolver
	{
		/// <Summary>
		/// Replace type arguments of given generic method by another set of type arguments.
		/// </Summary>
		public static MethodInfo ChangeGenericMethodArguments(MethodInfo method, params Type[] arguments)
		{
			// Change method generic parameters if requested
			if (!method.IsGenericMethod)
				throw new ArgumentException("method is not generic", nameof(method));

			if (method.GetGenericArguments().Length != arguments.Length)
				throw new ArgumentException($"method doesn't have {arguments.Length} generic argument(s)", nameof(method));

			return method.GetGenericMethodDefinition().MakeGenericMethod(arguments);
		}

		/// <Summary>
		/// Resolve method information from given compile-type expression.
		/// </Summary>
		public static MethodInfo GetMethod<TMethod>(Expression<TMethod> lambda)
		{
			if (!(lambda.Body is MethodCallExpression expression))
				throw new ArgumentException("can't get method information from expression", nameof(lambda));

			return expression.Method;
		}

		/// <Summary>
		/// Check whether given type is a generic type with one type argument
		/// and has the same type definition than type argument and return its
		/// type argument, e.g. when called with `IEnumerable<object>` as
		/// `TGeneric` check that `type` is `IEnumerable<U>` and returns
		/// `typeof(U)` as `first`.
		/// </Summary>
		public static bool HasSameGenericDefinitionThan<TGeneric>(Type type, out Type first)
		{
			var expected = typeof(TGeneric);

			if (!expected.IsGenericType)
				throw new ArgumentOutOfRangeException(nameof(TGeneric), "type argument is not a generic type");

			if (expected.GetGenericArguments().Length != 1)
				throw new ArgumentOutOfRangeException(nameof(TGeneric), "type argument doesn't have one generic type argument");

			var definition = expected.GetGenericTypeDefinition();

			if (!type.IsGenericType || type.GetGenericTypeDefinition() != definition)
			{
				first = default;

				return false;
			}

			var arguments = type.GetGenericArguments();

			if (arguments.Length != 1)
			{
				first = default;

				return false;
			}

			first = arguments[0];

			return true;
		}
	}
}