using System;
using System.Collections.Generic;
using System.Reflection;
using Verse.Generators;
using Verse.Resolvers;

namespace Verse;

/// <summary>
/// Utility class able to scan any type (using reflection) to automatically
/// declare its fields to a decoder or encoder descriptor.
/// </summary>
public static class Linker
{
    private const BindingFlags DefaultBindings = BindingFlags.Instance | BindingFlags.Public;

    /// <summary>
    /// Describe and create decoder for given schema using reflection on target entity and provided binding flags.
    /// </summary>
    /// <typeparam name="TNative">Schema native value type</typeparam>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="schema">Entity schema</param>
    /// <param name="converters">Custom type converters for given schema, values must have
    /// DecoderConverter&lt;T, TNative&gt; for each key = typeof(T)</param>
    /// <param name="bindings">Binding flags used to filter bound fields and properties</param>
    /// <returns>Entity decoder</returns>
    public static IDecoder<TEntity> CreateDecoder<TNative, TEntity>(ISchema<TNative, TEntity> schema,
        IReadOnlyDictionary<Type, object> converters, BindingFlags bindings)
    {
        if (!TryLinkDecoder(schema.DecoderDescriptor, schema.DecoderAdapter, converters, bindings,
                new Dictionary<Type, object>()))
            throw new ArgumentException($"can't link decoder for type '{typeof(TEntity)}'", nameof(schema));

        return schema.CreateDecoder();
    }

    /// <summary>
    /// Describe and create decoder for given schema using reflection on target entity. Only public instance fields
    /// and properties are linked.
    /// </summary>
    /// <typeparam name="TNative">Schema native value type</typeparam>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="schema">Entity schema</param>
    /// <returns>Entity decoder</returns>
    public static IDecoder<TEntity> CreateDecoder<TNative, TEntity>(ISchema<TNative, TEntity> schema)
    {
        return CreateDecoder(schema, new Dictionary<Type, object>(), DefaultBindings);
    }

    /// <summary>
    /// Describe and create encoder for given schema using reflection on target entity and provided binding flags.
    /// </summary>
    /// <typeparam name="TNative">Schema native value type</typeparam>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="schema">Entity schema</param>
    /// <param name="converters">Custom type converters for given schema, values must have Func&lt;TNative, T&gt;
    /// for each key = typeof(T)</param>
    /// <param name="bindings">Binding flags used to filter bound fields and properties</param>
    /// <returns>Entity encoder</returns>
    public static IEncoder<TEntity> CreateEncoder<TNative, TEntity>(ISchema<TNative, TEntity> schema,
        IReadOnlyDictionary<Type, object> converters, BindingFlags bindings)
    {
        if (!TryLinkEncoder(schema.EncoderDescriptor, schema.EncoderAdapter, converters, bindings,
                new Dictionary<Type, object>()))
            throw new ArgumentException($"can't link encoder for type '{typeof(TEntity)}'", nameof(schema));

        return schema.CreateEncoder();
    }

    /// <summary>
    /// Describe and create encoder for given schema using reflection on target entity. Only public instance fields
    /// and properties are linked.
    /// </summary>
    /// <typeparam name="TNative">Schema native value type</typeparam>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="schema">Entity schema</param>
    /// <returns>Entity encoder</returns>
    public static IEncoder<TEntity> CreateEncoder<TNative, TEntity>(ISchema<TNative, TEntity> schema)
    {
        return CreateEncoder(schema, new Dictionary<Type, object>(), DefaultBindings);
    }

    private static bool TryGetDecoderConverter<TNative, TEntity>(IDecoderAdapter<TNative> adapter,
        IReadOnlyDictionary<Type, object> converters, out Setter<TEntity, TNative> converter)
    {
        if (converters.TryGetValue(typeof(TEntity), out var untyped))
            converter = (Setter<TEntity, TNative>) untyped;
        else if (!AdapterResolver.TryGetDecoderConverter(adapter, out converter))
            return false;

        return true;
    }

    private static bool TryGetEncoderConverter<TValue, TEntity>(IEncoderAdapter<TValue> adapter,
        IReadOnlyDictionary<Type, object> converters, out Func<TEntity, TValue> converter)
    {
        if (converters.TryGetValue(typeof(TEntity), out var untyped))
            converter = (Func<TEntity, TValue>) untyped;
        else if (!AdapterResolver.TryGetEncoderConverter(adapter, out converter))
            return false;

        return true;
    }

