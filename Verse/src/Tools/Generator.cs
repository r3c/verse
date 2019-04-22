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
		private static readonly OpCode[] OpCodeLdArgs =
			{OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3};

		private static readonly Type[] Functions =
			{typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>)};

		public static Func<T> CreateConstructor<T>()
		{
			var constructor = typeof(T).GetConstructor(
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.DefaultBinder,
				Type.EmptyTypes, Array.Empty<ParameterModifier>());

			if (constructor == null)
				return () => default;

			var parameters = constructor.GetParameters();

			if (parameters.Length >= Generator.Functions.Length)
				throw new ArgumentOutOfRangeException(nameof(constructor),
					$"can't generate constructor with more than {Generator.Functions.Length} arguments");

			var method = new DynamicMethod(string.Empty, constructor.DeclaringType,
				Array.ConvertAll(parameters, (p) => p.ParameterType), constructor.Module, true);
			var generator = method.GetILGenerator();
			var index = 0;

			foreach (var parameter in parameters)
				Generator.LoadParameter(generator, parameter.ParameterType, index++);

			generator.Emit(OpCodes.Newobj, constructor);
			generator.Emit(OpCodes.Ret);

			var arguments = new[] {constructor.DeclaringType}
				.Concat(parameters.Select(p => p.ParameterType))
				.ToArray();

			var type = Generator.Functions[parameters.Length].MakeGenericType(arguments);

			return (Func<T>) method.CreateDelegate(type);
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
		public static object CreateFieldGetter(FieldInfo field)
		{
			var parentType = field.DeclaringType;
			var method = new DynamicMethod(string.Empty, field.FieldType, new[] {parentType}, field.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, field);
			generator.Emit(OpCodes.Ret);

			var methodType = typeof(Func<,>).MakeGenericType(parentType, field.FieldType);

			return method.CreateDelegate(methodType);
		}

		/// <Summary>
		/// Create field setter delegate for given runtime field.
		/// </Summary>
		public static object CreateFieldSetter(FieldInfo field)
		{
			var parentType = field.DeclaringType;
			var method = new DynamicMethod(string.Empty, null, new[] {parentType.MakeByRefType(), field.FieldType},
				field.Module, true);
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

		private static void LoadParameter(ILGenerator generator, Type type, int index)
		{
			if (type.IsByRef)
				generator.Emit(index < 256 ? OpCodes.Ldarga_S : OpCodes.Ldarga, index);
			else if (index < Generator.OpCodeLdArgs.Length)
				generator.Emit(Generator.OpCodeLdArgs[index]);
			else
				generator.Emit(index < 256 ? OpCodes.Ldarg_S : OpCodes.Ldarg, index);
		}
	}
}
