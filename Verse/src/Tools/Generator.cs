using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Verse.Tools
{
	static class Generator
	{
		#region Attributes

		private static readonly ParameterModifier[]	emptyModifiers = new ParameterModifier[0];

		private static readonly OpCode[]			opCodeLdargs = new [] {OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3};

		#endregion

		#region Methods / Public

		public static Func<R> Constructor<R> (ConstructorInfo constructor)
		{
			return (Func<R>)Generator.Constructor (constructor);
		}

		public static Func<T, R> Constructor<T, R> (ConstructorInfo constructor)
		{
			return (Func<T, R>)Generator.Constructor (constructor);
		}

		public static Func<T1, T2, R> Constructor<T1, T2, R> (ConstructorInfo constructor)
		{
			return (Func<T1, T2, R>)Generator.Constructor (constructor);
		}

		public static Func<T1, T2, T3, R> Constructor<T1, T2, T3, R> (ConstructorInfo constructor)
		{
			return (Func<T1, T2, T3, R>)Generator.Constructor (constructor);
		}

		public static Func<T1, T2, T3, T4, R> Constructor<T1, T2, T3, T4, R> (ConstructorInfo constructor)
		{
			return (Func<T1, T2, T3, T4, R>)Generator.Constructor (constructor);
		}

		public static Func<R> ConstructorDefault<R> ()
		{
			ConstructorInfo	constructor;

			constructor = typeof (R).GetConstructor (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.DefaultBinder, Type.EmptyTypes, Generator.emptyModifiers);

			if (constructor == null)
				return () => default (R);

			return Generator.Constructor<R> (constructor);
		}

		#endregion

		#region Methods / Private

		private static object Constructor (ConstructorInfo constructor)
		{
			ILGenerator		generator;
			Type[]			generics;
			int				index;
			DynamicMethod	method;
			ParameterInfo[]	parameters;
			Type			type;

			parameters = constructor.GetParameters ();

			switch (parameters.Length)
			{
				case 0:
					type = typeof (Func<>);

					break;

				case 1:
					type = typeof (Func<, >);

					break;

				case 2:
					type = typeof (Func<, , >);

					break;

				case 3:
					type = typeof (Func<, , , >);

					break;

				case 4:
					type = typeof (Func<, , , , >);

					break;

				default:
					throw new ArgumentOutOfRangeException ("constructor", "can't generate constructor with more than 4 arguments");
			}

			method = new DynamicMethod (string.Empty, constructor.DeclaringType, Array.ConvertAll (parameters, (p) => p.ParameterType), constructor.Module, true);

			generator = method.GetILGenerator ();
			index = 0;

			foreach (ParameterInfo parameter in parameters)
			{
				if (parameter.ParameterType.IsByRef)
					generator.Emit (index < 256 ? OpCodes.Ldarga_S : OpCodes.Ldarga, index);
				else if (index < Generator.opCodeLdargs.Length)
					generator.Emit (Generator.opCodeLdargs[index]);
				else
					generator.Emit (index < 256 ? OpCodes.Ldarg_S : OpCodes.Ldarg, index);

				++index;
			}

			generator.Emit (OpCodes.Newobj, constructor);
			generator.Emit (OpCodes.Ret);

			generics = new Type[parameters.Length + 1];
			generics[0] = constructor.DeclaringType;

			for (index = 0; index < parameters.Length; ++index)
				generics[index + 1] = parameters[index].ParameterType;

			return method.CreateDelegate (type.MakeGenericType (generics));
		}

		#endregion
	}
}