    private static bool TryLinkDecoder<TNative, TEntity>(IDecoderDescriptor<TNative, TEntity> descriptor,
        IDecoderAdapter<TNative> adapter, IReadOnlyDictionary<Type, object> converters, BindingFlags bindings,
        IDictionary<Type, object> parents)
    {
        var entityType = typeof(TEntity);

        parents[entityType] = descriptor;

        if (TryLinkDecoderAsValue(descriptor, adapter, converters))
            return true;

        // Bind descriptor as an array of target type is also array
        if (entityType.IsArray)
        {
            var element = entityType.GetElementType();
            var converter = MethodResolver
                .Create<Func<Func<IEnumerable<object>, object[]>>>(() =>
                    ConverterGenerator.CreateFromEnumerable<object>())
                .SetGenericArguments(element)
                .Invoke(null);

            return TryLinkDecoderAsArray(descriptor, adapter, converters, bindings, element, converter,
                parents);
        }

        // Try to bind descriptor as an array if target type IEnumerable<>
        foreach (var interfaceType in entityType.GetInterfaces())
        {
            // Make sure that interface is IEnumerable<T> and store typeof(T)
            if (!TypeResolver.Create(interfaceType)
                    .HasSameDefinitionThan<IEnumerable<object>>(out var interfaceTypeArguments))
                continue;

            var elementType = interfaceTypeArguments[0];

            // Search constructor compatible with IEnumerable<>
            foreach (var constructor in entityType.GetConstructors())
            {
                var parameters = constructor.GetParameters();

                if (parameters.Length != 1)
                    continue;

                var parameterType = parameters[0].ParameterType;

                if (!TypeResolver.Create(parameterType)
                        .HasSameDefinitionThan<IEnumerable<object>>(out var parameterArguments) ||
                    parameterArguments[0] != elementType)
                    continue;

                var converter = MethodResolver
                    .Create<Func<ConstructorInfo, Func<object, object>>>(c =>
                        ConverterGenerator.CreateFromConstructor<object, object>(c))
                    .SetGenericArguments(entityType, parameterType)
                    .Invoke(null, constructor);

                return TryLinkDecoderAsArray(descriptor, adapter, converters, bindings, elementType,
                    converter, parents);
            }
        }

        // Bind descriptor as object
        var objectDescriptor = descriptor.IsObject(ConstructorGenerator.CreateConstructor<TEntity>(bindings));

        // Bind readable and writable instance properties
        foreach (var property in entityType.GetProperties(bindings))
        {
            if (property.GetGetMethod() == null || property.GetSetMethod() == null ||
                property.Attributes.HasFlag(PropertyAttributes.SpecialName))
                continue;

            var setter = MethodResolver
                .Create<Func<PropertyInfo, Setter<object, object>>>(p =>
                    SetterGenerator.CreateFromProperty<object, object>(p))
                .SetGenericArguments(entityType, property.PropertyType)
                .Invoke(null, property);

            if (!TryLinkDecoderAsObject(objectDescriptor, adapter, converters, bindings,
                    property.PropertyType, property.Name, setter, parents))
                return false;
        }

        // Bind public instance fields
        foreach (var field in entityType.GetFields(bindings))
        {
            if (field.Attributes.HasFlag(FieldAttributes.SpecialName))
                continue;

            var setter = MethodResolver
                .Create<Func<FieldInfo, Setter<object, object>>>(f =>
                    SetterGenerator.CreateFromField<object, object>(f))
                .SetGenericArguments(entityType, field.FieldType)
                .Invoke(null, field);

            if (!TryLinkDecoderAsObject(objectDescriptor, adapter, converters, bindings, field.FieldType,
                    field.Name, setter, parents))
                return false;
        }

        return true;
    }

