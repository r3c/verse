namespace Verse.ParserDescriptors.Recurse.Nodes
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

        public bool Enter(ref TEntity target, IReader<TContext, TNative> reader, TContext context)
        {
            return reader.ReadValue(ref target, EmptyNode<TEntity, TContext, TNative>.blank, context);
        }

        public INode<TEntity, TContext, TNative> Follow(char c)
        {
            return this;
        }

        #endregion
    }
}