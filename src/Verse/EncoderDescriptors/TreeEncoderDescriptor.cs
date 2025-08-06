using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Tree;

namespace Verse.EncoderDescriptors;

internal class TreeEncoderDescriptor<TState, TNative, TEntity>(IWriterDefinition<TState, TNative, TEntity> definition)
    : IEncoderDescriptor<TNative, TEntity>
{
    private readonly WriterLayer<TState, TNative, TEntity> _layer = new(definition);

    public IEncoder<TEntity> CreateEncoder(IWriter<TState, TNative> reader)
    {
        return new TreeEncoder<TState, TNative, TEntity>(reader, _layer.Callback);
    }

    public IEncoderDescriptor<TNative, TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>?> getter,
        IEncoderDescriptor<TNative, TElement> descriptor)
    {
        if (descriptor is not TreeEncoderDescriptor<TState, TNative, TElement> ancestor)
            throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

        return BindArray(_layer, getter, ancestor);
    }

    public IEncoderDescriptor<TNative, TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>?> getter)
    {
        var elementDefinition = _layer.Definition.Create<TElement>();
        var elementDescriptor = new TreeEncoderDescriptor<TState, TNative, TElement>(elementDefinition);

        return BindArray(_layer, getter, elementDescriptor);
    }

    public IEncoderObjectDescriptor<TNative, TObject> IsObject<TObject>(Func<TEntity, TObject> converter)
    {
        var objectDefinition = _layer.Definition.Create<TObject>();
        var objectDescriptor = new TreeEncoderDescriptor<TState, TNative, TObject>.ObjectDescriptor(objectDefinition);

        _layer.Callback = (writer, state, source) => writer.WriteAsObject(state, converter(source),
            objectDefinition.Fields);

        return objectDescriptor;
    }

    public IEncoderObjectDescriptor<TNative, TEntity> IsObject()
    {
        return IsObject(e => e);
    }

    public void IsValue(Func<TEntity, TNative> converter)
    {
        _layer.Callback = (writer, state, entity) => writer.WriteAsValue(state, converter(entity));
    }

    private static IEncoderDescriptor<TNative, TElement> BindArray<TElement>(
        WriterLayer<TState, TNative, TEntity> arrayLayer, Func<TEntity, IEnumerable<TElement>?> getter,
        TreeEncoderDescriptor<TState, TNative, TElement> elementDescriptor)
    {
        var elementLayer = elementDescriptor._layer;

        arrayLayer.Callback = (reader, state, entity) => reader.WriteAsArray(state, getter(entity),
            elementLayer.Callback);

        return elementDescriptor;
    }

    private class ObjectDescriptor(IWriterDefinition<TState, TNative, TEntity> definition)
        : IEncoderObjectDescriptor<TNative, TEntity>
    {
        private readonly WriterLayer<TState, TNative, TEntity> _layer = new(definition);

        public IEncoderDescriptor<TNative, TField> HasField<TField>(string name, Func<TEntity, TField> getter,
            IEncoderDescriptor<TNative, TField> descriptor)
        {
            if (descriptor is not TreeEncoderDescriptor<TState, TNative, TField> ancestor)
                throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

            return BindField(_layer, name, getter, ancestor);
        }

        public IEncoderDescriptor<TNative, TField> HasField<TField>(string name, Func<TEntity, TField> getter)
        {
            var fieldDefinition = _layer.Definition.Create<TField>();
            var fieldDescriptor = new TreeEncoderDescriptor<TState, TNative, TField>(fieldDefinition);

            return BindField(_layer, name, getter, fieldDescriptor);
        }

        public IEncoderDescriptor<TNative, TEntity> HasField(string name)
        {
            return HasField(name, e => e);
        }

        private static IEncoderDescriptor<TNative, TField> BindField<TField>(
            WriterLayer<TState, TNative, TEntity> objectLayer, string name, Func<TEntity, TField> getter,
            TreeEncoderDescriptor<TState, TNative, TField> fieldDescriptor)
        {
            var objectDefinition = objectLayer.Definition;
            var objectFields = objectDefinition.Fields;

            if (objectFields.ContainsKey(name))
                throw new InvalidOperationException($"field '{name}' was declared twice on same descriptor");

            objectLayer.Callback = (reader, state, entity) => reader.WriteAsObject(state, entity, objectFields);

            var fieldLayer = fieldDescriptor._layer;

            objectFields[name] = (reader, state, entity) => fieldLayer.Callback(reader, state, getter(entity));

            return fieldDescriptor;
        }
    }
}