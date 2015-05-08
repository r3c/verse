using System;

namespace Verse.Schemas
{
    /// <summary>
    /// Base class for schema implementations.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public abstract class AbstractSchema<TEntity> : ISchema<TEntity>
    {
        #region Properties

        /// <inheritdoc/>
        public abstract IPrinterDescriptor<TEntity> PrinterDescriptor
        {
            get;
        }

        /// <inheritdoc/>
        public abstract IParserDescriptor<TEntity> ParserDescriptor
        {
            get;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public abstract IPrinter<TEntity> CreatePrinter();

        /// <inheritdoc/>
        public abstract IParser<TEntity> CreateParser();

        #endregion
    }
}