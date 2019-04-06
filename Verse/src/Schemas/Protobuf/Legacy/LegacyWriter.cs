using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Base;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.Protobuf.Legacy
{
	class LegacyWriter<TEntity> : TreeWriter<LegacyWriterState, TEntity, ProtobufValue>
	{
		public override TreeWriter<LegacyWriterState, TOther, ProtobufValue> Create<TOther>()
		{
			return new LegacyWriter<TOther>();
		}

		public override void WriteElements(LegacyWriterState state, IEnumerable<TEntity> elements)
		{
			foreach (var element in elements)
				this.Write(state, element);
		}

		public override void WriteFields(LegacyWriterState state, TEntity source, IReadOnlyDictionary<string, EntityWriter<LegacyWriterState, TEntity>> fields)
		{
			state.ObjectBegin();

			foreach (var field in fields)
			{
				if (field.Key.Length > 1 && field.Key[0] == '_')
					state.Key(field.Key.Substring(1));
				else
					state.Key(field.Key);

				field.Value(state, source);
			}

			state.ObjectEnd();
		}

		public override void WriteNull(LegacyWriterState state)
		{
			// No-op
		}

		public override void WriteValue(LegacyWriterState state, ProtobufValue value)
		{
			if (!state.Value(value))
				state.Error("failed to write value");
		}
	}
}
