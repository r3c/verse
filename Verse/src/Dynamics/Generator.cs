using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Verse.Dynamics
{
	static class Generator
	{
		#region Attributes
		
		private static readonly ParameterModifier[]	modifiers = new ParameterModifier[0];

		#endregion

		#region Methods

		public static Func<T>	Constructor<T> ()
		{
			ConstructorInfo	constructor;
			ILGenerator		generator;
			DynamicMethod	method;
			Type			type;

			type = typeof (T);
			constructor = type.GetConstructor (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.DefaultBinder, Type.EmptyTypes, Generator.modifiers);

			if (constructor == null)
				return () => default (T);

			method = new DynamicMethod (string.Empty, type, Type.EmptyTypes, constructor.Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Newobj, constructor);
			generator.Emit (OpCodes.Ret);

			return (Func<T>)method.CreateDelegate (typeof (Func<T>));
		}

		#endregion
	}
}
