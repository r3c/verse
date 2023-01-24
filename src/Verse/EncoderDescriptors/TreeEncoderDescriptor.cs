using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Tree;

namespace Verse.EncoderDescriptors;

internal class TreeEncoderDescriptor<TState, TNative, TEntity> : IEncoderDescriptor<TNative, TEntity>
{
    private readonly IWriterDefinition<TState, TNative, TEntity> _definition;

    public TreeEncoderDescriptor(IWriterDefinition<TState, TNative, TEntity> definition)
    {
        _definition = definition;
    }

    public IEncoder<TEntity> CreateEncoder(IWriter<TState, TNative> reader)
    {
        return new TreeEncoder<TState, TNative, TEntity>(reader, _definition.Callback);
    }

    public IEncoderDescriptor<TNative, TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> getter,
        IEncoderDescriptor<TNative, TElement> descriptor)
    {
        if (descriptor is not TreeEncoderDescriptor<TState, TNative, TElement> ancestor)
            throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

        return BindArray(_definition, getter, ancestor);
    }

    public IEncoderDescriptor<TNative, TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> getter)
    {
        var elementDefinition = _definition.Create<TElement>();
        var elementDescriptor = new TreeEncoderDescriptor<TState, TNative, TElement>(elementDefinition);

        return BindArray(_definition, getter,
            elementDescriptor);
    }

    public IEncoderObjectDescriptor<TNative, TObject> IsObject<TObject>(Func<TEntity, TObject> converter)
    {
        var objectDefinition = _definition.Create<TObject>();
        var objectDescriptor = new TreeEncoderDescriptor<TState, TNative, TObject>.ObjectDescriptor(objectDefinition);

        _definition.Callback = (writer, state, source) => writer.WriteAsObject(state, converter(source),
            objectDefinition.Fields);

        return objectDescriptor;
    }

    public IEncoderObjectDescriptor<TNative, TEntity> IsObject()
    {
        return IsObject(e => e);
    }

    public void IsValue(Func<TEntity, TNative> converter)
    {
        _definition.Callback = (writer, state, entity) => writer.WriteAsValue(state, converter(entity));
    }

    private static IEncoderDescriptor<TNative, TElement> BindArray<TElement>(
        IWriterDefinition<TState, TNative, TEntity> parent, Func<TEntity, IEnumerable<TElement>> getter,
        TreeEncoderDescriptor<TState, TNative, TElement> elementDescriptor)
    {
        var elementDefinition = elementDescriptor._definition;

        parent.Callback = (reader, state, entity) =>
            reader.WriteAsArray(state, getter(entity), elementDefinition.Callback);

        return elementDescriptor;
    }

    private class ObjectDescriptor : IEncoderObjectDescriptor<TNative, TEntity>
    {
        private readonly IWriterDefinition<TState, TNative, TEntity> _definition;

        public ObjectDescriptor(IWriterDefinition<TState, TNative, TEntity> definition)
        {
            _definition = definition;
        }

        public IEncoderDescriptor<TNative, TField> HasField<TField>(string name, Func<TEntity, TField> getter,
            IEncoderDescriptor<TNative, TField> descriptor)
        {
            if (descriptor is not TreeEncoderDescriptor<TState, TNative, TField> ancestor)
                throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

            return BindField(_definition, name, getter, ancestor);
        }

        public IEncoderDescriptor<TNative, TField> HasField<TField>(string name, Func<TEntity, TField> getter)
        {
            var fieldDefinition = _definition.Create<TField>();
            var fieldDescriptor = new TreeEncoderDescriptor<TState, TNative, TField>(fieldDefinition);

            return BindField(_definition, name, getter, fieldDescriptor);
        }

        public IEncoderDescriptor<TNative, TEntity> HasField(string name)
        {
            return HasField(name, e => e);
        }

        private static IEncoderDescriptor<TNative, TField> BindField<TField>(
            IWriterDefinition<TState, TNative, TEntity> parentDefinition, string name,
            Func<TEntity, TField> getter, TreeEncoderDescriptor<TState, TNative, TField> fieldDescriptor)
        {
            var parentFields = parentDefinition.Fields;

            if (parentFields.ContainsKey(name))
                throw new InvalidOperationException($"field '{name}' was declared twice on same descriptor");

            parentDefinition.Callback = (reader, state, entity) => reader.WriteAsObject(state, entity, parentFields);

            var fieldDefinition = fieldDescriptor._definition;

            parentFields[name] = (reader, state, entity) => fieldDefinition.Callback(reader, state, getter(entity));

            return fieldDescriptor;
        }
    }
}