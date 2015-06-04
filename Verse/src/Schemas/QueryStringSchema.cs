using System;
using System.Text;
using Verse.ParserDescriptors;
using Verse.Schemas.QueryString;

namespace Verse.Schemas
{
    public sealed class QueryStringSchema<TEntity> : AbstractSchema<TEntity>
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

        private readonly ValueDecoder decoder;

        private readonly Encoding encoding;

        private readonly FlatParserDescriptor<TEntity, ReaderContext> parserDescriptor;



        #endregion

        #region Constructor

        public QueryStringSchema(Encoding encoding)
        {
            ValueDecoder valueDecoder;

            valueDecoder = new ValueDecoder();

            this.decoder = valueDecoder;
            this.encoding = encoding;
            this.parserDescriptor = new FlatParserDescriptor<TEntity, ReaderContext>(this.decoder);
        }

        public QueryStringSchema() :
            this(new UTF8Encoding(false))
        {
        }

        #endregion

        #region Methods / Public

        public override IParser<TEntity> CreateParser()
        {
            return this.parserDescriptor.CreateParser(new Reader(this.encoding));
        }

        public override IPrinter<TEntity> CreatePrinter()
        {
            throw new NotImplementedException("printing not implemented");
        }

        public void SetDecoder<U>(Converter<string, U> converter)
        {
            this.decoder.Set(converter);
        }

        #endregion
    }
}