    private static bool TryLinkDecoderAsArray<TNative, TEntity>(IDecoderDescriptor<TNative, TEntity> descriptor,
        IDecoderAdapter<TNative> adapter, IReadOnlyDictionary<Type, object> converters, BindingFlags bindings,
        Type type, object converter, IDictionary<Type, object> parents)
    {
        if (parents.TryGetValue(type, out var recurse))
        {
            MethodResolver
                .Create<Func<IDecoderDescriptor<TNative, TEntity>, Func<IEnumerable<object>, TEntity>,
                    IDecoderDescriptor<TNative, object>, IDecoderDescriptor<TNative, object>>>((d, c, p) =>
                    d.IsArray(c, p))
                .SetGenericArguments(type)
                .Invoke(descriptor, converter, recurse);

            return true;
        }

        var itemDescriptor = MethodResolver
            .Create<Func<IDecoderDescriptor<TNative, TEntity>, Func<IEnumerable<object>, TEntity>,
                IDecoderDescriptor<TNative, object>>>((d, c) => d.IsArray(c))
            .SetGenericArguments(type)
            .Invoke(descriptor, converter);

        return (bool) MethodResolver
            .Create<Func<IDecoderDescriptor<TNative, object>, IDecoderAdapter<TNative>,
                IReadOnlyDictionary<Type, object>, BindingFlags, Dictionary<Type, object>, bool>>(
                (d, a, c, b, p) => TryLinkDecoder(d, a, c, b, p))
            .SetGenericArguments(typeof(TNative), type)
            .Invoke(null, itemDescriptor, adapter, converters, bindings, parents);
    }

    private static bool TryLinkDecoderAsObject<TNative, TEntity>(
        IDecoderObjectDescriptor<TNative, TEntity> objectDescriptor, IDecoderAdapter<TNative> adapter,
        IReadOnlyDictionary<Type, object> converters, BindingFlags bindings, Type type, string name,
        object setter, IDictionary<Type, object> parents)
    {
        if (parents.TryGetValue(type, out var parent))
        {
            MethodResolver
                .Create<Func<IDecoderObjectDescriptor<TNative, TEntity>, string, Setter<TEntity, object>,
                    IDecoderDescriptor<TNative, object>,
                    IDecoderDescriptor<TNative, object>>>((d, n, s, p) =>
                    d.HasField(n, s, p))
                .SetGenericArguments(type)
                .Invoke(objectDescriptor, name, setter, parent);

            return true;
        }

        var fieldDescriptor = MethodResolver
            .Create<Func<IDecoderObjectDescriptor<TNative, TEntity>, string, Setter<TEntity, object>,
                IDecoderDescriptor<TNative, object>>>((d, n, s) => d.HasField(n, s))
            .SetGenericArguments(type)
            .Invoke(objectDescriptor, name, setter);

        return (bool) MethodResolver
            .Create<Func<IDecoderDescriptor<TNative, object>, IDecoderAdapter<TNative>,
                IReadOnlyDictionary<Type, object>, BindingFlags, Dictionary<Type, object>, bool>>(
                (d, a, c, f, p) => TryLinkDecoder(d, a, c, f, p))
            .SetGenericArguments(typeof(TNative), type)
            .Invoke(null, fieldDescriptor, adapter, converters, bindings, parents);
    }

    private static bool TryLinkDecoderAsValue<TValue, TEntity>(
        IDecoderDescriptor<TValue, TEntity> descriptor, IDecoderAdapter<TValue> adapter,
        IReadOnlyDictionary<Type, object> converters)
    {
        // Try linking using provided entity type directly
        if (TryGetDecoderConverter<TValue, TEntity>(adapter, converters, out var converter))
        {
            descriptor.IsValue(converter);

            return true;
        }

        // Try linking using using underlying type if entity is nullable
        if (!TypeResolver.Create(typeof(TEntity)).HasSameDefinitionThan<int?>(out var arguments))
            return false;

        return (bool) MethodResolver
            .Create<Func<IDecoderDescriptor<object, int?>, IDecoderAdapter<object>,
                IReadOnlyDictionary<Type, object>, bool>>((d, a, c) =>
                TryLinkDecoderAsValueNullable(d, a, c))
            .SetGenericArguments(typeof(TValue), arguments[0])
            .Invoke(null, descriptor, adapter, converters);
    }

