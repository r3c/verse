using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Base;
using Verse.EncoderDescriptors.Tree;
using Verse.Schemas.Protobuf.Definition;

namespace Verse.Schemas.Protobuf
{
	class Writer<TEntity> : TreeWriter<WriterState, TEntity, ProtobufValue>
	{
		private readonly ProtoBinding[] fields;

		public Writer(ProtoBinding[] fields)
		{
			this.fields = fields;
		}

		public override TreeWriter<WriterState, TOther, ProtobufValue> Create<TOther>()
		{
			return new Writer<TOther>(this.fields);
		}

		public override void WriteElements(WriterState state, IEnumerable<TEntity> elements)
		{
			throw new NotImplementedException();
		}

		public override void WriteFields(WriterState state, TEntity source, IReadOnlyDictionary<string, EntityWriter<WriterState, TEntity>> fields)
		{
			throw new NotImplementedException();
		}

		public override void WriteNull(WriterState state)
		{
			throw new NotImplementedException();
		}

		public override void WriteValue(WriterState state, ProtobufValue value)
		{
			throw new NotImplementedException();
		}

		protected bool TryLookup<TOther>(string name, out int index, out IWriter<WriterState, TOther> writer)
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
