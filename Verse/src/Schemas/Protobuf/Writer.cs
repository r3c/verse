using System;
using System.Collections.Generic;
using System.IO;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.Protobuf
{
	internal class Writer : IWriter<WriterState, ProtobufValue>
	{
		public WriterState Start(Stream stream, ErrorEvent error)
		{
			throw new NotImplementedException();
		}

		public void Stop(WriterState state)
		{
			throw new NotImplementedException();
		}

		public void WriteAsArray<TEntity>(WriterState state, IEnumerable<TEntity> elements,
			WriterCallback<WriterState, ProtobufValue, TEntity> writer)
		{
			throw new NotImplementedException();
		}

		public void WriteAsObject<TEntity>(WriterState state, TEntity entity,
			IReadOnlyDictionary<string, WriterCallback<WriterState, ProtobufValue, TEntity>> fields)
		{
			throw new NotImplementedException();
		}

		public void WriteAsValue(WriterState state, ProtobufValue value)
		{
			throw new NotImplementedException();
		}
	}
}
