using System.Collections.Generic;
using System.IO;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.RawProtobuf
{
	internal class RawProtobufWriter : IWriter<RawProtobufWriterState, RawProtobufValue>
	{
		private readonly bool noZigZagEncoding;

		public RawProtobufWriter(bool noZigZagEncoding)
		{
			this.noZigZagEncoding = noZigZagEncoding;
		}

		public RawProtobufWriterState Start(Stream stream, ErrorEvent error)
		{
			return new RawProtobufWriterState(stream, error, this.noZigZagEncoding);
		}

		public void Stop(RawProtobufWriterState state)
		{
			state.Flush();
		}

		public void WriteAsArray<TEntity>(RawProtobufWriterState state, IEnumerable<TEntity> elements,
			WriterCallback<RawProtobufWriterState, RawProtobufValue, TEntity> writer)
		{
			foreach (var element in elements)
			{
				var fieldIndex = state.FieldIndex;

				writer(this, state, element);

				state.FieldIndex = fieldIndex;
			}
		}

		public void WriteAsObject<TEntity>(RawProtobufWriterState state, TEntity source,
			IReadOnlyDictionary<string, WriterCallback<RawProtobufWriterState, RawProtobufValue, TEntity>> fields)
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

		public void WriteAsValue(RawProtobufWriterState state, RawProtobufValue value)
		{
			state.Value(value);
		}
	}
}
