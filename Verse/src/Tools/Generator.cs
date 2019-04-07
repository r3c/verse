using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse.Resolvers;

namespace Verse.Tools
{
	static class Generator
	{
		private static readonly OpCode[] OpCodeLdArgs = { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 };
		private static readonly Type[] Functions = { typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>) };

		public static Func<T> CreateConstructor<T>()
		{
			var constructor = typeof(T).GetConstructor(
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.DefaultBinder,
				Type.EmptyTypes, Array.Empty<ParameterModifier>());

			if (constructor == null)
				return () => default;

			var parameters = constructor.GetParameters();

			if (parameters.Length >= Functions.Length)
				throw new ArgumentOutOfRangeException(nameof(constructor), $"can't generate constructor with more than {Functions.Length} arguments");

			var method = new DynamicMethod(string.Empty, constructor.DeclaringType,
				Array.ConvertAll(parameters, (p) => p.ParameterType), constructor.Module, true);
			var generator = method.GetILGenerator();
			var index = 0;

			foreach (ParameterInfo parameter in parameters)
				LoadParameter(generator, parameter, index++);

			generator.Emit(OpCodes.Newobj, constructor);
			generator.Emit(OpCodes.Ret);

			var arguments = new[] { constructor.DeclaringType }
				.Concat(parameters.Select(p => p.ParameterType))
				.ToArray();

			var type = Functions[parameters.Length].MakeGenericType(arguments);

			return (Func<T>)method.CreateDelegate(type);
		}

		/// <summary>
		/// Create DecoderAssign<T, U> delegate using compatible constructor.
		/// </summary>
		public static object CreateConstructorSetter(ConstructorInfo constructor)
		{
			var parameters = constructor.GetParameters();

			if (parameters.Length != 1)
				throw new ArgumentException("constructor doesn't take one argument", nameof(constructor));

			var parameterType = parameters[0].ParameterType;
			var caller = constructor.DeclaringType;
			var method = new DynamicMethod(string.Empty, null, new[] { caller.MakeByRefType(), parameterType }, constructor.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Newobj, constructor);

			if (caller.IsValueType)
				generator.Emit(OpCodes.Stobj, caller);
			else
				generator.Emit(OpCodes.Stind_Ref);

			generator.Emit(OpCodes.Ret);

			var type = typeof(DecodeAssign<,>).MakeGenericType(caller, parameterType);

			return method.CreateDelegate(type);
		}

		/// <Summary>
		/// Create field getter delegate for given runtime field.
		/// </Summary>
		public static object CreateFieldGetter(FieldInfo field)
		{
			var parentType = field.DeclaringType;
			var method = new DynamicMethod(string.Empty, field.FieldType, new[] { parentType }, field.Module, true);
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
			var method = new DynamicMethod(string.Empty, null, new[] { parentType.MakeByRefType(), field.FieldType }, field.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);

			if (!parentType.IsValueType)
				generator.Emit(OpCodes.Ldind_Ref);

			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Stfld, field);
			generator.Emit(OpCodes.Ret);

			var methodType = typeof(DecodeAssign<,>).MakeGenericType(parentType, field.FieldType);

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
			var method = new DynamicMethod(string.Empty, null, new[] { parentType.MakeByRefType(), property.PropertyType }, property.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);

			if (!parentType.IsValueType)
				generator.Emit(OpCodes.Ldind_Ref);

			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Call, property.GetSetMethod());
			generator.Emit(OpCodes.Ret);

			var methodType = typeof(DecodeAssign<,>).MakeGenericType(parentType, property.PropertyType);

			return method.CreateDelegate(methodType);
		}

		private static void LoadParameter(ILGenerator generator, ParameterInfo parameter, int index)
		{
			if (parameter.ParameterType.IsByRef)
				generator.Emit(index < 256 ? OpCodes.Ldarga_S : OpCodes.Ldarga, index);
			else if (index < Generator.OpCodeLdArgs.Length)
				generator.Emit(Generator.OpCodeLdArgs[index]);
			else
				generator.Emit(index < 256 ? OpCodes.Ldarg_S : OpCodes.Ldarg, index);
		}
	}
}