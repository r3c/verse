using System.Collections.Generic;
using System.IO;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.RawProtobuf
{
	internal class Writer : IWriter<WriterState, RawProtobufValue>
	{
		private readonly bool noZigZagEncoding;

		public Writer(bool noZigZagEncoding)
		{
			this.noZigZagEncoding = noZigZagEncoding;
		}

		public void Flush(WriterState state)
		{
			state.Flush();
		}

		public WriterState Start(Stream stream, ErrorEvent error)
		{
			return new WriterState(stream, error, noZigZagEncoding);
		}

		public void Stop(WriterState state)
		{
		}

		public void WriteAsArray<TEntity>(WriterState state, IEnumerable<TEntity> elements,
			WriterCallback<WriterState, RawProtobufValue, TEntity> writer)
		{
			foreach (var element in elements)
			{
				var fieldIndex = state.FieldIndex;

				writer(this, state, element);

				state.FieldIndex = fieldIndex;
			}
		}

		public void WriteAsObject<TEntity>(WriterState state, TEntity source,
			IReadOnlyDictionary<string, WriterCallback<WriterState, RawProtobufValue, TEntity>> fields)
		{
			var marker = state.ObjectBegin();

			foreach (var field in fields)
			{
				if (field.Key.Length > 1 && field.Key[0] == '_')
					state.Key(field.Key.Substring(1));
				else
					state.Key(field.Key);

				field.Value(this, state, source);
			}

			state.ObjectEnd(marker);
		}

		public void WriteAsValue(WriterState state, RawProtobufValue value)
		{
			state.Value(value);
		}
	}
}
