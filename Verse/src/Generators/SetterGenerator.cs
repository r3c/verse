using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse.Resolvers;

namespace Verse.Generators
{
	internal static class SetterGenerator
	{
		/// <summary>
		/// Create setter using compatible constructor.
		/// </summary>
		public static Setter<TEntity, TParameter> CreateFromConstructor<TEntity, TParameter>(
			ConstructorInfo constructor)
		{
			var parameters = constructor.GetParameters();

			if (constructor.DeclaringType != typeof(TEntity))
				throw new ArgumentException($"constructor parent type is not {typeof(TEntity)}",
					nameof(constructor));

			if (parameters.Length != 1)
				throw new ArgumentException("constructor doesn't take one argument", nameof(constructor));

			if (parameters[0].ParameterType != typeof(TParameter))
				throw new ArgumentException($"constructor argument type is not {typeof(TParameter)}",
					nameof(constructor));

			var parameterTypes = new[] {typeof(TEntity).MakeByRefType(), typeof(TParameter)};
			var method = new DynamicMethod(string.Empty, null, parameterTypes, constructor.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Newobj, constructor);
			generator.Emit(OpCodes.Stind_Ref);
			generator.Emit(OpCodes.Ret);

			return (Setter<TEntity, TParameter>) method.CreateDelegate(typeof(Setter<TEntity, TParameter>));
		}

		/// <summary>
		/// Create setter from any <see cref="IEnumerable{T}"/> elements to array target.
		/// </summary>
		/// <typeparam name="TElement">Element type</typeparam>
		/// <returns>Setter callback</returns>
		public static Setter<TElement[], IEnumerable<TElement>> CreateFromEnumerable<TElement>()
		{
			var arrayConverter = MethodResolver.Create<Func<IEnumerable<TElement>, TElement[]>>(e => e.ToArray());
			var parameterTypes = new[] {typeof(TElement[]).MakeByRefType(), typeof(IEnumerable<TElement>)};
			var method = new DynamicMethod(string.Empty, null, parameterTypes, typeof(TElement).Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Call, arrayConverter.Method);
			generator.Emit(OpCodes.Stind_Ref);
			generator.Emit(OpCodes.Ret);

			return (Setter<TElement[], IEnumerable<TElement>>) method.CreateDelegate(
				typeof(Setter<TElement[], IEnumerable<TElement>>));
		}

		/// <Summary>
		/// Create field setter delegate for given runtime field.
		/// </Summary>
		public static Setter<TEntity, TField> CreateFromField<TEntity, TField>(FieldInfo field)
		{
			if (field.DeclaringType != typeof(TEntity))
				throw new ArgumentException($"field declaring type is not {typeof(TEntity)}", nameof(field));

			if (field.FieldType != typeof(TField))
				throw new ArgumentException($"field type is not {typeof(TField)}", nameof(field));

			var parentType = typeof(TEntity);
			var parameterTypes = new[] {parentType.MakeByRefType(), typeof(TField)};
			var method = new DynamicMethod(string.Empty, null, parameterTypes, field.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);

			if (!parentType.IsValueType)
				generator.Emit(OpCodes.Ldind_Ref);

			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Stfld, field);
			generator.Emit(OpCodes.Ret);

			return (Setter<TEntity, TField>) method.CreateDelegate(typeof(Setter<TEntity, TField>));
		}

		/// <Summary>
		/// Create property setter delegate for given runtime property.
		/// </Summary>
		public static Setter<TEntity, TProperty> CreateFromProperty<TEntity, TProperty>(PropertyInfo property)
		{
			if (property.DeclaringType != typeof(TEntity))
				throw new ArgumentException($"property declaring type is not {typeof(TEntity)}", nameof(property));

			if (property.PropertyType != typeof(TProperty))
				throw new ArgumentException($"property type is not {typeof(TProperty)}", nameof(property));

			var setter = property.GetSetMethod();

			if (setter == null)
				throw new ArgumentException("property has no setter", nameof(property));

			var parentType = typeof(TEntity);
			var parameterTypes = new[] {parentType.MakeByRefType(), typeof(TProperty)};
			var method = new DynamicMethod(string.Empty, null, parameterTypes, property.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);

			if (!parentType.IsValueType)
				generator.Emit(OpCodes.Ldind_Ref);

			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Call, property.GetSetMethod());
			generator.Emit(OpCodes.Ret);

			return (Setter<TEntity, TProperty>) method.CreateDelegate(typeof(Setter<TEntity, TProperty>));
		}
	}
}
