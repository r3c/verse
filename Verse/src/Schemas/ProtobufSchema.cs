using System;
using Verse.ParserDescriptors;
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
                throw new NotImplementedException("printing not implemented");
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

        private readonly RecurseParserDescriptor<TEntity, ReaderContext, Value> parserDescriptor;

        private readonly ValueDecoder valueDecoder;

        #endregion

        #region Constructor

        public ProtobufSchema()
        {
            ValueDecoder decoder;

            decoder = new ValueDecoder();

            this.parserDescriptor = new RecurseParserDescriptor<TEntity, ReaderContext, Value>(decoder);
            this.valueDecoder = decoder;
        }

        #endregion

        #region Methods / Public

        public override IParser<TEntity> CreateParser()
        {
            return this.parserDescriptor.CreateParser(new Reader());
        }

        public override IPrinter<TEntity> CreatePrinter()
        {
            throw new NotImplementedException("printing not implemented");
        }

        public void SetDecoder<U>(Converter<Value, U> converter)
        {
            this.valueDecoder.Set(converter);
        }

        #endregion
    }
}
