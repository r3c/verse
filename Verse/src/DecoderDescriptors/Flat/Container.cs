using Verse.DecoderDescriptors.Flat.Nodes;

namespace Verse.DecoderDescriptors.Flat
{
    internal class Container<TEntity, TContext, TNative>
    {
        public BranchNode<TEntity, TContext, TNative> fields = new BranchNode<TEntity, TContext, TNative>();

        public DecodeAssign<TEntity, TNative> value = null;
    }
}