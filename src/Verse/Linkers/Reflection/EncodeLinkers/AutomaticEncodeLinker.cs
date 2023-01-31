using System.Collections.Generic;
using System.Linq;

namespace Verse.Linkers.Reflection.EncodeLinkers;

internal class AutomaticEncodeLinker<TNative> : IEncodeLinker<TNative>
{
    private static readonly IReadOnlyList<IEncodeLinker<TNative>> DefaultEncodeLinkers = new IEncodeLinker<TNative>[]
    {
        ValueEncodeLinker<TNative>.Instance,
        ArrayEncodeLinker<TNative>.Instance,
        ObjectEncodeLinker<TNative>.Instance
    };

    private readonly IReadOnlyList<IEncodeLinker<TNative>> _encodeLinkers;

    public AutomaticEncodeLinker(IEnumerable<IEncodeLinker<TNative>> customEncodeLinkers)
    {
        _encodeLinkers = customEncodeLinkers.Concat(DefaultEncodeLinkers).ToList();
    }

    public bool TryDescribe<TEntity>(EncodeContext<TNative> context, IEncoderDescriptor<TNative, TEntity> descriptor)
    {
        // TODO: should "Parents" be stored as a field within this class?
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