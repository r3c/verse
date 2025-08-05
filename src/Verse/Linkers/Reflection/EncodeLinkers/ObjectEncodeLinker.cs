using System;
using System.Reflection;
using Verse.Generators;
using Verse.Resolvers;

namespace Verse.Linkers.Reflection.EncodeLinkers;

internal class ObjectEncodeLinker<TNative> : IEncodeLinker<TNative>
{
    public static readonly ObjectEncodeLinker<TNative> Instance = new();

    public bool TryDescribe<TEntity>(EncodeContext<TNative> context, IEncoderDescriptor<TNative, TEntity> descriptor)
    {
        var entityType = typeof(TEntity);
        var objectDescriptor = descriptor.IsObject();

        // Bind readable and writable instance properties
        foreach (var property in entityType.GetProperties(context.BindingFlags))
        {
            if (property.GetMethod == null ||
                property.SetMethod == null ||
                property.Attributes.HasFlag(PropertyAttributes.SpecialName))
                continue;

            var getter = MethodResolver
                .Create<Func<PropertyInfo, Func<object, object>>>(p =>
                    GetterGenerator.CreateFromProperty<object, object>(p))
                .SetGenericArguments(entityType, property.PropertyType)
                .InvokeStatic(property);

            if (!TryDescribeAsObject(context, objectDescriptor, property.PropertyType, property.Name, getter))
                return false;
        }

        // Bind public instance fields
        foreach (var field in entityType.GetFields(context.BindingFlags))
        {
            if (field.Attributes.HasFlag(FieldAttributes.SpecialName))
                continue;

            var getter = MethodResolver
                .Create<Func<FieldInfo, Func<object, object>>>(f => GetterGenerator.CreateFromField<object, object>(f))
                .SetGenericArguments(entityType, field.FieldType)
                .InvokeStatic(field);

            if (!TryDescribeAsObject(context, objectDescriptor, field.FieldType, field.Name, getter))
                return false;
        }

        return true;
    }

    private static bool TryDescribeAsObject<TEntity>(EncodeContext<TNative> context,
        IEncoderObjectDescriptor<TNative, TEntity> objectDescriptor, Type type, string name, object? getter)
    {
        if (context.Parents.TryGetValue(type, out var recurse))
        {
            MethodResolver
                .Create<Func<IEncoderObjectDescriptor<TNative, TEntity>, string, Func<TEntity, object>,
                    IEncoderDescriptor<TNative, object>,
                    IEncoderDescriptor<TNative, object>>>((d, n, a, p) => d.HasField(n, a, p))
                .SetGenericArguments(type)
                .InvokeInstance(objectDescriptor, name, getter, recurse);

            return true;
        }

        var fieldDescriptor = MethodResolver
            .Create<Func<IEncoderObjectDescriptor<TNative, TEntity>, string, Func<TEntity, object>,
                IEncoderDescriptor<TNative, object>>>((d, n, a) => d.HasField(n, a))
            .SetGenericArguments(type)
            .InvokeInstance(objectDescriptor, name, getter);

        return (bool)MethodResolver
            .Create<Func<IEncodeLinker<TNative>, EncodeContext<TNative>, IEncoderDescriptor<TNative, object>, bool>>((l,
                c, d) => l.TryDescribe(c, d))
            .SetGenericArguments(type)
            .InvokeInstance(context.Automatic, context, fieldDescriptor)!;
    }
}