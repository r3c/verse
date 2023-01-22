using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Verse.Generators;

internal static class SetterGenerator
{
    /// <Summary>
    /// Create field setter delegate for given runtime field.
    /// </Summary>
    public static Setter<TEntity, TField> CreateFromField<TEntity, TField>(FieldInfo field)
    {
        if (field.DeclaringType != typeof(TEntity))
            throw new ArgumentException($"field declaring type is not {typeof(TEntity)}", nameof(field));

        if (field.FieldType != typeof(TField))
            throw new ArgumentException($"field type is not {typeof(TField)}", nameof(field));

        var parentType = typeof(TEntity);
        var parameterTypes = new[] {parentType.MakeByRefType(), typeof(TField)};
        var method = new DynamicMethod(string.Empty, null, parameterTypes, field.Module, true);
        var generator = method.GetILGenerator();

        generator.Emit(OpCodes.Ldarg_0);

        if (!parentType.IsValueType)
            generator.Emit(OpCodes.Ldind_Ref);

        generator.Emit(OpCodes.Ldarg_1);
        generator.Emit(OpCodes.Stfld, field);
        generator.Emit(OpCodes.Ret);

        return (Setter<TEntity, TField>) method.CreateDelegate(typeof(Setter<TEntity, TField>));
    }

    /// <Summary>
    /// Create property setter delegate for given runtime property.
    /// </Summary>
    public static Setter<TEntity, TProperty> CreateFromProperty<TEntity, TProperty>(PropertyInfo property)
    {
        if (property.DeclaringType != typeof(TEntity))
            throw new ArgumentException($"property declaring type is not {typeof(TEntity)}", nameof(property));

        if (property.PropertyType != typeof(TProperty))
            throw new ArgumentException($"property type is not {typeof(TProperty)}", nameof(property));

        var setter = property.GetSetMethod();

        if (setter == null)
            throw new ArgumentException("property has no setter", nameof(property));

        var parentType = typeof(TEntity);
        var parameterTypes = new[] {parentType.MakeByRefType(), typeof(TProperty)};
        var method = new DynamicMethod(string.Empty, null, parameterTypes, property.Module, true);
        var generator = method.GetILGenerator();

        generator.Emit(OpCodes.Ldarg_0);

        if (!parentType.IsValueType)
            generator.Emit(OpCodes.Ldind_Ref);

        generator.Emit(OpCodes.Ldarg_1);
        generator.Emit(OpCodes.Call, property.GetSetMethod());
        generator.Emit(OpCodes.Ret);

        return (Setter<TEntity, TProperty>) method.CreateDelegate(typeof(Setter<TEntity, TProperty>));
    }
}