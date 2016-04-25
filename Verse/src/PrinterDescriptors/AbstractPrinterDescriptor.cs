using System;
using System.Collections.Generic;
using Verse.PrinterDescriptors.Abstract;

namespace Verse.PrinterDescriptors
{
    abstract class AbstractPrinterDescriptor<TEntity, TValue> : IPrinterDescriptor<TEntity>
    {
        #region Attributes

        protected readonly IEncoderConverter<TValue> converter;

        #endregion

        #region Constructors

        protected AbstractPrinterDescriptor(IEncoderConverter<TValue> converter)
        {
            this.converter = converter;
        }

        #endregion

        #region Methods / Abstract

        public abstract IPrinterDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access, IPrinterDescriptor<TField> parent);

        public abstract IPrinterDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access);

        public abstract IPrinterDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access, IPrinterDescriptor<TElement> parent);

        public abstract IPrinterDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access);

        public abstract void IsValue<TRaw>(Func<TEntity, TRaw> access);

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

        protected Converter<TRaw, TValue> GetConverter<TRaw>()
        {
            return this.converter.Get<TRaw>();
        }

        #endregion
    }
}