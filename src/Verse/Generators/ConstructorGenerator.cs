using System;
using System.Reflection;
using System.Reflection.Emit;
using Verse.Exceptions;

namespace Verse.Generators;

internal static class ConstructorGenerator
{
    /// <summary>
    /// Create parameterless constructor function for given entity type.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <returns>Constructor function</returns>
    public static Func<TEntity> CreateConstructor<TEntity>(BindingFlags bindingFlags)
    {
        var entityType = typeof(TEntity);

        var method = new DynamicMethod(string.Empty, entityType, Type.EmptyTypes, entityType.Module, true);
        var generator = method.GetILGenerator();

        if (entityType.IsValueType)
        {
            var instance = generator.DeclareLocal(entityType);

            generator.Emit(OpCodes.Ldloca_S, instance);
            generator.Emit(OpCodes.Initobj, entityType);
            generator.Emit(OpCodes.Ldloc, instance);
        }
        else
        {
            var modifiers = Array.Empty<ParameterModifier>();
            var constructor = entityType.GetConstructor(bindingFlags, null, Type.EmptyTypes, modifiers);

            if (constructor is null)
                throw new ConstructorNotFoundException(entityType);

            generator.Emit(OpCodes.Newobj, constructor);
        }

        generator.Emit(OpCodes.Ret);

        return (Func<TEntity>)method.CreateDelegate(typeof(Func<TEntity>));
    }
}