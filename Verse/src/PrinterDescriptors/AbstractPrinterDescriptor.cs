using System;
using System.Collections.Generic;

namespace Verse.PrinterDescriptors
{
    internal abstract class AbstractPrinterDescriptor<TEntity> : IPrinterDescriptor<TEntity>
    {
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
    }
}