using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse.Resolvers;

namespace Verse.Tools
{
	internal static class Generator
	{
		/// <summary>
		/// Create parameterless constructor function for given entity type.
		/// </summary>
		/// <typeparam name="TEntity">Entity type</typeparam>
		/// <returns>Constructor function</returns>
		public static Func<TEntity> CreateConstructor<TEntity>(BindingFlags bindings)
		{
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

		/// <summary>
		/// Create setter from any <see cref="IEnumerable{T}"/> elements to array target.
		/// </summary>
		/// <typeparam name="TElement">Element type</typeparam>
		/// <returns>Setter callback</returns>
		public static Setter<TElement[], IEnumerable<TElement>> CreateSetterFromArrayConverter<TElement>()
		{
			var converter = MethodResolver.Create<Func<IEnumerable<TElement>, TElement[]>>(e => e.ToArray());
			var parameterTypes = new[] {typeof(TElement[]).MakeByRefType(), typeof(IEnumerable<TElement>)};

			var method = new DynamicMethod(string.Empty, null, parameterTypes, typeof(TElement).Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Call, converter.Method);
			generator.Emit(OpCodes.Stind_Ref);
			generator.Emit(OpCodes.Ret);

			var type = typeof(Setter<TElement[], IEnumerable<TElement>>);

			return (Setter<TElement[], IEnumerable<TElement>>) method.CreateDelegate(type);
		}

		/// <summary>
		/// Create setter using compatible constructor.
		/// </summary>
		public static Setter<TEntity, TParameter> CreateSetterFromConstructor<TEntity, TParameter>(
			ConstructorInfo constructor)
		{
			var parameters = constructor.GetParameters();

			if (parameters.Length != 1)
				throw new ArgumentException("constructor doesn't take one argument", nameof(constructor));

			var entityType = constructor.DeclaringType;

			if (entityType != typeof(TEntity))
				throw new ArgumentException($"constructor parent type is not {typeof(TEntity)}",
					nameof(constructor));

			var parameterType = parameters[0].ParameterType;

			if (parameterType != typeof(TParameter))
				throw new ArgumentException($"constructor argument type is not {typeof(TParameter)}",
					nameof(constructor));

			var parameterTypes = new[] {entityType.MakeByRefType(), parameterType};
			var method = new DynamicMethod(string.Empty, null, parameterTypes, constructor.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Newobj, constructor);
			generator.Emit(OpCodes.Stind_Ref);
			generator.Emit(OpCodes.Ret);

			var type = typeof(Setter<,>).MakeGenericType(entityType, parameterType);

			return (Setter<TEntity, TParameter>) method.CreateDelegate(type);
		}

		/// <Summary>
		/// Create field getter delegate for given runtime field.
		/// </Summary>
		public static Func<TEntity, TField> CreateFieldGetter<TEntity, TField>(FieldInfo field)
		{
			if (field.DeclaringType != typeof(TEntity))
				throw new ArgumentException($"field declaring type is not {typeof(TEntity)}", nameof(field));

			if (field.FieldType != typeof(TField))
				throw new ArgumentException($"field type is not {typeof(TField)}", nameof(field));

			var parentType = field.DeclaringType;
			var parameterTypes = new[] {parentType};
			var method = new DynamicMethod(string.Empty, field.FieldType, parameterTypes, field.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, field);
			generator.Emit(OpCodes.Ret);

			return (Func<TEntity, TField>) method.CreateDelegate(typeof(Func<TEntity, TField>));
		}

		/// <Summary>
		/// Create field setter delegate for given runtime field.
		/// </Summary>
		public static object CreateFieldSetter(FieldInfo field)
		{
			var parentType = field.DeclaringType;
			var parameterTypes = new[] {parentType.MakeByRefType(), field.FieldType};
			var method = new DynamicMethod(string.Empty, null, parameterTypes, field.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);

			if (!parentType.IsValueType)
				generator.Emit(OpCodes.Ldind_Ref);

			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Stfld, field);
			generator.Emit(OpCodes.Ret);

			var methodType = typeof(Setter<,>).MakeGenericType(parentType, field.FieldType);

			return method.CreateDelegate(methodType);
		}

		/// <Summary>
		/// Create property getter delegate for given runtime property.
		/// </Summary>
		public static object CreatePropertyGetter(PropertyInfo property)
		{
			var getter = property.GetGetMethod();

			if (getter == null)
				throw new ArgumentException("property has no getter", nameof(property));

			var methodType = typeof(Func<,>).MakeGenericType(property.DeclaringType, property.PropertyType);

			return Delegate.CreateDelegate(methodType, getter);
		}

		/// <Summary>
		/// Create property setter delegate for given runtime property.
		/// </Summary>
		public static object CreatePropertySetter(PropertyInfo property)
		{
			var setter = property.GetSetMethod();

			if (setter == null)
				throw new ArgumentException("property has no setter", nameof(property));

			var parentType = property.DeclaringType;
			var method = new DynamicMethod(string.Empty, null,
				new[] {parentType.MakeByRefType(), property.PropertyType}, property.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);

			if (!parentType.IsValueType)
				generator.Emit(OpCodes.Ldind_Ref);

			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Call, property.GetSetMethod());
			generator.Emit(OpCodes.Ret);

			var methodType = typeof(Setter<,>).MakeGenericType(parentType, property.PropertyType);

			return method.CreateDelegate(methodType);
		}
	}
}
