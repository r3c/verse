using Verse.DecoderDescriptors.Tree;
using Verse.Lookups;

namespace Verse.Schemas.JSON
{
	internal class ReaderDefinition<TEntity> : IReaderDefinition<ReaderState, JSONValue, char, TEntity>
	{
		public ReaderCallback<ReaderState, JSONValue, char, TEntity> Callback { get; set; } =
			(IReader<ReaderState, JSONValue, char> reader, ReaderState state, ref TEntity entity) =>
				reader.ReadToValue(state, out _);

		public ILookup<char, ReaderCallback<ReaderState, JSONValue, char, TEntity>> Lookup { get; } =
			new NameLookup<ReaderCallback<ReaderState, JSONValue, char, TEntity>>();

		public IReaderDefinition<ReaderState, JSONValue, char, TOther> Create<TOther>()
		{
			return new ReaderDefinition<TOther>();
		}
	}
}
