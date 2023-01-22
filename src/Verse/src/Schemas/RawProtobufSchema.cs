using System;
using Verse.DecoderDescriptors;
using Verse.EncoderDescriptors;
using Verse.Schemas.RawProtobuf;

namespace Verse.Schemas
{
	/// <inheritdoc />
	/// <summary>
	/// Protobuf serialization implementation using implicitly-typed fields (no schema declaration).
	/// See: https://developers.google.com/protocol-buffers/docs/encoding
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	public sealed class RawProtobufSchema<TEntity> : ISchema<RawProtobufValue, TEntity>
	{
		/// <inheritdoc/>
		public IDecoderAdapter<RawProtobufValue> DecoderAdapter => this.decoderAdapter;

		/// <inheritdoc/>
		public IDecoderDescriptor<RawProtobufValue, TEntity> DecoderDescriptor => this.decoderDescriptor;

		private readonly RawProtobufConfiguration configuration;

		/// <inheritdoc/>
		public IEncoderAdapter<RawProtobufValue> EncoderAdapter => this.encoderAdapter;

		/// <inheritdoc/>
		public IEncoderDescriptor<RawProtobufValue, TEntity> EncoderDescriptor => this.encoderDescriptor;

		private readonly RawProtobufDecoderAdapter decoderAdapter;

		private readonly TreeDecoderDescriptor<ReaderState, RawProtobufValue, char, TEntity>
			decoderDescriptor;

		private readonly RawProtobufEncoderAdapter encoderAdapter;

		private readonly TreeEncoderDescriptor<WriterState, RawProtobufValue, TEntity> encoderDescriptor;

		public RawProtobufSchema(RawProtobufConfiguration configuration)
		{
			var readerDefinition = new ReaderDefinition<TEntity>();
			var writerDefinition = new WriterDefinition<TEntity>();

			this.configuration = configuration;
			this.decoderAdapter = new RawProtobufDecoderAdapter();
			this.decoderDescriptor =
				new TreeDecoderDescriptor<ReaderState, RawProtobufValue, char, TEntity>(readerDefinition);
			this.encoderAdapter = new RawProtobufEncoderAdapter();
			this.encoderDescriptor =
				new TreeEncoderDescriptor<WriterState, RawProtobufValue, TEntity>(writerDefinition);
		}

		public RawProtobufSchema() :
			this(new RawProtobufConfiguration())
		{
		}

		public IDecoder<TEntity> CreateDecoder()
		{
			return this.decoderDescriptor.CreateDecoder(new Reader(this.configuration.NoZigZagEncoding));
		}

		public IEncoder<TEntity> CreateEncoder()
		{
			return this.encoderDescriptor.CreateEncoder(new Writer(this.configuration.NoZigZagEncoding));
		}
	}
}
