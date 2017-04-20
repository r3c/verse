using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Abstract;
using Verse.EncoderDescriptors.Recurse;

namespace Verse.Schemas.Protobuf
{
	class Writer<TEntity> : RecurseWriter<TEntity, WriterState, ProtobufValue>
	{
		private readonly Dictionary<string, EntityWriter<TEntity, WriterState>> fields = new Dictionary<string, EntityWriter<TEntity, WriterState>>();

		#region Methods

		public override RecurseWriter<TOther, WriterState, ProtobufValue> Create<TOther>()
		{
			return new Writer<TOther>();
		}

		public override void DeclareField(string name, EntityWriter<TEntity, WriterState> enter)
		{
			if (this.fields.ContainsKey(name))
				throw new InvalidOperationException("can't declare same field '" + name + "' twice on same descriptor");

			this.fields[name] = enter;
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

		#endregion
	}
}
