using System;
using System.Collections.Generic;

namespace Verse.Schemas
{
    public abstract class ConvertSchema<T, V> : ISchema<T>
    {
        #region Attributes

        private readonly Dictionary<Type, object>   converters;

        #endregion

        #region Methods / Abstract

        public abstract IDescriptor<U> ForChildren<U>(DescriptorDeclareChildren<T, U> declare);

        public abstract IDescriptor<U> ForField<U>(string name, DescriptorDeclareField<T, U> declare);

        public abstract bool Generate(out IParser<T> parser);

        #endregion

        #region Methods / Public

        public void LetValue<U>(DescriptorAssign<T, U> assign)
        {
            Converter<U>    converter;
            object          value;

            if (!this.converters.TryGetValue(typeof(U), out value))
                throw new ArgumentOutOfRangeException("assign", assign, "no converter available for this value assignment");

            converter = value as Converter<U>;

            throw new NotImplementedException();
        }

        public void SetConverter<U>(Converter<U> converter)
        {
            if (converter == null)
                throw new ArgumentNullException("converter");

            this.converters[typeof(U)] = converter;
        }

        #endregion

        #region Types

        public delegate bool Converter<U>(V value, out U output);

        #endregion
    }
}
