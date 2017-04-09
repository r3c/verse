using System;
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

        private readonly RecurseDecoderDescriptor<TEntity, Value, ReaderState> decoderDescriptor;

        private readonly EncoderConverter encoderConverter;

        private readonly RecurseEncoderDescriptor<TEntity, Value, WriterState> encoderDescriptor;

        #endregion

        #region Constructor

        public ProtobufSchema()
        {
            var sourceConverter = new DecoderConverter();
            var targetConverter = new EncoderConverter();

            this.decoderConverter = sourceConverter;
            this.encoderConverter = targetConverter;
            this.decoderDescriptor = new RecurseDecoderDescriptor<TEntity, Value, ReaderState>(sourceConverter, new Reader<TEntity>());
            this.encoderDescriptor = new RecurseEncoderDescriptor<TEntity, Value, WriterState>(targetConverter, new Writer<TEntity>());
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

        public void SetDecoder<U>(Converter<Value, U> converter)
        {
            this.decoderConverter.Set(converter);
        }

        public void SetEncoder<U>(Converter<U, Value> converter)
        {
            this.encoderConverter.Set(converter);
        }

        #endregion
    }
}
