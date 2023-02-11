using System;
using System.Reflection;
using Verse.Generators;
using Verse.Resolvers;

namespace Verse.Linkers.Reflection.DecodeLinkers;

internal class ObjectDecodeLinker<TNative> : IDecodeLinker<TNative>
{
    public static readonly ObjectDecodeLinker<TNative> Instance = new();

    public bool TryDescribe<TEntity>(DecodeContext<TNative> context, IDecoderDescriptor<TNative, TEntity> descriptor)
    {
        var entityType = typeof(TEntity);
        var objectConstructor = ConstructorGenerator.CreateConstructor<TEntity>(context.BindingFlags);
        var objectDescriptor = descriptor.IsObject(objectConstructor);

        // Bind readable and writable instance properties
        foreach (var property in entityType.GetProperties(context.BindingFlags))
        {
            if (property.GetMethod == null ||
                property.SetMethod == null ||
                property.Attributes.HasFlag(PropertyAttributes.SpecialName))
                continue;

            var setter = MethodResolver
                .Create<Func<PropertyInfo, Func<object, object, object>>>(p =>
                    SetterGenerator.CreateFromProperty<object, object>(p))
                .SetGenericArguments(entityType, property.PropertyType)
                .InvokeStatic(property);

            if (!TryDescribeAsObject(context, objectDescriptor, property.PropertyType, property.Name, setter))
                return false;
        }

        // Bind public instance fields
        foreach (var field in entityType.GetFields(context.BindingFlags))
        {
            if (field.Attributes.HasFlag(FieldAttributes.SpecialName))
                continue;

            var setter = MethodResolver
                .Create<Func<FieldInfo, Func<object, object, object>>>(f =>
                    SetterGenerator.CreateFromField<object, object>(f))
                .SetGenericArguments(entityType, field.FieldType)
                .InvokeStatic(field);

            if (!TryDescribeAsObject(context, objectDescriptor, field.FieldType, field.Name, setter))
                return false;
        }

        return true;
    }

    private static bool TryDescribeAsObject<TEntity>(DecodeContext<TNative> context,
        IDecoderObjectDescriptor<TNative, TEntity> objectDescriptor, Type type, string name, object? setter)
    {
        if (context.Parents.TryGetValue(type, out var parent))
        {
            MethodResolver
                .Create<Func<IDecoderObjectDescriptor<TNative, TEntity>, string, Func<TEntity, object, TEntity>,
                    IDecoderDescriptor<TNative, object>,
                    IDecoderDescriptor<TNative, object>>>((d, n, s, p) =>
                    d.HasField(n, s, p))
                .SetGenericArguments(type)
                .InvokeInstance(objectDescriptor, name, setter, parent);

            return true;
        }

        var fieldDescriptor = MethodResolver
            .Create<Func<IDecoderObjectDescriptor<TNative, TEntity>, string, Func<TEntity, object, TEntity>,
                IDecoderDescriptor<TNative, object>>>((d, n, s) => d.HasField(n, s))
            .SetGenericArguments(type)
            .InvokeInstance(objectDescriptor, name, setter);

        return (bool)MethodResolver
            .Create<Func<IDecodeLinker<TNative>, DecodeContext<TNative>, IDecoderDescriptor<TNative, object>, bool>>(
                (l, c, d) => l.TryDescribe(c, d))
            .SetGenericArguments(type)
            .InvokeInstance(context.Automatic, context, fieldDescriptor)!;
    }
}