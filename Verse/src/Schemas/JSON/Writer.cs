using System.Collections.Generic;
using System.IO;
using System.Text;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.JSON
{
	internal class Writer : IWriter<WriterState, JSONValue>
	{
		private readonly Encoding encoding;

		private readonly bool omitNull;

		public Writer(Encoding encoding, bool omitNull)
		{
			this.encoding = encoding;
			this.omitNull = omitNull;
		}

		public WriterState Start(Stream stream, ErrorEvent error)
		{
			return new WriterState(stream, this.encoding, this.omitNull);
		}

		public void Stop(WriterState state)
		{
			state.Dispose();
		}

		public void WriteAsArray<TElement>(WriterState state, IEnumerable<TElement> elements,
			WriterCallback<WriterState, JSONValue, TElement> writer)
		{
			if (elements == null)
				this.WriteAsValue(state, JSONValue.Void);
			else
			{
				state.ArrayBegin();

				foreach (var element in elements)
					writer(this, state, element);

				state.ArrayEnd();
			}
		}

		public void WriteAsObject<TObject>(WriterState state, TObject parent,
			IReadOnlyDictionary<string, WriterCallback<WriterState, JSONValue, TObject>> fields)
		{
			if (parent == null)
				this.WriteAsValue(state, JSONValue.Void);
			else
			{
				state.ObjectBegin();

				foreach (var field in fields)
				{
					state.Key(field.Key);
					field.Value(this, state, parent);
				}

				state.ObjectEnd();
			}
		}

		public void WriteAsValue(WriterState state, JSONValue value)
		{
			state.Value(value);
		}

		public void WriteAsRawValue(WriterState state, JSONValue value)
		{
			if (value.Type == JSONType.String)
			{
				state.RawJson(value.String);
			}
			else
			{
				// Normally, it is expected that raw values are only string
				// But we can easily write raw JSON from null, booleans and numbers
				this.WriteAsValue(state, value);
			}
		}
	}
}
