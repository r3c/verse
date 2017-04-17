﻿using System;
using Verse.DecoderDescriptors;
using Verse.EncoderDescriptors;
using Verse.Schemas.Protobuf;

namespace Verse.Schemas
{
	public class ProtobufSchema<TEntity> : AbstractSchema<TEntity>
	{
		#region Properties

		public override IDecoderDescriptor<TEntity> DecoderDescriptor
		{
			get
			{
				return this.decoderDescriptor;
			}
		}

		public override IEncoderDescriptor<TEntity> EncoderDescriptor
		{
			get
			{
				return this.encoderDescriptor;
			}
		}

		#endregion

		#region Attributes

		private readonly DecoderConverter decoderConverter;

		private readonly RecurseDecoderDescriptor<TEntity, ReaderState, ProtobufValue> decoderDescriptor;

		private readonly EncoderConverter encoderConverter;

		private readonly RecurseEncoderDescriptor<TEntity, WriterState, ProtobufValue> encoderDescriptor;

		#endregion

		#region Constructor

		public ProtobufSchema()
		{
			var decoderConverter = new DecoderConverter();
			var encoderConverter = new EncoderConverter();

			this.decoderConverter = decoderConverter;
			this.encoderConverter = encoderConverter;
			this.decoderDescriptor = new RecurseDecoderDescriptor<TEntity, ReaderState, ProtobufValue>(decoderConverter, new ReaderSession(), new Reader<TEntity>());
			this.encoderDescriptor = new RecurseEncoderDescriptor<TEntity, WriterState, ProtobufValue>(encoderConverter, new WriterSession(), new Writer<TEntity>());
		}

		#endregion

		#region Methods / Public

		public override IDecoder<TEntity> CreateDecoder()
		{
			return this.decoderDescriptor.CreateDecoder();
		}

		public override IEncoder<TEntity> CreateEncoder()
		{
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
