using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Base;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.JSON
{
	class Writer<TEntity> : TreeWriter<WriterState, TEntity, JSONValue>
	{
		public override TreeWriter<WriterState, TOther, JSONValue> Create<TOther>()
		{
			return new Writer<TOther>();
		}

		public override void WriteElements(WriterState state, IEnumerable<TEntity> elements)
		{
			state.ArrayBegin();

			foreach (var element in elements)
				this.Write(state, element);

			state.ArrayEnd();
		}

		public override void WriteFields(WriterState state, TEntity source, IReadOnlyDictionary<string, EntityWriter<WriterState, TEntity>> fields)
		{
			state.ObjectBegin();

			foreach (var field in fields)
			{
				state.Key(field.Key);
				field.Value(state, source);
			}

			state.ObjectEnd();
		}

		public override void WriteNull(WriterState state)
		{
			state.Value(JSON.JSONValue.Void);
		}

		public override void WriteValue(WriterState state, JSONValue value)
		{
			state.Value(value);
		}
	}
}