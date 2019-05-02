using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.RawProtobuf
{
	internal class
		RawProtobufWriterDefinition<TEntity> : IWriterDefinition<RawProtobufWriterState, RawProtobufValue, TEntity>
	{
		public WriterCallback<RawProtobufWriterState, RawProtobufValue, TEntity> Callback { get; set; } =
			(reader, state, entity) => reader.WriteAsValue(state, new RawProtobufValue());

		public IWriterDefinition<RawProtobufWriterState, RawProtobufValue, TOther> Create<TOther>()
		{
			return new RawProtobufWriterDefinition<TOther>();
		}
	}
}