using System;
using System.Text;
using Verse.ParserDescriptors;
using Verse.PrinterDescriptors;
using Verse.Schemas.JSON;

namespace Verse.Schemas
{
    /// <summary>
    /// Schema implementation using JSON format.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public class JSONSchema<TEntity> : AbstractSchema<TEntity>
    {
        #region Properties

        /// <inheritdoc/>
        public override IPrinterDescriptor<TEntity> PrinterDescriptor
        {
            get
            {
                return this.printerDescriptor;
            }
        }

        /// <inheritdoc/>
        public override IParserDescriptor<TEntity> ParserDescriptor
        {
            get
            {
                return this.parserDescriptor;
            }
        }

        #endregion

        #region Attributes

        private readonly JSONSettings settings;

        private readonly RecurseParserDescriptor<TEntity, Value, ReaderState> parserDescriptor;

        private readonly RecursePrinterDescriptor<TEntity, Value, WriterState> printerDescriptor;

        private readonly ValueDecoder valueDecoder;

        private readonly ValueEncoder valueEncoder;

        #endregion

        #region Constructors

        /// <summary>
        /// Create new JSON schema using given settings
        /// </summary>
        /// <param name="settings">Text encoding, ignore null...</param>
        public JSONSchema(JSONSettings settings)
        {
            ValueDecoder decoder;
            ValueEncoder encoder;

            decoder = new ValueDecoder();
            encoder = new ValueEncoder();

            this.settings = settings;
            this.parserDescriptor = new RecurseParserDescriptor<TEntity, Value, ReaderState>(decoder, new Reader<TEntity>(settings.Encoding));
            this.printerDescriptor = new RecursePrinterDescriptor<TEntity, Value, WriterState>(encoder, new Writer<TEntity>(settings));
            this.valueDecoder = decoder;
            this.valueEncoder = encoder;
        }

        /// <summary>
        /// Create new JSON schema using given text encoding.
        /// </summary>
        /// <param name="encoding">Text encoding</param>
        public JSONSchema(Encoding encoding)
            : this(new JSONSettings(encoding, false))
        {
        }

        /// <summary>
        /// Create JSON schema using default UTF8 encoding.
        /// </summary>
        public JSONSchema() :
            this(new UTF8Encoding(false))
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override IParser<TEntity> CreateParser()
        {
            return this.parserDescriptor.CreateParser();
        }

        /// <inheritdoc/>
        public override IPrinter<TEntity> CreatePrinter()
        {
            return this.printerDescriptor.CreatePrinter();
        }

        /// <summary>
        /// Declare decoder to convert JSON native value into target output type.
        /// </summary>
        /// <typeparam name="TOutput">Target output type</typeparam>
        /// <param name="converter">Converter from JSON native value to output type</param>
        public void SetDecoder<TOutput>(Converter<Value, TOutput> converter)
        {
            this.valueDecoder.Set(converter);
        }

        /// <summary>
        /// Declare encoder to convert target input type into JSON native value.
        /// </summary>
        /// <typeparam name="TInput">Target input type</typeparam>
        /// <param name="converter">Converter from input type to JSON native value</param>
        public void SetEncoder<TInput>(Converter<TInput, Value> converter)
        {
            this.valueEncoder.Set(converter);
        }

        #endregion
    }
}