using System;
using Verse.DecoderDescriptors;
using Verse.DecoderDescriptors.Tree;
using Verse.EncoderDescriptors;
using Verse.EncoderDescriptors.Tree;
using Verse.Schemas.RawProtobuf;

namespace Verse.Schemas
{
	public sealed class RawProtobufSchema<TEntity> : ISchema<TEntity>
	{
		public IDecoderDescriptor<TEntity> DecoderDescriptor => this.decoderDescriptor;

		public IEncoderDescriptor<TEntity> EncoderDescriptor => this.encoderDescriptor;

		private readonly RawProtobufConfiguration configuration;

		private readonly RawProtobufDecoderConverter decoderConverter;

		private readonly TreeDecoderDescriptor<RawProtobufReaderState, RawProtobufValue, TEntity> decoderDescriptor;

		private readonly RawProtobufEncoderConverter encoderConverter;

		private readonly TreeEncoderDescriptor<RawProtobufWriterState, RawProtobufValue, TEntity> encoderDescriptor;

		public RawProtobufSchema(RawProtobufConfiguration configuration)
		{
			var decoderConverter = new RawProtobufDecoderConverter();
			var encoderConverter = new RawProtobufEncoderConverter();
			var readerDefinition = new ReaderDefinition<RawProtobufReaderState, RawProtobufValue, TEntity>();
			var writerDefinition = new WriterDefinition<RawProtobufWriterState, RawProtobufValue, TEntity>();

			this.configuration = configuration;
			this.decoderConverter = decoderConverter;
			this.decoderDescriptor =
				new TreeDecoderDescriptor<RawProtobufReaderState, RawProtobufValue, TEntity>(decoderConverter,
					readerDefinition);
			this.encoderConverter = encoderConverter;
			this.encoderDescriptor =
				new TreeEncoderDescriptor<RawProtobufWriterState, RawProtobufValue, TEntity>(encoderConverter,
					writerDefinition);
		}

		public RawProtobufSchema() :
			this(new RawProtobufConfiguration())
		{
		}

		public IDecoder<TEntity> CreateDecoder(Func<TEntity> constructor)
		{
			return this.decoderDescriptor.CreateDecoder(new RawProtobufReader(this.configuration.NoZigZagEncoding),
				constructor);
		}

		public IEncoder<TEntity> CreateEncoder()
		{
			return this.encoderDescriptor.CreateEncoder(new RawProtobufWriter(this.configuration.NoZigZagEncoding));
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
