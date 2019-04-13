using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Tree;
using Verse.Schemas.Protobuf.Definition;

namespace Verse.Schemas.Protobuf
{
	class Writer<TEntity> : WriterDefinition<WriterState, ProtobufValue, TEntity>
	{
		private readonly ProtoBinding[] fields;

		public Writer(ProtoBinding[] fields)
		{
			this.fields = fields;
		}

		public override WriterDefinition<WriterState, ProtobufValue, TOther> Create<TOther>()
		{
			return new Writer<TOther>(this.fields);
		}

		protected bool TryLookup<TOther>(string name, out int index, out WriterDefinition<WriterState, ProtobufValue, TOther> writer)
		{
			index = Array.FindIndex(this.fields, binding => binding.Name == name);

			if (index < 0)
			{
				writer = null;

				return false;
			}

			writer = new Writer<TOther>(this.fields[index].Fields);

			return true;
		}
	}
}
