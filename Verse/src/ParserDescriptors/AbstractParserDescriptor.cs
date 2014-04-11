using System;
using Verse.Dynamics;

namespace Verse.ParserDescriptors
{
    public abstract class AbstractParserDescriptor<T> : IParserDescriptor<T>
    {
        #region Methods / Abstract

        public abstract IParserDescriptor<U> HasChildren<U> (DescriptorSet<T, U> store, DescriptorGet<T, U> create, IParserDescriptor<U> recurse);

        public abstract IParserDescriptor<U> HasChildren<U> (DescriptorSet<T, U> store, DescriptorGet<T, U> create);

        public abstract IParserDescriptor<T> HasChildren ();

        public abstract IParserDescriptor<U> HasField<U> (string name, DescriptorSet<T, U> store, DescriptorGet<T, U> create, IParserDescriptor<U> recurse);

        public abstract IParserDescriptor<U> HasField<U> (string name, DescriptorSet<T, U> store, DescriptorGet<T, U> create);

        public abstract IParserDescriptor<T> HasField (string name);

        public abstract void IsValue ();

        #endregion

        #region Methods / Public

        public IParserDescriptor<U> HasChildren<U> (DescriptorSet<T, U> store)
        {
            Func<U>	create;

            create = Generator.Constructor<U> ();

            return this.HasChildren (store, (ref T target) => create ());
        }

        public IParserDescriptor<U> HasField<U> (string name, DescriptorSet<T, U> store)
        {
            Func<U>	create;

            create = Generator.Constructor<U> ();

            return this.HasField (name, store, (ref T target) => create ());
        }

        #endregion
    }
}
