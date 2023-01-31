using System;

namespace Verse.Linkers.Reflection.DecodeLinkers;

internal class DescriptorDecodeLinker<TNative> : IDecodeLinker<TNative>
{
    private readonly object _action;
    private readonly Type _match;

    public DescriptorDecodeLinker(Type match, object action)
    {
        _action = action;
        _match = match;
    }

    public bool TryDescribe<TEntity>(DecodeContext<TNative> context, IDecoderDescriptor<TNative, TEntity> descriptor)
    {
        if (typeof(TEntity) != _match)
            return false;

        var action = (Action<IDecoderDescriptor<TNative, TEntity>>)_action;

        action(descriptor);

        return true;
    }
}