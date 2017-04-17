namespace Verse.DecoderDescriptors.Flat.FlatReaders.PatternFlat
{
	interface INode<TEntity, TState, TValue>
	{
		#region Properties

		bool CanAssign
		{
			get;
		}

		#endregion

		#region Methods

		void Assign(ref TEntity target, TValue value);

		bool Enter(ref TEntity target, IFlatReader<TEntity, TState, TValue> unknown, TState context);

		INode<TEntity, TState, TValue> Follow(char c);

		#endregion
	}
}