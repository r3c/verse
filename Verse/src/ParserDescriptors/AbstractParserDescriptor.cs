using System;
using System.Collections.Generic;
using System.Reflection;
using Verse.Tools;

namespace Verse.ParserDescriptors
{
    internal abstract class AbstractParserDescriptor<TEntity> : IParserDescriptor<TEntity>
    {
        #region Attributes

        private readonly Dictionary<Type, object> constructors = new Dictionary<Type, object>();

        #endregion

        #region Methods / Abstract

        public abstract IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign, IParserDescriptor<TField> parent);

        public abstract IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign);

        public abstract IParserDescriptor<TEntity> HasField(string name);

        public abstract IParserDescriptor<TItem> IsArray<TItem>(ParserAssign<TEntity, IEnumerable<TItem>> assign, IParserDescriptor<TItem> parent);

        public abstract IParserDescriptor<TItem> IsArray<TItem>(ParserAssign<TEntity, IEnumerable<TItem>> assign);

        public abstract void IsValue<TValue>(ParserAssign<TEntity, TValue> assign);

        #endregion

        #region Methods / Public

        public void CanCreate<TValue>(Func<TEntity, TValue> constructor)
        {
            if (constructor == null)
                throw new ArgumentNullException("constructor");

            this.constructors[typeof (TValue)] = constructor;
        }

        public void IsValue()
        {
            this.IsValue((ref TEntity target, TEntity value) => target = value);
        }

        #endregion

        #region Methods / Protected

        protected Func<TEntity, TValue> GetConstructor<TValue>()
        {
            object box;
            Func<TValue> constructor;

            if (!this.constructors.TryGetValue(typeof (TValue), out box))
            {
                constructor = Generator.Constructor<TValue>();

                return (source) => constructor();
            }

            return (Func<TEntity, TValue>)box;
        }

        #endregion
    }
}