using System;
using Verse.Dynamics;

namespace Verse.ParserDescriptors
{
    public abstract class AbstractParserDescriptor<T> : IParserDescriptor<T>
    {
        #region Methods / Abstract

        public abstract IParserDescriptor<U> ForChildren<U> (DescriptorSet<T, U> assign, DescriptorGet<T, U> create);

        public abstract IParserDescriptor<T> ForChildren (IParserDescriptor<T> descriptor);

        public abstract IParserDescriptor<U> ForField<U> (string name, DescriptorSet<T, U> assign, DescriptorGet<T, U> create);

        public abstract IParserDescriptor<T> ForField (string name, IParserDescriptor<T> descriptor);

        public abstract void ForValue<U> (DescriptorSet<T, U> store);

        #endregion

        #region Methods / Public

        public IParserDescriptor<U> ForChildren<U> (DescriptorSet<T, U> store)
        {
            Func<U>	create;

            create = Generator.Constructor<U> ();

            return this.ForChildren (store, (ref T target) => create ());
        }

        public IParserDescriptor<T> ForChildren ()
        {
        	return this.ForChildren (this);
        }

        public IParserDescriptor<U> ForField<U> (string name, DescriptorSet<T, U> store)
        {
            Func<U>	create;

            create = Generator.Constructor<U> ();

            return this.ForField (name, store, (ref T target) => create ());
        }

        public IParserDescriptor<T> ForField (string name)
        {
        	return this.ForField (name, this);
        }

        #endregion
    }
}
