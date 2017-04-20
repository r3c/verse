using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Abstract;
using Verse.EncoderDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
	class Writer<TEntity> : RecurseWriter<TEntity, WriterState, JSONValue>
	{
		private readonly Dictionary<string, EntityWriter<TEntity, WriterState>> fields = new Dictionary<string, EntityWriter<TEntity, WriterState>>();

		#region Methods

		public override RecurseWriter<TOther, WriterState, JSONValue> Create<TOther>()
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
			IEnumerator<TEntity> item;

			state.ArrayBegin();
			item = elements.GetEnumerator();

			while (item.MoveNext())
				this.WriteEntity(item.Current, state);

			state.ArrayEnd();
		}

		public override void WriteEntity(TEntity source, WriterState state)
		{
			IEnumerator<KeyValuePair<string, EntityWriter<TEntity, WriterState>>> field;

			if (source == null)
				state.Value(JSON.JSONValue.Void);
			else if (this.IsArray)
				this.ProcessArray(source, state);
			else if (this.IsValue)
				state.Value(this.ProcessValue(source));
			else
			{
				state.ObjectBegin();
				field = this.fields.GetEnumerator();

				while (field.MoveNext())
				{
					state.Key(field.Current.Key);

					field.Current.Value(source, state);
				}

				state.ObjectEnd();
			}
		}

		#endregion
	}
}