using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Verse.Generators
{
	internal static class ConstructorGenerator
	{
		/// <summary>
		/// Create parameterless constructor function for given entity type.
		/// </summary>
		/// <typeparam name="TEntity">Entity type</typeparam>
		/// <returns>Constructor function</returns>
		public static Func<TEntity> CreateConstructor<TEntity>()
		{
			const BindingFlags bindings = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

			var constructor = typeof(TEntity).GetConstructor(bindings, Type.DefaultBinder, Type.EmptyTypes,
				Array.Empty<ParameterModifier>());

			if (constructor == null)
				return () => default;

			var method = new DynamicMethod(string.Empty, typeof(TEntity), Type.EmptyTypes, constructor.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Newobj, constructor);
			generator.Emit(OpCodes.Ret);

			return (Func<TEntity>) method.CreateDelegate(typeof(Func<TEntity>));
		}
	}
}
