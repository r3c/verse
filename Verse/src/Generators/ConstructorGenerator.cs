using System;
using System.Reflection;
using System.Reflection.Emit;
using Verse.Exceptions;

namespace Verse.Generators
{
	internal static class ConstructorGenerator
	{
		/// <summary>
		/// Create parameterless constructor function for given entity type.
		/// </summary>
		/// <typeparam name="TEntity">Entity type</typeparam>
		/// <returns>Constructor function</returns>
		public static Func<TEntity> CreateConstructor<TEntity>(BindingFlags bindingFlags)
		{
			var entityType = typeof(TEntity);
			var constructor = entityType.GetConstructor(bindingFlags, Type.DefaultBinder, Type.EmptyTypes,
				Array.Empty<ParameterModifier>());

			if (constructor == null)
				throw new ConstructorNotFoundException(entityType);

			var method = new DynamicMethod(string.Empty, entityType, Type.EmptyTypes, constructor.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Newobj, constructor);
			generator.Emit(OpCodes.Ret);

			return (Func<TEntity>) method.CreateDelegate(typeof(Func<TEntity>));
		}
	}
}
