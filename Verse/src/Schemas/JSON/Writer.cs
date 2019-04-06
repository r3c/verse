using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Base;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.JSON
{
	class Writer<TEntity> : TreeWriter<TEntity, WriterState, JSONValue>
	{
		private readonly Dictionary<string, EntityWriter<TEntity, WriterState>> fields = new Dictionary<string, EntityWriter<TEntity, WriterState>>();

		public override TreeWriter<TOther, WriterState, JSONValue> Create<TOther>()
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
			state.ArrayBegin();

            foreach (var item in elements)
                this.WriteEntity(item, state);

            state.ArrayEnd();
		}

		public override void WriteEntity(TEntity source, WriterState state)
		{
		    if (source == null)
				state.Value(JSON.JSONValue.Void);
			else if (this.IsArray)
				this.WriteArray(source, state);
			else if (this.IsValue)
				state.Value(this.ConvertValue(source));
		    else
		    {
		        state.ObjectBegin();

		        foreach (var field in this.fields)
		        {
		            state.Key(field.Key);

		            field.Value(source, state);
                }

		        state.ObjectEnd();
		    }
		}
	}
}