using System;

namespace Verse
{
    /// <summary>
    /// Schema describes how to print (through a printer descriptor) or parse
    /// (through a parser descriptor) any instance of type
    /// <typeparamref name="TEntity"/> using a given serialization format which
    /// depends on the actual implementation.
    /// </summary>
    /// <typeparam name="TEntity">Associated entity type</typeparam>
    public interface ISchema<TEntity>
    {
        #region Properties

        /// <summary>
        /// Get parser descriptor for this schema and entity type.
        /// </summary>
        IParserDescriptor<TEntity> ParserDescriptor
        {
            get;
        }

        /// <summary>
        /// Get printer descriptor for this schema and entity type.
        /// </summary>
        IPrinterDescriptor<TEntity> PrinterDescriptor
        {
            get;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create an entity parser based on instructions passed to the
        /// parser descriptor associated to this schema.
        /// </summary>
        /// <returns>Parser descriptor</returns>
        IParser<TEntity> CreateParser();

        /// <summary>
        /// Create an entity printer based on instructions passed to the
        /// Printer descriptor associated to this schema.
        /// </summary>
        /// <returns>Printer descriptor</returns>
        IPrinter<TEntity> CreatePrinter();

        #endregion
    }
}