namespace Verse.EncoderDescriptors.Tree
{
	internal interface IWriterDefinition<TState, TNative, TEntity>
	{
		WriterCallback<TState, TNative, TEntity> Callback { get; set; }

		IWriterDefinition<TState, TNative, TOther> Create<TOther>();
	}
}