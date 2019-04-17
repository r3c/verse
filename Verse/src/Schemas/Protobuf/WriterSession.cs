using System;
using System.IO;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.Protobuf
{
	class WriterSession : IWriterSession<WriterState, ProtobufValue>
	{
        public bool Start(Stream stream, EncodeError error, out WriterState state)
        {
            throw new NotImplementedException();
        }
    
        public void Stop(WriterState state)
        {
            throw new NotImplementedException();
        }

		public void WriteArray<TEntity>(WriterState state, System.Collections.Generic.IEnumerable<TEntity> elements, WriterCallback<WriterState, ProtobufValue, TEntity> writer)
		{
			throw new NotImplementedException();
		}

		public void WriteObject<TEntity>(WriterState state, TEntity entity, System.Collections.Generic.IReadOnlyDictionary<string, WriterCallback<WriterState, ProtobufValue, TEntity>> fields)
		{
			throw new NotImplementedException();
		}

		public void WriteValue(WriterState state, ProtobufValue value)
		{
			throw new NotImplementedException();
		}
	}
}
