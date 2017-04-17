namespace Verse.DecoderDescriptors.Flat.FlatReaders.PatternFlat.Nodes
{
	class EmptyNode<TEntity, TState, TValue> : INode<TEntity, TState, TValue>
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

		#region Methods

		public void Assign(ref TEntity target, TValue value)
		{
		}

		public bool Enter(ref TEntity target, IFlatReader<TEntity, TState, TValue> unknown, TState context)
		{
			TEntity dummy;

			return unknown.ReadValue(() => default(TEntity), context, out dummy);
		}

		public INode<TEntity, TState, TValue> Follow(char c)
		{
			return this;
		}

		#endregion
	}
}