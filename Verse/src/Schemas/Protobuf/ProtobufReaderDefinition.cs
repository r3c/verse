using Verse.DecoderDescriptors.Tree;
using Verse.Schemas.Protobuf.Definition;

namespace Verse.Schemas.Protobuf
{
	internal class ProtobufReaderDefinition<TEntity> : ReaderDefinition<ReaderState, ProtobufValue, TEntity>
	{
		//private static readonly Reader<TEntity> emptyReader = new Reader<TEntity>(new ProtoBinding[0], false);

		private readonly ProtoBinding[] bindings;

		//private readonly ReaderCallback<ReaderState, ProtobufValue, TEntity>[] fields;

		private readonly bool rejectUnknown;

		public ProtobufReaderDefinition(ProtoBinding[] bindings, bool rejectUnknown)
		{
			this.bindings = bindings;
			//this.fields = new ReaderCallback<ReaderState, ProtobufValue, TEntity>[bindings.Length];
			this.rejectUnknown = rejectUnknown;
		}

		public override ReaderDefinition<ReaderState, ProtobufValue, TOther> Create<TOther>()
		{
			return new ProtobufReaderDefinition<TOther>(this.bindings, this.rejectUnknown);
		}
/*
		public override TreeReader<ReaderState, TField, ProtobufValue> HasField<TField>(string name, ReaderCallback<ReaderState, TEntity> enter)
		{
			int index = Array.FindIndex(this.bindings, binding => binding.Name == name);

			if (index < 0)
				throw new ArgumentOutOfRangeException("name", name, "field doesn't exist in proto definition");

			this.fields[index] = enter;

			return new Reader<TField>(this.bindings[index].Fields, this.rejectUnknown);
		}
*/
	}
}
