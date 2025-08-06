using System;

namespace Verse.Linkers.Reflection.EncodeLinkers;

internal class ActionEncodeLinker<TNative>(Type match, object action) : IEncodeLinker<TNative>
{
    public bool TryDescribe<TEntity>(EncodeContext<TNative> context, IEncoderDescriptor<TNative, TEntity> descriptor)
    {
        if (typeof(TEntity) != match)
            return false;

        var action1 = (Action<IEncoderDescriptor<TNative, TEntity>>)action;

        action1(descriptor);

        return true;
    }
}