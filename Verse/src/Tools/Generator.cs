using System;
using System.Reflection;
using System.Reflection.Emit;

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

			return (Func<T>)Generator.Create(constructor);
		}

		private static object Create(ConstructorInfo constructor)
		{
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

			var generics = new Type[parameters.Length + 1];

			generics[0] = constructor.DeclaringType;

			for (index = 0; index < parameters.Length; ++index)
				generics[index + 1] = parameters[index].ParameterType;

			return method.CreateDelegate(Functions[parameters.Length].MakeGenericType(generics));
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