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
	public sealed class RawProtobufSchema<TEntity> : ISchema<TEntity>
	{
		public IDecoderDescriptor<TEntity> DecoderDescriptor => this.decoderDescriptor;

		public IEncoderDescriptor<TEntity> EncoderDescriptor => this.encoderDescriptor;

		private readonly RawProtobufConfiguration configuration;

		private readonly RawProtobufDecoderConverter decoderConverter;

		private readonly TreeDecoderDescriptor<ReaderState, RawProtobufValue, char, TEntity>
			decoderDescriptor;

		private readonly RawProtobufEncoderConverter encoderConverter;

		private readonly TreeEncoderDescriptor<WriterState, RawProtobufValue, TEntity> encoderDescriptor;

		public RawProtobufSchema(RawProtobufConfiguration configuration)
		{
			var decoderConverter = new RawProtobufDecoderConverter();
			var encoderConverter = new RawProtobufEncoderConverter();
			var readerDefinition = new ReaderDefinition<TEntity>();
			var writerDefinition = new WriterDefinition<TEntity>();

			this.configuration = configuration;
			this.decoderConverter = decoderConverter;
			this.decoderDescriptor =
				new TreeDecoderDescriptor<ReaderState, RawProtobufValue, char, TEntity>(decoderConverter,
					readerDefinition);
			this.encoderConverter = encoderConverter;
			this.encoderDescriptor =
				new TreeEncoderDescriptor<WriterState, RawProtobufValue, TEntity>(encoderConverter,
					writerDefinition);
		}

		public RawProtobufSchema() :
			this(new RawProtobufConfiguration())
		{
		}

		public IDecoder<TEntity> CreateDecoder(Func<TEntity> constructor)
		{
			return this.decoderDescriptor.CreateDecoder(new Reader(this.configuration.NoZigZagEncoding),
				constructor);
		}

		public IEncoder<TEntity> CreateEncoder()
		{
			return this.encoderDescriptor.CreateEncoder(new Writer(this.configuration.NoZigZagEncoding));
		}

		public void SetDecoderConverter<TValue>(Converter<RawProtobufValue, TValue> converter)
		{
			this.decoderConverter.Set(converter);
		}

		public void SetEncoderConverter<TValue>(Converter<TValue, RawProtobufValue> converter)
		{
			this.encoderConverter.Set(converter);
		}
	}
}
