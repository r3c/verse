using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Verse.Resolvers
{
	readonly struct TypeResolver
	{
		public readonly Type Type;

		public static TypeResolver Create(Type type)
		{
			return new TypeResolver(type);
		}

		private TypeResolver(Type type)
		{
			Type = type;
		}

		/// <Summary>
		/// Check whether type is a generic type with same type definition than
		/// type argument `TGeneric` and return its type arguments, e.g. when
		/// called with `IEnumerable<object>` as `TGeneric` check that type is
		/// `IEnumerable<U>` and returns `{ typeof(U) }` as `arguments`.
		/// </Summary>
		public bool HasSameDefinitionThan<TGeneric>(out Type[] arguments)
		{
			var expected = typeof(TGeneric);

			if (!expected.IsGenericType)
				throw new InvalidOperationException("type is not generic");

			if (expected.GetGenericArguments().Length != 1)
				throw new InvalidOperationException("type doesn't have one generic argument");

			var definition = expected.GetGenericTypeDefinition();

			if (!this.Type.IsGenericType || this.Type.GetGenericTypeDefinition() != definition)
			{
				arguments = default;

				return false;
			}

			arguments = this.Type.GetGenericArguments();

			return true;
		}
	}
}