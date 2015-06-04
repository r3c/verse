using Verse.ParserDescriptors.Flat.Nodes;

namespace Verse.ParserDescriptors.Flat
{
    internal class Container<TEntity, TContext, TNative>
    {
        public BranchNode<TEntity, TContext, TNative> fields = new BranchNode<TEntity, TContext, TNative>();

        public ParserAssign<TEntity, TNative> value = null;
    }
}