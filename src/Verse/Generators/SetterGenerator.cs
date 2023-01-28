using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Verse.Generators;

internal static class SetterGenerator
{
    /// <Summary>
    /// Create field setter delegate for given runtime field.
    /// </Summary>
    public static Func<TEntity, TField, TEntity> CreateFromField<TEntity, TField>(FieldInfo field)
    {
        if (field.DeclaringType != typeof(TEntity))
            throw new ArgumentException($"field declaring type is not {typeof(TEntity)}", nameof(field));

        if (field.FieldType != typeof(TField))
            throw new ArgumentException($"field type is not {typeof(TField)}", nameof(field));

        var parentType = typeof(TEntity);
        var parameterTypes = new[] { parentType, typeof(TField) };
        var method = new DynamicMethod(string.Empty, parentType, parameterTypes, field.Module, true);
        var generator = method.GetILGenerator();

        if (parentType.IsValueType)
            generator.Emit(OpCodes.Ldarga_S, 0);
        else
            generator.Emit(OpCodes.Ldarg_0);

        generator.Emit(OpCodes.Ldarg_1);
        generator.Emit(OpCodes.Stfld, field);
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Ret);

        return (Func<TEntity, TField, TEntity>) method.CreateDelegate(typeof(Func<TEntity, TField, TEntity>));
    }

    /// <Summary>
    /// Create property setter delegate for given runtime property.
    /// </Summary>
    public static Func<TEntity, TProperty, TEntity> CreateFromProperty<TEntity, TProperty>(PropertyInfo property)
    {
        if (property.DeclaringType != typeof(TEntity))
            throw new ArgumentException($"property declaring type is not {typeof(TEntity)}", nameof(property));

        if (property.PropertyType != typeof(TProperty))
            throw new ArgumentException($"property type is not {typeof(TProperty)}", nameof(property));

        var setter = property.SetMethod;

        if (setter is null)
            throw new ArgumentException("property has no setter", nameof(property));

        var parentType = typeof(TEntity);
        var parameterTypes = new[] { parentType, typeof(TProperty) };
        var method = new DynamicMethod(string.Empty, parentType, parameterTypes, property.Module, true);
        var generator = method.GetILGenerator();

        if (parentType.IsValueType)
            generator.Emit(OpCodes.Ldarga_S, 0);
        else
            generator.Emit(OpCodes.Ldarg_0);

        generator.Emit(OpCodes.Ldarg_1);
        generator.Emit(OpCodes.Call, setter);
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Ret);

        return (Func<TEntity, TProperty, TEntity>) method.CreateDelegate(typeof(Func<TEntity, TProperty, TEntity>));
    }
}