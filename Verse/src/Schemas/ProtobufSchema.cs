using System;
using Verse.ParserDescriptors;
using Verse.PrinterDescriptors;
using Verse.Schemas.Protobuf;

namespace Verse.Schemas
{
    public class ProtobufSchema<TEntity> : AbstractSchema<TEntity>
    {
        #region Properties

        public override IPrinterDescriptor<TEntity> PrinterDescriptor
        {
            get
            {
                return this.printerDescriptor;
            }
        }

        public override IParserDescriptor<TEntity> ParserDescriptor
        {
            get
            {
                return this.parserDescriptor;
            }
        }

        #endregion

        #region Attributes

        private readonly DecoderConverter decoderConverter;

        private readonly EncoderConverter encoderConverter;

        private readonly RecurseParserDescriptor<TEntity, Value, ReaderState> parserDescriptor;

        private readonly RecursePrinterDescriptor<TEntity, Value, WriterState> printerDescriptor;

        #endregion

        #region Constructor

        public ProtobufSchema()
        {
            var sourceConverter = new DecoderConverter();
            var targetConverter = new EncoderConverter();

            this.decoderConverter = sourceConverter;
            this.encoderConverter = targetConverter;
            this.parserDescriptor = new RecurseParserDescriptor<TEntity, Value, ReaderState>(sourceConverter, new Reader<TEntity>());
            this.printerDescriptor = new RecursePrinterDescriptor<TEntity, Value, WriterState>(targetConverter, new Writer<TEntity>());
        }

        #endregion

        #region Methods / Public

        public override IParser<TEntity> CreateParser()
        {
            return this.parserDescriptor.CreateParser();
        }

        public override IPrinter<TEntity> CreatePrinter()
        {
            return this.printerDescriptor.CreatePrinter();
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
