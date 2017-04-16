namespace Verse.DecoderDescriptors.Flat.Nodes
{
    internal class EmptyNode<TEntity, TContext, TNative> : INode<TEntity, TContext, TNative>
    {
        #region Properties

        public bool CanAssign
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Attributes

        private static readonly Container<TEntity, TContext, TNative> blank = new Container<TEntity, TContext, TNative>();

        #endregion

        #region Methods

        public void Assign(ref TEntity target, TNative value)
        {
        }

        public bool Enter(ref TEntity target, IReader<TContext, TNative> unknown, TContext context)
        {
        	TEntity dummy;

        	return unknown.ReadValue(() => default(TEntity), EmptyNode<TEntity, TContext, TNative>.blank, context, out dummy);
        }

        public INode<TEntity, TContext, TNative> Follow(char c)
        {
            return this;
        }

        #endregion
    }
}