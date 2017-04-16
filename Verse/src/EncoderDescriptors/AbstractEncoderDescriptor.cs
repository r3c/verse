using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Abstract;

namespace Verse.EncoderDescriptors
{
    abstract class AbstractEncoderDescriptor<TEntity, TValue> : IEncoderDescriptor<TEntity>
    {
        #region Attributes

        protected readonly IEncoderConverter<TValue> converter;

        #endregion

        #region Constructors

        protected AbstractEncoderDescriptor(IEncoderConverter<TValue> converter)
        {
            this.converter = converter;
        }

        #endregion

        #region Methods / Abstract

        public abstract IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access, IEncoderDescriptor<TField> parent);

        public abstract IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access);

        public abstract IEncoderDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access, IEncoderDescriptor<TElement> parent);

        public abstract IEncoderDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access);

        public abstract void IsValue<TRaw>(Func<TEntity, TRaw> access);

        #endregion

        #region Methods / Public

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