using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Recurse;
using Verse.EncoderDescriptors.Recurse.RecurseWriters;

namespace Verse.Schemas.JSON
{
	class Writer<TEntity> : PatternRecurseWriter<TEntity, WriterState, JSONValue>
	{
		#region Methods

		public override IRecurseWriter<TOther, WriterState, JSONValue> Create<TOther>()
		{
			return new Writer<TOther>();
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
			IEnumerator<KeyValuePair<string, WriteEntity<TEntity, WriterState>>> field;

			if (source == null)
				state.Value(JSON.JSONValue.Void);
			else if (this.IsArray)
				this.ProcessArray(source, state);
			else if (this.IsValue)
				state.Value(this.ProcessValue(source));
			else
			{
				state.ObjectBegin();
				field = this.Fields.GetEnumerator();

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