    private static bool TryLinkDecoderAsValueNullable<TValue, TEntity>(
        IDecoderDescriptor<TValue, TEntity?> descriptor, IDecoderAdapter<TValue> adapter,
        IReadOnlyDictionary<Type, object> converters) where TEntity : struct
    {
        if (!TryGetDecoderConverter<TValue, TEntity>(adapter, converters, out var converter))
            return false;

        descriptor.IsValue((ref TEntity? target, TValue source) =>
        {
            // Assume that default TValue is equivalent to null entity
            if (EqualityComparer<TValue>.Default.Equals(source, default))
                target = default;

            // Otherwise use underlying converter to retreive non-null entity
            else
            {
                var entity = target.GetValueOrDefault();

                converter(ref entity, source);

                target = entity;
            }
        });

        return true;
    }

    private static bool TryLinkEncoder<TNative, TEntity>(IEncoderDescriptor<TNative, TEntity> descriptor,
        IEncoderAdapter<TNative> adapter, IReadOnlyDictionary<Type, object> converters, BindingFlags bindings,
        IDictionary<Type, object> parents)
    {
        var entityType = typeof(TEntity);

        parents[entityType] = descriptor;

        if (TryLinkEncoderAsValue(descriptor, adapter, converters))
            return true;

        // Bind descriptor as an array of target type is also array
        if (entityType.IsArray)
        {
            var element = entityType.GetElementType();
            var getter = MethodResolver
                .Create<Func<Func<object, object>>>(() => GetterGenerator.CreateIdentity<object>())
                .SetGenericArguments(typeof(IEnumerable<>).MakeGenericType(element))
                .Invoke(null);

            return TryLinkEncoderAsArray(descriptor, adapter, converters, bindings, element, getter,
                parents);
        }

        // Try to bind descriptor as an array if target type IEnumerable<>
        foreach (var interfaceType in entityType.GetInterfaces())
        {
            // Make sure that interface is IEnumerable<T> and store typeof(T)
            if (!TypeResolver.Create(interfaceType).HasSameDefinitionThan<IEnumerable<object>>(out var arguments))
                continue;

            var elementType = arguments[0];
            var getter = MethodResolver
                .Create<Func<Func<object, object>>>(() => GetterGenerator.CreateIdentity<object>())
                .SetGenericArguments(typeof(IEnumerable<>).MakeGenericType(elementType))
                .Invoke(null);

            return TryLinkEncoderAsArray(descriptor, adapter, converters, bindings, elementType, getter,
                parents);
        }

        // Bind readable and writable instance properties
        foreach (var property in entityType.GetProperties(bindings))
        {
            if (property.GetMethod == null || property.SetMethod == null ||
                property.Attributes.HasFlag(PropertyAttributes.SpecialName))
                continue;

            var getter = MethodResolver
                .Create<Func<PropertyInfo, Func<object, object>>>(p =>
                    GetterGenerator.CreateFromProperty<object, object>(p))
                .SetGenericArguments(entityType, property.PropertyType)
                .Invoke(null, property);

            if (!TryLinkEncoderAsObject(descriptor, adapter, converters, bindings, property.PropertyType,
                    property.Name, getter, parents))
                return false;
        }

        // Bind public instance fields
        foreach (var field in entityType.GetFields(bindings))
        {
            if (field.Attributes.HasFlag(FieldAttributes.SpecialName))
                continue;

            var getter = MethodResolver
                .Create<Func<FieldInfo, Func<object, object>>>(f =>
                    GetterGenerator.CreateFromField<object, object>(f))
                .SetGenericArguments(entityType, field.FieldType)
                .Invoke(null, field);

            if (!TryLinkEncoderAsObject(descriptor, adapter, converters, bindings, field.FieldType,
                    field.Name,
                    getter, parents))
                return false;
        }

        return true;
    }

