using System;
using System.IO;
using Verse.DecoderDescriptors;
using Verse.EncoderDescriptors;
using Verse.Schemas.Protobuf;
using Verse.Schemas.Protobuf.Definition;

namespace Verse.Schemas
{
	public sealed class ProtobufSchema<TEntity> : ISchema<TEntity>
	{
		public IDecoderDescriptor<TEntity> DecoderDescriptor => this.decoderDescriptor;

		public IEncoderDescriptor<TEntity> EncoderDescriptor => this.encoderDescriptor;

		private readonly DecoderConverter decoderConverter;

		private readonly TreeDecoderDescriptor<ReaderState, ProtobufValue, TEntity> decoderDescriptor;

		private readonly EncoderConverter encoderConverter;

		private readonly TreeEncoderDescriptor<WriterState, ProtobufValue, TEntity> encoderDescriptor;

		public ProtobufSchema(TextReader proto, string messageName, ProtobufConfiguration configuration)
		{
			var bindings = Parser.Parse(proto).Resolve(messageName);
			var decoderConverter = new DecoderConverter();
			var encoderConverter = new EncoderConverter();
			var reader = new ProtobufReaderDefinition<TEntity>(bindings, configuration.RejectUnknown);
			var writer = new ProtobufWriterDefinition<TEntity>(bindings);

			this.decoderConverter = decoderConverter;
			this.decoderDescriptor =
				new TreeDecoderDescriptor<ReaderState, ProtobufValue, TEntity>(decoderConverter, reader);

			this.encoderConverter = encoderConverter;
			this.encoderDescriptor =
				new TreeEncoderDescriptor<WriterState, ProtobufValue, TEntity>(encoderConverter, writer);
		}

		public ProtobufSchema(TextReader proto, string messageName)
			: this(proto, messageName, default)
		{
		}

		public IDecoder<TEntity> CreateDecoder(Func<TEntity> constructor)
		{
			return this.decoderDescriptor.CreateDecoder(new Reader(), constructor);
		}

		public IEncoder<TEntity> CreateEncoder()
		{
			return this.encoderDescriptor.CreateEncoder(new Writer());
		}

		public void SetDecoderConverter<TValue>(Converter<ProtobufValue, TValue> converter)
		{
			this.decoderConverter.Set(converter);
		}

		public void SetEncoderConverter<TValue>(Converter<TValue, ProtobufValue> converter)
		{
			this.encoderConverter.Set(converter);
		}
	}
}
