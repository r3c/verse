using System;
using System.Text;
using Verse.DecoderDescriptors;
using Verse.Schemas.QueryString;

namespace Verse.Schemas
{
    public sealed class QueryStringSchema<TEntity> : AbstractSchema<TEntity>
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
                throw new NotImplementedException("encoding not implemented");
            }
        }

        #endregion

        #region Attributes

        private readonly DecoderConverter decoderConverter;

        private readonly FlatDecoderDescriptor<TEntity, ReaderContext> decoderDescriptor;

        private readonly Encoding encoding;

        #endregion

        #region Constructor

        public QueryStringSchema(Encoding encoding)
        {
            var sourceConverter = new DecoderConverter();

            this.decoderConverter = sourceConverter;
            this.decoderDescriptor = new FlatDecoderDescriptor<TEntity, ReaderContext>(sourceConverter);
            this.encoding = encoding;
        }

        public QueryStringSchema() :
            this(new UTF8Encoding(false))
        {
        }

        #endregion

        #region Methods / Public

        public override IDecoder<TEntity> CreateDecoder()
        {
            return this.decoderDescriptor.CreateDecoder(new Reader(this.encoding));
        }

        public override IEncoder<TEntity> CreateEncoder()
        {
            throw new NotImplementedException("encoding not implemented");
        }

        public void SetDecoder<U>(Converter<string, U> converter)
        {
            this.decoderConverter.Set(converter);
        }

        #endregion
    }
}
