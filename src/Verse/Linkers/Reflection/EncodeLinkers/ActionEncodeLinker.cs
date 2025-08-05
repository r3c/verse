using System;

namespace Verse.Linkers.Reflection.EncodeLinkers;

internal class ActionEncodeLinker<TNative> : IEncodeLinker<TNative>
{
    private readonly object _action;
    private readonly Type _match;

    public ActionEncodeLinker(Type match, object action)
    {
        _action = action;
        _match = match;
    }

    public bool TryDescribe<TEntity>(EncodeContext<TNative> context, IEncoderDescriptor<TNative, TEntity> descriptor)
    {
        if (typeof(TEntity) != _match)
            return false;

        var action = (Action<IEncoderDescriptor<TNative, TEntity>>)_action;

        action(descriptor);

        return true;
    }
}