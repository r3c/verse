using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Verse.Resolvers
{
	readonly struct MethodResolver
	{
		public readonly MethodInfo Method;

		/// <Summary>
		/// Resolve method information from given compile-type expression.
		/// </Summary>
		public static MethodResolver Create<TMethod>(Expression<TMethod> lambda)
		{
			if (!(lambda.Body is MethodCallExpression expression))
				throw new ArgumentException("can't get method information from expression", nameof(lambda));

			return new MethodResolver(expression.Method);
		}

		private MethodResolver(MethodInfo method)
		{
			this.Method = method;
		}

		/// <Summary>
		/// Invoke method with given caller instance and arguments.
		/// </Summary>
		public object Invoke(object caller, params object[] arguments)
		{
			return this.Method.Invoke(caller, arguments);
		}

		/// <Summary>
		/// Replace type arguments of given generic method by another set of type arguments.
		/// </Summary>
		public MethodResolver SetGenericArguments(params Type[] arguments)
		{
			// Change method generic parameters if requested
			if (!this.Method.IsGenericMethod)
				throw new InvalidOperationException("method is not generic");

			if (this.Method.GetGenericArguments().Length != arguments.Length)
				throw new InvalidOperationException($"method doesn't have {arguments.Length} generic argument(s)");

			return new MethodResolver(this.Method.GetGenericMethodDefinition().MakeGenericMethod(arguments));
		}
	}
}