namespace Verse.Linkers.Reflection;

internal interface IDecodeLinker<TNative>
{
    bool TryDescribe<TEntity>(DecodeContext<TNative> context, IDecoderDescriptor<TNative, TEntity> descriptor);
}