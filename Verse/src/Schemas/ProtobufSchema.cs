using System;
using System.IO;
using Verse.DecoderDescriptors;
using Verse.DecoderDescriptors.Tree;
using Verse.EncoderDescriptors;
using Verse.EncoderDescriptors.Tree;
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

		private readonly LegacyDecoderConverter legacyDecoderConverter;

		private readonly TreeDecoderDescriptor<TEntity, LegacyReaderState, ProtobufValue> legacyDecoderDescriptor;

		private readonly LegacyEncoderConverter legacyEncoderConverter;

		private readonly TreeEncoderDescriptor<TEntity, LegacyWriterState, ProtobufValue> legacyEncoderDescriptor;

		public ProtobufSchema(TextReader proto, string messageName, ProtobufConfiguration configuration)
		{
			// Native implementation
			var bindings = Parser.Parse(proto).Resolve(messageName);
			var decoderConverter = new DecoderConverter();
			var encoderConverter = new EncoderConverter();
			var reader = new Reader<TEntity>(bindings, configuration.RejectUnknown);
			var writer = new Writer<TEntity>(bindings);

			this.decoderConverter = decoderConverter;
			this.decoderDescriptor =
				new TreeDecoderDescriptor<TEntity, ReaderState, ProtobufValue>(decoderConverter, reader);

			this.encoderConverter = encoderConverter;
			this.encoderDescriptor =
				new TreeEncoderDescriptor<TEntity, WriterState, ProtobufValue>(encoderConverter, writer);

			// Legacy implementation
			this.legacyDecoderConverter = null;
			this.legacyDecoderDescriptor = null;
			this.legacyEncoderConverter = null;
			this.legacyEncoderDescriptor = null;
		}

		public ProtobufSchema(TextReader proto, string messageName)
			: this(proto, messageName, default)
		{
		}

		public ProtobufSchema()
		{
			// Native implementation
			this.decoderConverter = null;
			this.decoderDescriptor = null;
			this.encoderConverter = null;
			this.encoderDescriptor = null;

			// Legacy implementation
			var decoderConverter = new LegacyDecoderConverter();
			var encoderConverter = new LegacyEncoderConverter();
			var reader = new ReaderDefinition<LegacyReaderState, ProtobufValue, TEntity>();
			var writer = new WriterDefinition<LegacyWriterState, ProtobufValue, TEntity>();

			this.legacyDecoderConverter = decoderConverter;
			this.legacyDecoderDescriptor =
				new TreeDecoderDescriptor<TEntity, LegacyReaderState, ProtobufValue>(decoderConverter, reader);
			this.legacyEncoderConverter = encoderConverter;
			this.legacyEncoderDescriptor =
				new TreeEncoderDescriptor<TEntity, LegacyWriterState, ProtobufValue>(encoderConverter, writer);
		}

		public IDecoder<TEntity> CreateDecoder()
		{
			if (this.legacyDecoderDescriptor != null)
				return this.legacyDecoderDescriptor.CreateDecoder(new LegacyReaderSession());

			return this.decoderDescriptor.CreateDecoder(new ReaderSession());
		}

		public IEncoder<TEntity> CreateEncoder()
		{
			if (this.legacyEncoderDescriptor != null)
				return this.legacyEncoderDescriptor.CreateEncoder(new LegacyWriterSession());

			return this.encoderDescriptor.CreateEncoder(new WriterSession());
		}

		public void SetDecoderConverter<U>(Converter<ProtobufValue, U> converter)
		{
			if (this.legacyDecoderConverter != null)
				this.legacyDecoderConverter.Set(converter);
			else
				this.decoderConverter.Set(converter);
		}

		public void SetEncoderConverter<U>(Converter<U, ProtobufValue> converter)
		{
			if (this.legacyEncoderConverter != null)
				this.legacyEncoderConverter.Set(converter);
			else
				this.encoderConverter.Set(converter);
		}
	}
}
