using System;
using System.Collections.Generic;
using Verse.DecoderDescriptors.Tree;

namespace Verse.DecoderDescriptors;

internal class TreeDecoderDescriptor<TState, TNative, TKey, TEntity>(
    IReaderDefinition<TState, TNative, TKey, TEntity> definition)
    : IDecoderDescriptor<TNative, TEntity>
{
    private readonly ReaderLayer<TState, TNative, TKey, TEntity> _layer = new(definition);

    public IDecoder<TEntity> CreateDecoder(IReader<TState, TNative, TKey> reader)
    {
        return new TreeDecoder<TState, TNative, TKey, TEntity>(reader, _layer.Callback);
    }

    public IDecoderDescriptor<TNative, TElement> IsArray<TElement>(Func<IEnumerable<TElement>, TEntity> converter,
        IDecoderDescriptor<TNative, TElement> descriptor)
    {
        if (descriptor is not TreeDecoderDescriptor<TState, TNative, TKey, TElement> treeDescriptor)
            throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

        return BindArray(_layer, converter, treeDescriptor);
    }

    public IDecoderDescriptor<TNative, TElement> IsArray<TElement>(Func<IEnumerable<TElement>, TEntity> converter)
    {
        var elementDefinition = _layer.Definition.Create<TElement>();
        var elementDescriptor =
            new TreeDecoderDescriptor<TState, TNative, TKey, TElement>(elementDefinition);

        return BindArray(_layer, converter, elementDescriptor);
    }

    public IDecoderObjectDescriptor<TNative, TObject> IsObject<TObject>(Func<TObject> constructor,
        Func<TObject, TEntity> converter)
    {
        var objectDefinition = _layer.Definition.Create<TObject>();
        var objectLookup = objectDefinition.Lookup;
        var objectDescriptor = new TreeDecoderDescriptor<TState, TNative, TKey, TObject>.ObjectDescriptor(
            objectDefinition);

        _layer.Callback = (IReader<TState, TNative, TKey> reader, TState state, ref TEntity target) =>
        {
            var entity = constructor();

            switch (reader.ReadToObject(state, objectLookup.Root, ref entity))
            {
                case ReaderStatus.Succeeded:
                    target = converter(entity);

                    return ReaderStatus.Succeeded;

                case ReaderStatus.Ignored:
                    return ReaderStatus.Ignored;

                default:
                    return ReaderStatus.Failed;
            }
        };

        return objectDescriptor;
    }

    public IDecoderObjectDescriptor<TNative, TEntity> IsObject(Func<TEntity> constructor)
    {
        return IsObject(constructor, e => e);
    }

    public void IsValue(Func<TNative, TEntity> converter)
    {
        _layer.Callback = (IReader<TState, TNative, TKey> reader, TState state, ref TEntity entity) =>
        {
            switch (reader.ReadToValue(state, out var value))
            {
                case ReaderStatus.Succeeded:
                    entity = converter(value);

                    return ReaderStatus.Succeeded;

                case ReaderStatus.Ignored:
                    return ReaderStatus.Ignored;

                case ReaderStatus.Failed:
                default:
                    entity = default!;

                    return ReaderStatus.Failed;
            }
        };
    }

    private static TreeDecoderDescriptor<TState, TNative, TKey, TElement> BindArray<TElement>(
        ReaderLayer<TState, TNative, TKey, TEntity> arrayLayer, Func<IEnumerable<TElement>, TEntity> converter,
        TreeDecoderDescriptor<TState, TNative, TKey, TElement> elementDescriptor)
    {
        var elementDefinition = elementDescriptor._layer;

        arrayLayer.Callback = (IReader<TState, TNative, TKey> reader, TState state, ref TEntity entity) =>
        {
            var status = reader.ReadToArray(state, elementDefinition.Callback, out var arrayReader);

            if (status != ReaderStatus.Succeeded)
                return status;

            using var iterator = new ArrayIterator<TElement>(arrayReader);

            entity = converter(iterator);

            return iterator.Flush() ? ReaderStatus.Succeeded : ReaderStatus.Failed;
        };

        return elementDescriptor;
    }

    private class ObjectDescriptor(IReaderDefinition<TState, TNative, TKey, TEntity> definition)
        : IDecoderObjectDescriptor<TNative, TEntity>
    {
        private readonly ReaderLayer<TState, TNative, TKey, TEntity> _layer = new(definition);

        public IDecoderDescriptor<TNative, TField> HasField<TField>(string name, Func<TEntity, TField, TEntity> setter,
            IDecoderDescriptor<TNative, TField> descriptor)
        {
            if (descriptor is not TreeDecoderDescriptor<TState, TNative, TKey, TField> ancestor)
                throw new ArgumentOutOfRangeException(nameof(descriptor), "incompatible descriptor type");

            return BindField(_layer, name, setter, ancestor);
        }

        public IDecoderDescriptor<TNative, TField> HasField<TField>(string name, Func<TEntity, TField, TEntity> setter)
        {
            var fieldDefinition = _layer.Definition.Create<TField>();
            var fieldDescriptor = new TreeDecoderDescriptor<TState, TNative, TKey, TField>(fieldDefinition);

            return BindField(_layer, name, setter, fieldDescriptor);
        }

        public IDecoderDescriptor<TNative, TEntity> HasField(string name)
        {
            var objectDefinition = _layer.Definition;
            var objectLookup = objectDefinition.Lookup;
            var objectRoot = objectLookup.Root;
            var fieldDefinition = objectDefinition.Create<TEntity>();
            var fieldDescriptor = new TreeDecoderDescriptor<TState, TNative, TKey, TEntity>(fieldDefinition);
            var fieldLayer = fieldDescriptor._layer;

            var success = objectLookup.ConnectTo(name,
                (IReader<TState, TNative, TKey> reader, TState state, ref TEntity entity) =>
                    fieldLayer.Callback(reader, state, ref entity));

            if (!success)
                throw new InvalidOperationException($"field '{name}' was declared twice on same descriptor");

            _layer.Callback = (IReader<TState, TNative, TKey> reader, TState state, ref TEntity target) =>
                reader.ReadToObject(state, objectRoot, ref target);

            return fieldDescriptor;
        }

        private static TreeDecoderDescriptor<TState, TNative, TKey, TField> BindField<TField>(
            ReaderLayer<TState, TNative, TKey, TEntity> objectLayer, string name,
            Func<TEntity, TField, TEntity> setter,
            TreeDecoderDescriptor<TState, TNative, TKey, TField> fieldDescriptor)
        {
            var objectDefinition = objectLayer.Definition;
            var objectLookup = objectDefinition.Lookup;
            var objectRoot = objectLookup.Root;
            var fieldLayer = fieldDescriptor._layer;

            var success = objectLookup.ConnectTo(name,
                (IReader<TState, TNative, TKey> reader, TState state, ref TEntity entity) =>
                {
                    TField field = default!;

                    switch (fieldLayer.Callback(reader, state, ref field))
                    {
                        case ReaderStatus.Failed:
                            return ReaderStatus.Failed;

                        case ReaderStatus.Succeeded:
                            entity = setter(entity, field);

                            break;
                    }

                    return ReaderStatus.Succeeded;
                });

            if (!success)
                throw new InvalidOperationException($"field '{name}' was declared twice on same descriptor");

            objectLayer.Callback = (IReader<TState, TNative, TKey> reader, TState state, ref TEntity target) =>
                reader.ReadToObject(state, objectRoot, ref target);

            return fieldDescriptor;
        }
    }
}