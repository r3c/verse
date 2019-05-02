using Verse.DecoderDescriptors.Tree;
using Verse.Lookups;

namespace Verse.Schemas.RawProtobuf
{
	internal class
		RawProtobufReaderDefinition<TEntity> : IReaderDefinition<RawProtobufReaderState, RawProtobufValue, char, TEntity
		>
	{
		public ReaderCallback<RawProtobufReaderState, RawProtobufValue, char, TEntity> Callback { get; set; } =
			(IReader<RawProtobufReaderState, RawProtobufValue, char> reader, RawProtobufReaderState state,
				ref TEntity entity) => reader.ReadToValue(state, out _);

		public ILookup<char, ReaderCallback<RawProtobufReaderState, RawProtobufValue, char, TEntity>> Lookup { get; } =
			new NameLookup<ReaderCallback<RawProtobufReaderState, RawProtobufValue, char, TEntity>>();

		public IReaderDefinition<RawProtobufReaderState, RawProtobufValue, char, TOther> Create<TOther>()
		{
			return new RawProtobufReaderDefinition<TOther>();
		}
	}
}