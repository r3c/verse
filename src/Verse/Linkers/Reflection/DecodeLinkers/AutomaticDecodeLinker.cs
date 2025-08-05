using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Linkers.Reflection.DecodeLinkers;

internal class AutomaticDecodeLinker<TNative> : IDecodeLinker<TNative>
{
    private static readonly IReadOnlyList<IDecodeLinker<TNative>> DefaultDecodeLinkers = new IDecodeLinker<TNative>[]
    {
        ValueDecodeLinker<TNative>.Instance,
        ArrayDecodeLinker<TNative>.Instance,
        ObjectDecodeLinker<TNative>.Instance
    };

    private readonly IReadOnlyList<IDecodeLinker<TNative>> _decodeLinkers;

    public AutomaticDecodeLinker(IEnumerable<IDecodeLinker<TNative>> customDecodeLinkers)
    {
        _decodeLinkers = customDecodeLinkers.Concat(DefaultDecodeLinkers).ToList();
    }

    public bool TryDescribe<TEntity>(DecodeContext<TNative> context, IDecoderDescriptor<TNative, TEntity> descriptor)
    {
        if (context.Parents.ContainsKey(typeof(TEntity)))
        {
            throw new InvalidOperationException(
                $"{typeof(TEntity)} already described by {nameof(AutomaticDecodeLinker<TNative>)}");
        }

        context.Parents[typeof(TEntity)] = descriptor;

        // Try describe using known decode linkers
        foreach (var linker in _decodeLinkers)
        {
            if (linker.TryDescribe(context, descriptor))
                return true;
        }

        return false;
    }
}