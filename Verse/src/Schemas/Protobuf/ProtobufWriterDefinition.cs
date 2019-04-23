using System;
using Verse.EncoderDescriptors.Tree;
using Verse.Schemas.Protobuf.Definition;

namespace Verse.Schemas.Protobuf
{
	internal class ProtobufWriterDefinition<TEntity> : WriterDefinition<WriterState, ProtobufValue, TEntity>
	{
		private readonly ProtoBinding[] fields;

		public ProtobufWriterDefinition(ProtoBinding[] fields)
		{
			this.fields = fields;
		}

		public override WriterDefinition<WriterState, ProtobufValue, TOther> Create<TOther>()
		{
			return new ProtobufWriterDefinition<TOther>(this.fields);
		}

		protected bool TryLookup<TOther>(string name, out int index, out WriterDefinition<WriterState, ProtobufValue, TOther> writer)
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
