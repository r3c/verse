using System;
using Verse.EncoderDescriptors.Tree;
using Verse.Schemas.Protobuf.Definition;

namespace Verse.Schemas.Protobuf
{
	internal class ProtobufWriterDefinition<TEntity> : IWriterDefinition<WriterState, ProtobufValue, TEntity>
	{
		private readonly ProtoBinding[] fields;

		public ProtobufWriterDefinition(ProtoBinding[] fields)
		{
			this.fields = fields;
		}

		public WriterCallback<WriterState, ProtobufValue, TEntity> Callback { get; set; } = (reader, state, entity) =>
			reader.WriteAsValue(state, ProtobufValue.Empty);

		public IWriterDefinition<WriterState, ProtobufValue, TOther> Create<TOther>()
		{
			return new ProtobufWriterDefinition<TOther>(this.fields);
		}

		protected bool TryLookup<TOther>(string name, out int index, out IWriterDefinition<WriterState, ProtobufValue, TOther> writer)
		{
			index = Array.FindIndex(this.fields, binding => binding.Name == name);

			if (index < 0)
			{
				writer = null;

				return false;
			}

			writer = new ProtobufWriterDefinition<TOther>(this.fields[index].Fields);

			return true;
		}
	}
}
