using System;
using System.Collections.Generic;
using System.IO;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.Protobuf
{
	internal class WriterSession : IWriterSession<WriterState, ProtobufValue>
	{
		public WriterState Start(Stream stream, ErrorEvent error)
		{
			throw new NotImplementedException();
		}

		public void Stop(WriterState state)
		{
			throw new NotImplementedException();
		}

		public void WriteArray<TEntity>(WriterState state, IEnumerable<TEntity> elements,
			WriterCallback<WriterState, ProtobufValue, TEntity> writer)
		{
			throw new NotImplementedException();
		}

		public void WriteObject<TEntity>(WriterState state, TEntity entity,
			IReadOnlyDictionary<string, WriterCallback<WriterState, ProtobufValue, TEntity>> fields)
		{
			throw new NotImplementedException();
		}

		public void WriteValue(WriterState state, ProtobufValue value)
		{
			throw new NotImplementedException();
		}
	}
}
