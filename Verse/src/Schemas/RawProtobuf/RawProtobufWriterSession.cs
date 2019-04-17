using System.Collections.Generic;
using System.IO;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.RawProtobuf
{
	internal class RawProtobufWriterSession : IWriterSession<RawProtobufWriterState, RawProtobufValue>
	{
		public bool Start(Stream stream, EncodeError error, out RawProtobufWriterState state)
		{
			state = new RawProtobufWriterState(stream, error);

			return true;
		}

		public void Stop(RawProtobufWriterState state)
		{
		}

		public void WriteArray<TEntity>(RawProtobufWriterState state, IEnumerable<TEntity> elements, WriterCallback<RawProtobufWriterState, RawProtobufValue, TEntity> writer)
		{
			foreach (var element in elements)
				writer(this, state, element);
		}

		public void WriteObject<TEntity>(RawProtobufWriterState state, TEntity source, IReadOnlyDictionary<string, WriterCallback<RawProtobufWriterState, RawProtobufValue, TEntity>> fields)
		{
			state.ObjectBegin();

			foreach (var field in fields)
			{
				if (field.Key.Length > 1 && field.Key[0] == '_')
					state.Key(field.Key.Substring(1));
				else
					state.Key(field.Key);

				field.Value(this, state, source);
			}

			state.ObjectEnd();
		}

		public void WriteValue(RawProtobufWriterState state, RawProtobufValue value)
		{
			if (!state.Value(value))
				state.Error("failed to write value");
		}
	}
}
