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

    private static readonly object NoCaller = new();

    /// <summary>
    /// Describe and create decoder for given schema using reflection on target entity and provided binding flags.
    /// </summary>
    /// <typeparam name="TNative">Schema native value type</typeparam>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="schema">Entity schema</param>
    /// <param name="converters">Custom type converters for given schema, values must have type
    /// Func&lt;TNative, TEntity&gt; for each TEntity = Type</param>
    /// <param name="bindings">Binding flags used to filter bound fields and properties</param>
    /// <returns>Entity decoder</returns>
    public static IDecoder<TEntity> CreateDecoder<TNative, TEntity>(ISchema<TNative, TEntity> schema,
        IReadOnlyDictionary<Type, object> converters, BindingFlags bindings)
    {
        var decoder = new DecoderSchema<TNative, TEntity>(schema.DecoderDescriptor,
            schema.NativeTo, schema.DefaultValue, converters);

        if (!TryLinkDecoder(decoder, bindings, new Dictionary<Type, object>()))
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
        var encoder = new EncoderSchema<TNative, TEntity>(schema.EncoderDescriptor,
            schema.NativeFrom, schema.DefaultValue, converters);

        if (!TryLinkEncoder(encoder, bindings, new Dictionary<Type, object>()))
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
        IReadOnlyDictionary<Type, object> converters, out Func<TNative, TEntity> converter)
    {
        if (converters.TryGetValue(typeof(TEntity), out var untyped))
        {
            if (untyped is not Func<TNative, TEntity> typed)
            {
                throw new InvalidOperationException(
                    $"invalid decoder converter for type {typeof(TEntity)}: {untyped.GetType()} (should be {typeof(Func<TNative, TEntity>)})");
            }

            converter = typed;
        }
        else if (!AdapterResolver.TryGetDecoderConverter(adapter, out converter))
            return false;

        return true;
    }

    private static bool TryGetEncoderConverter<TNative, TEntity>(IEncoderAdapter<TNative> adapter,
        IReadOnlyDictionary<Type, object> converters, out Func<TEntity, TNative> converter)
    {
        if (converters.TryGetValue(typeof(TEntity), out var untyped))
        {
            if (untyped is not Func<TEntity, TNative> typed)
            {
                throw new InvalidOperationException(
                    $"invalid encoder converter for type {typeof(TEntity)}: {untyped.GetType()} (should be {typeof(Func<TEntity, TNative>)})");
            }

            converter = typed;
        }
        else if (!AdapterResolver.TryGetEncoderConverter(adapter, out converter))
            return false;

        return true;
    }

    private static bool TryLinkDecoder<TNative, TEntity>(DecoderSchema<TNative, TEntity> decoder,
        BindingFlags bindings, IDictionary<Type, object> parents)
    {
        var entityType = typeof(TEntity);

        parents[entityType] = decoder.Descriptor;

        if (TryLinkDecoderAsValue(decoder))
            return true;

        // Bind descriptor as an array of target type is also array
        if (entityType.IsArray)
        {
            var element = entityType.GetElementType() ?? throw new InvalidOperationException("array has no element type");
            var converter = MethodResolver
                .Create<Func<Func<IEnumerable<object>, object[]>>>(() => ConverterGenerator.CreateFromEnumerable<object>())
                .SetGenericArguments(element)
                .Invoke(NoCaller);

            return TryLinkDecoderAsArray(decoder, bindings, element, converter, parents);
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
                    .Create<Func<ConstructorInfo, Func<object, object>>>(c => ConverterGenerator.CreateFromConstructor<object, object>(c))
                    .SetGenericArguments(entityType, parameterType)
                    .Invoke(NoCaller, constructor);

                return TryLinkDecoderAsArray(decoder, bindings, elementType, converter, parents);
            }
        }

        // Bind descriptor as object
        var objectDescriptor = decoder.Descriptor.IsObject(ConstructorGenerator.CreateConstructor<TEntity>(bindings));

        // Bind readable and writable instance properties
        foreach (var property in entityType.GetProperties(bindings))
        {
            if (property.GetGetMethod() == null || property.GetSetMethod() == null ||
                property.Attributes.HasFlag(PropertyAttributes.SpecialName))
                continue;

            var setter = MethodResolver
                .Create<Func<PropertyInfo, Func<object, object, object>>>(p => SetterGenerator.CreateFromProperty<object, object>(p))
                .SetGenericArguments(entityType, property.PropertyType)
                .Invoke(NoCaller, property);

            if (!TryLinkDecoderAsObject(decoder, objectDescriptor, bindings, property.PropertyType, property.Name, setter, parents))
                return false;
        }

        // Bind public instance fields
        foreach (var field in entityType.GetFields(bindings))
        {
            if (field.Attributes.HasFlag(FieldAttributes.SpecialName))
                continue;

            var setter = MethodResolver
                .Create<Func<FieldInfo, Func<object, object, object>>>(f => SetterGenerator.CreateFromField<object, object>(f))
                .SetGenericArguments(entityType, field.FieldType)
                .Invoke(NoCaller, field);

            if (!TryLinkDecoderAsObject(decoder, objectDescriptor, bindings, field.FieldType, field.Name, setter, parents))
                return false;
        }

        return true;
    }

    private static bool TryLinkDecoderAsArray<TNative, TEntity>(
        DecoderSchema<TNative, TEntity> decoder, BindingFlags bindings, Type type,
        object converter, IDictionary<Type, object> parents)
    {
        if (parents.TryGetValue(type, out var recurse))
        {
            MethodResolver
                .Create<Func<IDecoderDescriptor<TNative, TEntity>, Func<IEnumerable<object>, TEntity>, IDecoderDescriptor<TNative, object>,
                    IDecoderDescriptor<TNative, object>>>(
                    (d, c, p) => d.IsArray(c, p))
                .SetGenericArguments(type)
                .Invoke(decoder.Descriptor, converter, recurse);

            return true;
        }

        var itemDescriptor = MethodResolver
            .Create<Func<IDecoderDescriptor<TNative, TEntity>, Func<IEnumerable<object>, TEntity>, IDecoderDescriptor<TNative, object>>>(
                (d, c) => d.IsArray(c))
            .SetGenericArguments(type)
            .Invoke(decoder.Descriptor, converter);

        var itemDecoder = MethodResolver
            .Create<Func<DecoderSchema<TNative, TEntity>, IDecoderDescriptor<TNative, object>, DecoderSchema<TNative, object>>>(
                (d, o) => d.TurnInto(o))
            .SetGenericArguments(type)
            .Invoke(decoder, itemDescriptor);

        return (bool)MethodResolver
            .Create<Func<DecoderSchema<TNative, object>, BindingFlags, Dictionary<Type, object>, bool>>(
                (d, b, p) => TryLinkDecoder(d, b, p))
            .SetGenericArguments(typeof(TNative), type)
            .Invoke(NoCaller, itemDecoder, bindings, parents);
    }

    private static bool TryLinkDecoderAsObject<TNative, TEntity>(
        DecoderSchema<TNative, TEntity> decoder,
        IDecoderObjectDescriptor<TNative, TEntity> objectDescriptor,
        BindingFlags bindings, Type type, string name,
        object setter, IDictionary<Type, object> parents)
    {
        if (parents.TryGetValue(type, out var parent))
        {
            MethodResolver
                .Create<Func<IDecoderObjectDescriptor<TNative, TEntity>, string, Func<TEntity, object, TEntity>,
                    IDecoderDescriptor<TNative, object>,
                    IDecoderDescriptor<TNative, object>>>((d, n, s, p) =>
                    d.HasField(n, s, p))
                .SetGenericArguments(type)
                .Invoke(objectDescriptor, name, setter, parent);

            return true;
        }

        var fieldDescriptor = MethodResolver
            .Create<Func<IDecoderObjectDescriptor<TNative, TEntity>, string, Func<TEntity, object, TEntity>,
                IDecoderDescriptor<TNative, object>>>((d, n, s) => d.HasField(n, s))
            .SetGenericArguments(type)
            .Invoke(objectDescriptor, name, setter);

        var fieldDecoder = MethodResolver
            .Create<Func<DecoderSchema<TNative, TEntity>, IDecoderDescriptor<TNative, object>,
                DecoderSchema<TNative, object>>>((d, o) => d.TurnInto(o))
            .SetGenericArguments(type)
            .Invoke(decoder, fieldDescriptor);

        return (bool)MethodResolver
            .Create<Func<DecoderSchema<TNative, object>, BindingFlags, Dictionary<Type, object>, bool>>(
                (d, f, p) => TryLinkDecoder(d, f, p))
            .SetGenericArguments(typeof(TNative), type)
            .Invoke(NoCaller, fieldDecoder, bindings, parents);
    }

    private static bool TryLinkDecoderAsValue<TNative, TEntity>(
        DecoderSchema<TNative, TEntity> decoder)
    {
        // Try linking using provided entity type directly
        if (TryGetDecoderConverter<TNative, TEntity>(decoder.Adapter, decoder.Converters, out var converter))
        {
            decoder.Descriptor.IsValue(converter);

            return true;
        }

        // Try linking using using underlying type if entity is nullable
        if (!TypeResolver.Create(typeof(TEntity)).HasSameDefinitionThan<int?>(out var arguments))
            return false;

        return (bool)MethodResolver
            .Create<Func<DecoderSchema<object, int?>, bool>>(d => TryLinkDecoderAsValueNullable(d))
            .SetGenericArguments(typeof(TNative), arguments[0])
            .Invoke(NoCaller, decoder);
    }

    private static bool TryLinkDecoderAsValueNullable<TNative, TEntity>(
        DecoderSchema<TNative, TEntity?> decoder) where TEntity : struct
    {
        if (!TryGetDecoderConverter<TNative, TEntity>(decoder.Adapter, decoder.Converters, out var converter))
            return false;

        var nullValue = decoder.NullValue;

        decoder.Descriptor.IsValue(source => Equals(nullValue, source)
            // Assume that default TNative is equivalent to null entity
            ? default(TEntity?)
            // Otherwise use underlying converter to retreive non-null entity
            : converter(source));

        return true;
    }

    private static bool TryLinkEncoder<TNative, TEntity>(EncoderSchema<TNative, TEntity> encoder,
        BindingFlags bindings, IDictionary<Type, object> parents)
    {
        var entityType = typeof(TEntity);

        parents[entityType] = encoder.Descriptor;

        if (TryLinkEncoderAsValue(encoder))
            return true;

        // Bind descriptor as an array of target type is also array
        if (entityType.IsArray)
        {
            var element = entityType.GetElementType() ?? throw new InvalidOperationException("array has no element type");
            var getter = MethodResolver
                .Create<Func<Func<object, object>>>(() => GetterGenerator.CreateIdentity<object>())
                .SetGenericArguments(typeof(IEnumerable<>).MakeGenericType(element))
                .Invoke(NoCaller);

            return TryLinkEncoderAsArray(encoder, bindings, element, getter, parents);
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
                .Invoke(NoCaller);

            return TryLinkEncoderAsArray(encoder, bindings, elementType, getter, parents);
        }

        // Bind descriptor as object
        var objectDescriptor = encoder.Descriptor.IsObject();

        // Bind readable and writable instance properties
        foreach (var property in entityType.GetProperties(bindings))
        {
            if (property.GetMethod == null ||
                property.SetMethod == null ||
                property.Attributes.HasFlag(PropertyAttributes.SpecialName))
                continue;

            var getter = MethodResolver
                .Create<Func<PropertyInfo, Func<object, object>>>(p => GetterGenerator.CreateFromProperty<object, object>(p))
                .SetGenericArguments(entityType, property.PropertyType)
                .Invoke(NoCaller, property);

            if (!TryLinkEncoderAsObject(encoder, objectDescriptor, bindings, property.PropertyType, property.Name, getter, parents))
                return false;
        }

        // Bind public instance fields
        foreach (var field in entityType.GetFields(bindings))
        {
            if (field.Attributes.HasFlag(FieldAttributes.SpecialName))
                continue;

            var getter = MethodResolver
                .Create<Func<FieldInfo, Func<object, object>>>(f => GetterGenerator.CreateFromField<object, object>(f))
                .SetGenericArguments(entityType, field.FieldType)
                .Invoke(NoCaller, field);

            if (!TryLinkEncoderAsObject(encoder, objectDescriptor, bindings, field.FieldType, field.Name, getter, parents))
                return false;
        }

        return true;
    }

    private static bool TryLinkEncoderAsArray<TNative, TEntity>(
        EncoderSchema<TNative, TEntity> encoder, BindingFlags bindings, Type type, object getter,
        IDictionary<Type, object> parents)
    {
        if (parents.TryGetValue(type, out var recurse))
        {
            MethodResolver
                .Create<Func<IEncoderDescriptor<TNative, TEntity>, Func<TEntity, IEnumerable<object>>, IEncoderDescriptor<TNative, object>,
                    IEncoderDescriptor<TNative, object>>>((d, a, p) => d.IsArray(a, p))
                .SetGenericArguments(type)
                .Invoke(encoder.Descriptor, getter, recurse);

            return true;
        }

        var itemDescriptor = MethodResolver
            .Create<Func<IEncoderDescriptor<TNative, TEntity>, Func<TEntity, IEnumerable<object>>, IEncoderDescriptor<TNative, object>>>(
                (d, a) => d.IsArray(a))
            .SetGenericArguments(type)
            .Invoke(encoder.Descriptor, getter);

        var itemEncoder = MethodResolver
            .Create<Func<EncoderSchema<TNative, TEntity>, IEncoderDescriptor<TNative, object>, EncoderSchema<TNative, object>>>(
                (e, o) => e.TurnInto(o))
            .SetGenericArguments(type)
            .Invoke(encoder, itemDescriptor);

        return (bool)MethodResolver
            .Create<Func<EncoderSchema<TNative, object>, BindingFlags, Dictionary<Type, object>, bool>>(
                (e, b, p) => TryLinkEncoder(e, b, p))
            .SetGenericArguments(typeof(TNative), type)
            .Invoke(NoCaller, itemEncoder, bindings, parents);
    }

    private static bool TryLinkEncoderAsObject<TNative, TEntity>(EncoderSchema<TNative, TEntity> encoder,
        IEncoderObjectDescriptor<TNative, TEntity> objectDescriptor, BindingFlags bindings, Type type, string name,
        object getter, IDictionary<Type, object> parents)
    {
        if (parents.TryGetValue(type, out var recurse))
        {
            MethodResolver
                .Create<Func<IEncoderObjectDescriptor<TNative, TEntity>, string, Func<TEntity, object>, IEncoderDescriptor<TNative, object>,
                    IEncoderDescriptor<TNative, object>>>((d, n, a, p) => d.HasField(n, a, p))
                .SetGenericArguments(type)
                .Invoke(objectDescriptor, name, getter, recurse);

            return true;
        }

        var fieldDescriptor = MethodResolver
            .Create<Func<IEncoderObjectDescriptor<TNative, TEntity>, string, Func<TEntity, object>, IEncoderDescriptor<TNative, object>>>(
                (d, n, a) => d.HasField(n, a))
            .SetGenericArguments(type)
            .Invoke(objectDescriptor, name, getter);

        var fieldEncoder = MethodResolver
            .Create<Func<EncoderSchema<TNative, TEntity>, IEncoderDescriptor<TNative, object>, EncoderSchema<TNative, object>>>(
                (e, o) => e.TurnInto(o))
            .SetGenericArguments(type)
            .Invoke(encoder, fieldDescriptor);

        return (bool)MethodResolver
            .Create<Func<EncoderSchema<TNative, object>, BindingFlags, Dictionary<Type, object>, bool>>(
                (e, b, p) => TryLinkEncoder(e, b, p))
            .SetGenericArguments(typeof(TNative), type)
            .Invoke(NoCaller, fieldEncoder, bindings, parents);
    }

    private static bool TryLinkEncoderAsValue<TNative, TEntity>(EncoderSchema<TNative, TEntity> encoder)
    {
        // Try linking using provided entity type directly
        if (TryGetEncoderConverter<TNative, TEntity>(encoder.Adapter, encoder.Converters, out var converter))
        {
            encoder.Descriptor.IsValue(converter);

            return true;
        }

        // Try linking using using underlying type if entity is nullable
        if (!TypeResolver.Create(typeof(TEntity)).HasSameDefinitionThan<int?>(out var arguments))
            return false;

        return (bool)MethodResolver
            .Create<Func<EncoderSchema<object, int?>, bool>>(d => TryLinkEncoderAsValueNullable(d))
            .SetGenericArguments(typeof(TNative), arguments[0])
            .Invoke(NoCaller, encoder);
    }

    private static bool TryLinkEncoderAsValueNullable<TNative, TEntity>(EncoderSchema<TNative, TEntity?> encoder) where TEntity : struct
    {
        if (!TryGetEncoderConverter<TNative, TEntity>(encoder.Adapter, encoder.Converters, out var converter))
            return false;

        encoder.Descriptor.IsValue(source => source.HasValue ? converter(source.Value) : encoder.NullValue);

        return true;
    }

    private record struct DecoderSchema<TNative, TEntity>(
        IDecoderDescriptor<TNative, TEntity> Descriptor,
        IDecoderAdapter<TNative> Adapter,
        TNative NullValue,
        IReadOnlyDictionary<Type, object> Converters)
    {
        public DecoderSchema<TNative, TOther> TurnInto<TOther>(IDecoderDescriptor<TNative, TOther> descriptor)
        {
            return new DecoderSchema<TNative, TOther>(descriptor, Adapter, NullValue, Converters);
        }
    }

    private record struct EncoderSchema<TNative, TEntity>(
        IEncoderDescriptor<TNative, TEntity> Descriptor,
        IEncoderAdapter<TNative> Adapter,
        TNative NullValue,
        IReadOnlyDictionary<Type, object> Converters)
    {
        public EncoderSchema<TNative, TOther> TurnInto<TOther>(IEncoderDescriptor<TNative, TOther> descriptor)
        {
            return new EncoderSchema<TNative, TOther>(descriptor, Adapter, NullValue, Converters);
        }
    }
}