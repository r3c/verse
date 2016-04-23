using System;
using System.Collections.Generic;
using Verse.ParserDescriptors.Abstract;
using Verse.Tools;

namespace Verse.ParserDescriptors
{
    abstract class AbstractParserDescriptor<TEntity, TValue> : IParserDescriptor<TEntity>
    {
        #region Attributes

        private readonly Dictionary<Type, object> constructors;

        protected readonly IDecoderConverter<TValue> converter;

        #endregion

        #region Constructors

        protected AbstractParserDescriptor(IDecoderConverter<TValue> converter)
        {
            this.constructors = new Dictionary<Type, object>();
            this.converter = converter;
        }

        #endregion

        #region Methods / Abstract

        public abstract IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign, IParserDescriptor<TField> parent);

        public abstract IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign);

        public abstract IParserDescriptor<TEntity> HasField(string name);

        public abstract IParserDescriptor<TElement> IsArray<TElement>(ParserAssign<TEntity, IEnumerable<TElement>> assign, IParserDescriptor<TElement> parent);

        public abstract IParserDescriptor<TElement> IsArray<TElement>(ParserAssign<TEntity, IEnumerable<TElement>> assign);

        public abstract void IsValue<TRaw>(ParserAssign<TEntity, TRaw> assign);

        #endregion

        #region Methods / Public

        public void CanCreate<TField>(Func<TEntity, TField> constructor)
        {
            if (constructor == null)
                throw new ArgumentNullException("constructor");

            this.constructors[typeof (TField)] = constructor;
        }

        public void IsValue()
        {
            this.IsValue((ref TEntity target, TEntity value) => target = value);
        }

        #endregion

        #region Methods / Protected

        protected Func<TEntity, TField> GetConstructor<TField>()
        {
            object box;
            Func<TField> constructor;

            if (!this.constructors.TryGetValue(typeof (TField), out box))
            {
                constructor = Generator.Constructor<TField>();

                return (source) => constructor();
            }

            return (Func<TEntity, TField>)box;
        }

        protected Converter<TValue, TRaw> GetConverter<TRaw>()
        {
            return this.converter.Get<TRaw>();
        }

        #endregion
    }
}