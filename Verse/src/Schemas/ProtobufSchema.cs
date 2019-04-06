using System;
using System.IO;
using Verse.DecoderDescriptors;
using Verse.EncoderDescriptors;
using Verse.Schemas.Protobuf;
using Verse.Schemas.Protobuf.Definition;
using Verse.Schemas.Protobuf.Legacy;

namespace Verse.Schemas
{
	public class ProtobufSchema<TEntity> : ISchema<TEntity>
	{
		public IDecoderDescriptor<TEntity> DecoderDescriptor => this.decoderDescriptor as IDecoderDescriptor<TEntity> ?? this.legacyDecoderDescriptor;

	    public IEncoderDescriptor<TEntity> EncoderDescriptor => this.encoderDescriptor as IEncoderDescriptor<TEntity> ?? this.legacyEncoderDescriptor;

	    private readonly DecoderConverter decoderConverter;

		private readonly TreeDecoderDescriptor<TEntity, ReaderState, ProtobufValue> decoderDescriptor;

		private readonly EncoderConverter encoderConverter;

		private readonly TreeEncoderDescriptor<TEntity, WriterState, ProtobufValue> encoderDescriptor;

		private readonly TreeDecoderDescriptor<TEntity, LegacyReaderState, ProtobufValue> legacyDecoderDescriptor;
		
		private readonly TreeEncoderDescriptor<TEntity, LegacyWriterState, ProtobufValue> legacyEncoderDescriptor;

		public ProtobufSchema(TextReader proto, string messageName, ProtobufConfiguration configuration)
        {
            var bindings = Parser.Parse(proto).Resolve(messageName);
			var decoders = new DecoderConverter();
			var encoders = new EncoderConverter();

			// Native implementation
			this.decoderConverter = decoders;
			this.decoderDescriptor = new TreeDecoderDescriptor<TEntity, ReaderState, ProtobufValue>(decoders, new ReaderSession(), new Reader<TEntity>(bindings, configuration.RejectUnknown));
			this.encoderConverter = encoders;
			this.encoderDescriptor = new TreeEncoderDescriptor<TEntity, WriterState, ProtobufValue>(encoders, new WriterSession(), new Writer<TEntity>(bindings));

			// Legacy implementation
			this.legacyDecoderDescriptor = null;
			this.legacyEncoderDescriptor = null;
		}

		public ProtobufSchema(TextReader proto, string messageName)
			: this(proto, messageName, new ProtobufConfiguration())
		{
		}

        public ProtobufSchema()
        {
            var decoders = new DecoderConverter();
            var encoders = new EncoderConverter();

            // Native implementation
            this.decoderConverter = decoders;
            this.decoderDescriptor = null;
            this.encoderConverter = encoders;
            this.encoderDescriptor = null;

            // Legacy implementation
            this.legacyDecoderDescriptor = new TreeDecoderDescriptor<TEntity, LegacyReaderState, ProtobufValue>(decoders, new LegacyReaderSession(), new LegacyReader<TEntity>());
            this.legacyEncoderDescriptor = new TreeEncoderDescriptor<TEntity, LegacyWriterState, ProtobufValue>(encoders, new LegacyWriterSession(), new LegacyWriter<TEntity>());
        }

		public IDecoder<TEntity> CreateDecoder()
		{
            if (this.legacyDecoderDescriptor != null)
                return this.legacyDecoderDescriptor.CreateDecoder();

			return this.decoderDescriptor.CreateDecoder();
		}

		public IEncoder<TEntity> CreateEncoder()
		{
            if (this.legacyEncoderDescriptor != null)
                return this.legacyEncoderDescriptor.CreateEncoder();

			return this.encoderDescriptor.CreateEncoder();
		}

		public void SetDecoderConverter<U>(Converter<ProtobufValue, U> converter)
		{
			this.decoderConverter.Set(converter);
		}

		public void SetEncoderConverter<U>(Converter<U, ProtobufValue> converter)
		{
			this.encoderConverter.Set(converter);
		}
	}
}
