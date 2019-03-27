using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Verse.Tools
{
	static class Generator
	{
		private static readonly ParameterModifier[] EmptyModifiers = new ParameterModifier[0];

	    private static readonly OpCode[] OpCodeLdArgs =
	        {OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3};

	    public static Func<T> Constructor<T>()
	    {
	        var constructor = typeof(T).GetConstructor(
	            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.DefaultBinder,
	            Type.EmptyTypes, Generator.EmptyModifiers);

	        if (constructor == null)
	            return () => default;

	        return (Func<T>) Generator.Create(constructor);
	    }

	    private static object Create(ConstructorInfo constructor)
	    {
	        Type type;

	        var parameters = constructor.GetParameters();

	        switch (parameters.Length)
	        {
	            case 0:
	                type = typeof(Func<>);

	                break;

	            case 1:
	                type = typeof(Func<,>);

	                break;

	            case 2:
	                type = typeof(Func<,,>);

	                break;

	            case 3:
	                type = typeof(Func<,,,>);

	                break;

	            case 4:
	                type = typeof(Func<,,,,>);

	                break;

	            default:
	                throw new ArgumentOutOfRangeException(nameof(constructor),
	                    "can't generate constructor with more than 4 arguments");
	        }

	        var method = new DynamicMethod(string.Empty, constructor.DeclaringType,
	            Array.ConvertAll(parameters, (p) => p.ParameterType), constructor.Module, true);
	        var generator = method.GetILGenerator();
	        var index = 0;

	        foreach (ParameterInfo parameter in parameters)
	        {
	            if (parameter.ParameterType.IsByRef)
	                generator.Emit(index < 256 ? OpCodes.Ldarga_S : OpCodes.Ldarga, index);
	            else if (index < Generator.OpCodeLdArgs.Length)
	                generator.Emit(Generator.OpCodeLdArgs[index]);
	            else
	                generator.Emit(index < 256 ? OpCodes.Ldarg_S : OpCodes.Ldarg, index);

	            ++index;
	        }

	        generator.Emit(OpCodes.Newobj, constructor);
	        generator.Emit(OpCodes.Ret);

	        var generics = new Type[parameters.Length + 1];

	        generics[0] = constructor.DeclaringType;

	        for (index = 0; index < parameters.Length; ++index)
	            generics[index + 1] = parameters[index].ParameterType;

	        return method.CreateDelegate(type.MakeGenericType(generics));
	    }
	}
}