using System.Collections.Generic;
using System.IO;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.Protobuf.Legacy
{
	internal class LegacyWriterSession : IWriterSession<LegacyWriterState, ProtobufValue>
	{
		public bool Start(Stream stream, EncodeError error, out LegacyWriterState state)
		{
			state = new LegacyWriterState(stream, error);

			return true;
		}

		public void Stop(LegacyWriterState state)
		{
		}

		public void WriteArray<TEntity>(LegacyWriterState state, IEnumerable<TEntity> elements, WriterCallback<LegacyWriterState, ProtobufValue, TEntity> writer)
		{
			foreach (var element in elements)
				writer(this, state, element);
		}

		public void WriteObject<TEntity>(LegacyWriterState state, TEntity source, IReadOnlyDictionary<string, WriterCallback<LegacyWriterState, ProtobufValue, TEntity>> fields)
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

		public void WriteValue(LegacyWriterState state, ProtobufValue value)
		{
			if (!state.Value(value))
				state.Error("failed to write value");
		}
	}
}
