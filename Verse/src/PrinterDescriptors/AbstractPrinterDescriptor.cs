using System;
using System.Collections.Generic;
using Verse.PrinterDescriptors.Abstract;

namespace Verse.PrinterDescriptors
{
    internal abstract class AbstractPrinterDescriptor<TEntity, TNative> : IPrinterDescriptor<TEntity>
    {
        #region Attributes

        protected readonly IEncoder<TNative> encoder;

        #endregion

        #region Constructors

        protected AbstractPrinterDescriptor(IEncoder<TNative> encoder)
        {
            this.encoder = encoder;
        }

        #endregion

        #region Methods / Abstract

        public abstract IPrinterDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access, IPrinterDescriptor<TField> parent);

        public abstract IPrinterDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access);

        public abstract IPrinterDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access, IPrinterDescriptor<TElement> parent);

        public abstract IPrinterDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access);

        public abstract void IsValue<TValue>(Func<TEntity, TValue> access);

        #endregion

        #region Methods / Public

        public IPrinterDescriptor<TEntity> HasField(string name)
        {
            return this.HasField(name, (source) => source);
        }

        public void IsValue()
        {
            this.IsValue((target) => target);
        }

        #endregion

        #region Methods / Protected

        protected Converter<TValue, TNative> GetConverter<TValue>()
        {
            return this.encoder.Get<TValue>();
        }

        #endregion
    }
}