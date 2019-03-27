using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Abstract;
using Verse.EncoderDescriptors.Recurse;

namespace Verse.Schemas.Protobuf.Legacy
{
	class LegacyWriter<TEntity> : RecurseWriter<TEntity, LegacyWriterState, ProtobufValue>
	{
		private readonly Dictionary<string, EntityWriter<TEntity, LegacyWriterState>> fields = new Dictionary<string, EntityWriter<TEntity, LegacyWriterState>>();

		public override RecurseWriter<TOther, LegacyWriterState, ProtobufValue> Create<TOther>()
		{
			return new LegacyWriter<TOther>();
		}

		public override void DeclareField(string name, EntityWriter<TEntity, LegacyWriterState> enter)
		{
			if (this.fields.ContainsKey(name))
				throw new InvalidOperationException("can't declare same field '" + name + "' twice on same descriptor");

			this.fields[name] = enter;
		}

		public override void WriteElements(IEnumerable<TEntity> elements, LegacyWriterState state)
		{
			foreach (var item in elements)
				this.WriteEntity(item, state);
		}

		public override void WriteEntity(TEntity source, LegacyWriterState state)
		{
			if (source == null)
				return;

			if (this.IsArray)
				this.WriteArray(source, state);
			else if (this.IsValue)
			{
				if (!state.Value(this.ConvertValue(source)))
					state.Error("failed to write value");
			}
			else
			{
				state.ObjectBegin();

				foreach (var field in this.fields)
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
	}
}
