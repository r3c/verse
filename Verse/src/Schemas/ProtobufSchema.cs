using System;
using System.IO;
using Verse.DecoderDescriptors;
using Verse.EncoderDescriptors;
using Verse.Schemas.Protobuf;
using Verse.Schemas.Protobuf.Definition;
using Verse.Schemas.Protobuf.Legacy;

namespace Verse.Schemas
{
	public class ProtobufSchema<TEntity> : AbstractSchema<TEntity>
	{
		#region Properties

		public override IDecoderDescriptor<TEntity> DecoderDescriptor
		{
			get
			{
				return this.decoderDescriptor as IDecoderDescriptor<TEntity> ?? this.legacyDecoderDescriptor;
			}
		}

		public override IEncoderDescriptor<TEntity> EncoderDescriptor
		{
			get
			{
				return this.encoderDescriptor as IEncoderDescriptor<TEntity> ?? this.legacyEncoderDescriptor;
			}
		}

		#endregion

		#region Attributes

		private readonly DecoderConverter decoderConverter;

		private readonly RecurseDecoderDescriptor<TEntity, ReaderState, ProtobufValue> decoderDescriptor;

		private readonly EncoderConverter encoderConverter;

		private readonly RecurseEncoderDescriptor<TEntity, WriterState, ProtobufValue> encoderDescriptor;

		private readonly RecurseDecoderDescriptor<TEntity, LegacyReaderState, ProtobufValue> legacyDecoderDescriptor;
		
		private readonly RecurseEncoderDescriptor<TEntity, LegacyWriterState, ProtobufValue> legacyEncoderDescriptor;

		#endregion

		#region Constructor

		public ProtobufSchema(TextReader proto, string messageName, ProtobufConfiguration configuration)
        {
            var bindings = Parser.Parse(proto).Resolve(messageName);
			var decoders = new DecoderConverter();
			var encoders = new EncoderConverter();

			// Native implementation
			this.decoderConverter = decoders;
			this.decoderDescriptor = new RecurseDecoderDescriptor<TEntity, ReaderState, ProtobufValue>(decoders, new ReaderSession(), new Reader<TEntity>(bindings, configuration.RejectUnknown));
			this.encoderConverter = encoders;
			this.encoderDescriptor = new RecurseEncoderDescriptor<TEntity, WriterState, ProtobufValue>(encoders, new WriterSession(), new Writer<TEntity>(bindings));

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
            this.legacyDecoderDescriptor = new RecurseDecoderDescriptor<TEntity, LegacyReaderState, ProtobufValue>(decoders, new LegacyReaderSession(), new LegacyReader<TEntity>());
            this.legacyEncoderDescriptor = new RecurseEncoderDescriptor<TEntity, LegacyWriterState, ProtobufValue>(encoders, new LegacyWriterSession(), new LegacyWriter<TEntity>());
        }

		#endregion

		#region Methods / Public

		public override IDecoder<TEntity> CreateDecoder()
		{
            if (this.legacyDecoderDescriptor != null)
                return this.legacyDecoderDescriptor.CreateDecoder();

			return this.decoderDescriptor.CreateDecoder();
		}

		public override IEncoder<TEntity> CreateEncoder()
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

		#endregion
	}
}
