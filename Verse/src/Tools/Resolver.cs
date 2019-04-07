using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Verse.Tools
{
	static class Resolver
	{
		public static MethodInfo FindMethod<TCaller>(Expression<TCaller> lambda, Type[] typeArguments,
			Type[] methodArguments)
		{
			if (!(lambda.Body is MethodCallExpression expression))
				throw new ArgumentException("can't get method information from expression", nameof(lambda));

			var method = expression.Method;

			// Change method generic parameters if requested
			if (methodArguments != null && method.IsGenericMethod)
			{
				method = method.GetGenericMethodDefinition();

				if (methodArguments.Length > 0)
					method = method.MakeGenericMethod(methodArguments);
			}

			// Change target generic parameters if requested
			if (typeArguments != null && method.DeclaringType.IsGenericType)
			{
				var type = method.DeclaringType.GetGenericTypeDefinition();

				if (typeArguments.Length > 0)
					type = type.MakeGenericType(typeArguments);

				method = Array.Find(
					type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
									BindingFlags.Static), (m) => m.MetadataToken == method.MetadataToken);
			}

			return method;
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