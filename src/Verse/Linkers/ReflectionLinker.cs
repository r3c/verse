using System;
using System.Collections.Generic;
using System.Reflection;
using Verse.Linkers.Reflection;
using Verse.Linkers.Reflection.DecodeLinkers;
using Verse.Linkers.Reflection.EncodeLinkers;

namespace Verse.Linkers;

internal class ReflectionLinker<TNative> : ILinker<TNative>
{
    private readonly Dictionary<Type, IDecodeLinker<TNative>> _decodeLinkers = new();

    private readonly Dictionary<Type, IEncodeLinker<TNative>> _encodeLinkers = new();

    private BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public;

    public IDecoder<TEntity> CreateDecoder<TEntity>(IFormat<TNative> format, ISchema<TNative, TEntity> schema)
    {
        var automatic = new AutomaticDecodeLinker<TNative>(_decodeLinkers.Values);
        var context = new DecodeContext<TNative>(automatic, _bindingFlags, format, new Dictionary<Type, object>());

        if (!automatic.TryDescribe(context, schema.DecoderDescriptor))
            throw new ArgumentException($"can't link decoder for type '{typeof(TEntity)}'", nameof(schema));

        return schema.CreateDecoder();
    }

    public IEncoder<TEntity> CreateEncoder<TEntity>(IFormat<TNative> format, ISchema<TNative, TEntity> schema)
    {
        var automatic = new AutomaticEncodeLinker<TNative>(_encodeLinkers.Values);
        var context = new EncodeContext<TNative>(automatic, _bindingFlags, format, new Dictionary<Type, object>());

        if (!automatic.TryDescribe(context, schema.EncoderDescriptor))
            throw new ArgumentException($"can't link encoder for type '{typeof(TEntity)}'", nameof(schema));

        return schema.CreateEncoder();
    }

    public ILinker<TNative> SetBindingFlags(BindingFlags bindingFlags)
    {
        _bindingFlags = bindingFlags;

        return this;
    }

    public ILinker<TNative> SetDecoderDescriptor<TEntity>(Action<IDecoderDescriptor<TNative, TEntity>> describe)
    {
        _decodeLinkers[typeof(TEntity)] = new ActionDecodeLinker<TNative>(typeof(TEntity), describe);

        return this;
    }

    public ILinker<TNative> SetEncoderDescriptor<TEntity>(Action<IEncoderDescriptor<TNative, TEntity>> describe)
    {
        _encodeLinkers[typeof(TEntity)] = new ActionEncodeLinker<TNative>(typeof(TEntity), describe);

        return this;
    }
}