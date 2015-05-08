using System;
using Verse.ParserDescriptors.Recurse.Nodes;

namespace Verse.ParserDescriptors.Recurse
{
    internal class Container<TEntity, TContext, TNative>
    {
        public BranchNode<TEntity, TContext, TNative> fields = new BranchNode<TEntity, TContext, TNative>();

        public Follow<TEntity, TContext, TNative> items = null;

        public ParserAssign<TEntity, TNative> value = null;
    }
}