using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Linkers.Reflection.EncodeLinkers;

internal class AutomaticEncodeLinker<TNative>(IEnumerable<IEncodeLinker<TNative>> customEncodeLinkers)
    : IEncodeLinker<TNative>
{
    private static readonly IReadOnlyList<IEncodeLinker<TNative>> DefaultEncodeLinkers =
    [
        ValueEncodeLinker<TNative>.Instance,
        ArrayEncodeLinker<TNative>.Instance,
        ObjectEncodeLinker<TNative>.Instance
    ];

    private readonly IReadOnlyList<IEncodeLinker<TNative>> _encodeLinkers = customEncodeLinkers.Concat(DefaultEncodeLinkers).ToList();

    public bool TryDescribe<TEntity>(EncodeContext<TNative> context, IEncoderDescriptor<TNative, TEntity> descriptor)
    {
        if (context.Parents.ContainsKey(typeof(TEntity)))
        {
            throw new InvalidOperationException(
                $"{typeof(TEntity)} already described by {nameof(AutomaticEncodeLinker<TNative>)}");
        }

        context.Parents[typeof(TEntity)] = descriptor;

        // Try describe using known encode linkers
        foreach (var linker in _encodeLinkers)
        {
            if (linker.TryDescribe(context, descriptor))
                return true;
        }

        return false;
    }
}