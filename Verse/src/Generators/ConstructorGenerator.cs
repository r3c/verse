using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Verse.Generators
{
	static class ConstructorGenerator
	{
		#region Attributes
		
		private static readonly ParameterModifier[]	modifiers = new ParameterModifier[0];

		#endregion

		#region Methods

		public static Func<T>	Generate<T> ()
	    {
	    	ConstructorInfo	constructor;
	    	ILGenerator		generator;
	        DynamicMethod	method;
	        Type			type;

        	type = typeof (T);
        	constructor = type.GetConstructor (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.DefaultBinder, Type.EmptyTypes, ConstructorGenerator.modifiers);

			if (constructor == null)
				return () => default (T);

			method = new DynamicMethod (string.Empty, type, Type.EmptyTypes, constructor.DeclaringType);

	        generator = method.GetILGenerator ();
	        generator.Emit (OpCodes.Newobj, constructor);
	        generator.Emit (OpCodes.Ret);

	        return (Func<T>)method.CreateDelegate (typeof (Func<T>));
	    }

	    #endregion
	}
}
