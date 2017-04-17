using System.Collections.Generic;
using Verse.EncoderDescriptors.Recurse;
using Verse.EncoderDescriptors.Recurse.RecurseWriters;

namespace Verse.Schemas.Protobuf
{
	class Writer<TEntity> : PatternRecurseWriter<TEntity, WriterState, ProtobufValue>
	{
		#region Methods

		public override IRecurseWriter<TOther, WriterState, ProtobufValue> Create<TOther>()
		{
			return new Writer<TOther>();
		}

		public override void WriteElements(IEnumerable<TEntity> elements, WriterState state)
		{
			foreach (var item in elements)
				this.WriteEntity(item, state);
		}

		public override void WriteEntity(TEntity source, WriterState state)
		{
			if (source == null)
				return;

			if (this.IsArray)
				this.ProcessArray(source, state);
			else if (this.IsValue)
			{
				if (!state.Value(this.ProcessValue(source)))
					state.Error("failed to write value");
			}
			else
			{
				state.ObjectBegin();

				foreach (var field in this.Fields)
				{
					if (field.Key.Length > 1 && field.Key[0] == '_')
						state.Key(field.Key.Substring(1));
					else
						state.Key(field.Key);

					field.Value(source, state);
				}

				state.ObjectEnd();
			}
		}

		#endregion
	}
}