    private static bool TryLinkEncoderAsArray<TNative, TEntity>(IEncoderDescriptor<TNative, TEntity> descriptor,
        IEncoderAdapter<TNative> adapter, IReadOnlyDictionary<Type, object> converters, BindingFlags bindings,
        Type type, object getter, IDictionary<Type, object> parents)
    {
        if (parents.TryGetValue(type, out var recurse))
        {
            MethodResolver
                .Create<Func<IEncoderDescriptor<TNative, TEntity>, Func<TEntity, IEnumerable<object>>,
                    IEncoderDescriptor<TNative, object>, IEncoderDescriptor<TNative, object>>>((d, a, p) =>
                    d.HasElements(a, p))
                .SetGenericArguments(type)
                .Invoke(descriptor, getter, recurse);

            return true;
        }

        var itemDescriptor = MethodResolver
            .Create<Func<IEncoderDescriptor<TNative, TEntity>, Func<TEntity, IEnumerable<object>>,
                IEncoderDescriptor<TNative, object>
            >>((d, a) => d.HasElements(a))
            .SetGenericArguments(type)
            .Invoke(descriptor, getter);

        return (bool) MethodResolver
            .Create<Func<IEncoderDescriptor<TNative, object>, IEncoderAdapter<TNative>,
                IReadOnlyDictionary<Type, object>, BindingFlags, Dictionary<Type, object>, bool>>(
                (d, a, c, b, p) => TryLinkEncoder(d, a, c, b, p))
            .SetGenericArguments(typeof(TNative), type)
            .Invoke(null, itemDescriptor, adapter, converters, bindings, parents);
    }

    private static bool TryLinkEncoderAsObject<TNative, TEntity>(IEncoderDescriptor<TNative, TEntity> descriptor,
        IEncoderAdapter<TNative> adapter, IReadOnlyDictionary<Type, object> converters, BindingFlags bindings,
        Type type, string name, object getter, IDictionary<Type, object> parents)
    {
        if (parents.TryGetValue(type, out var recurse))
        {
            MethodResolver
                .Create<Func<IEncoderDescriptor<TNative, TEntity>, string, Func<TEntity, object>,
                    IEncoderDescriptor<TNative, object>,
                    IEncoderDescriptor<TNative, object>>>((d, n, a, p) => d.HasField(n, a, p))
                .SetGenericArguments(type)
                .Invoke(descriptor, name, getter, recurse);

            return true;
        }

        var fieldDescriptor = MethodResolver
            .Create<Func<IEncoderDescriptor<TNative, TEntity>, string, Func<TEntity, object>,
                IEncoderDescriptor<TNative, object>>>(
                (d, n, a) => d.HasField(n, a))
            .SetGenericArguments(type)
            .Invoke(descriptor, name, getter);

        return (bool) MethodResolver
            .Create<Func<IEncoderDescriptor<TNative, object>, IEncoderAdapter<TNative>,
                IReadOnlyDictionary<Type, object>, BindingFlags, Dictionary<Type, object>, bool>>((d, a, c, b, p) =>
                TryLinkEncoder(d, a, c, b, p))
            .SetGenericArguments(typeof(TNative), type)
            .Invoke(null, fieldDescriptor, adapter, converters, bindings, parents);
    }

    private static bool TryLinkEncoderAsValue<TValue, TEntity>(IEncoderDescriptor<TValue, TEntity> descriptor,
        IEncoderAdapter<TValue> adapter, IReadOnlyDictionary<Type, object> converters)
    {
        // Try linking using provided entity type directly
        if (TryGetEncoderConverter<TValue, TEntity>(adapter, converters, out var converter))
        {
            descriptor.HasValue(converter);

            return true;
        }

        // Try linking using using underlying type if entity is nullable
        if (!TypeResolver.Create(typeof(TEntity)).HasSameDefinitionThan<int?>(out var arguments))
            return false;

        return (bool) MethodResolver
            .Create<Func<IEncoderDescriptor<object, int?>, IEncoderAdapter<object>,
                IReadOnlyDictionary<Type, object>, bool>>((d, a, c) =>
                TryLinkEncoderAsValueNullable(d, a, c))
            .SetGenericArguments(typeof(TValue), arguments[0])
            .Invoke(null, descriptor, adapter, converters);
    }

    private static bool TryLinkEncoderAsValueNullable<TValue, TEntity>(
        IEncoderDescriptor<TValue, TEntity?> descriptor, IEncoderAdapter<TValue> adapter,
        IReadOnlyDictionary<Type, object> converters) where TEntity : struct
    {
        if (!TryGetEncoderConverter<TValue, TEntity>(adapter, converters, out var converter))
            return false;

        descriptor.HasValue(source => source.HasValue ? converter(source.Value) : default);

        return true;
    }
}