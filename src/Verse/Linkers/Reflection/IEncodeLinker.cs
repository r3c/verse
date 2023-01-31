namespace Verse.Linkers.Reflection;

internal interface IEncodeLinker<TNative>
{
    bool TryDescribe<TEntity>(EncodeContext<TNative> context, IEncoderDescriptor<TNative, TEntity> descriptor);
}