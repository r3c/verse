using System;

namespace Verse.Linkers.Reflection.DecodeLinkers;

internal class ActionDecodeLinker<TNative>(Type match, object action) : IDecodeLinker<TNative>
{
    public bool TryDescribe<TEntity>(DecodeContext<TNative> context, IDecoderDescriptor<TNative, TEntity> descriptor)
    {
        if (typeof(TEntity) != match)
            return false;

        var action1 = (Action<IDecoderDescriptor<TNative, TEntity>>)action;

        action1(descriptor);

        return true;
    }
}