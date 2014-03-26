using System;
using System.IO;
using Verse.Dynamics;
using Verse.ParserDescriptors;

namespace Verse.Schemas
{
    public abstract class TreeSchema<T, C, V> : ISchema<T>
    {
        #region Attributes

        private readonly RecurseParserDescriptor<T, C, V>	descriptor;

        #endregion

        #region Constructors

        protected TreeSchema (RecurseParserDescriptor.IAdapter<V> adapter)
        {
        	this.descriptor = new RecurseParserDescriptor<T, C, V> (adapter);
        }

        #endregion

        #region Methods / Abstract

        protected abstract RecurseParserDescriptor.IReader<C, V>	GetReader ();

        #endregion

        #region Methods / Public

        public IParser<T> GetParser (Func<T> constructor)
        {
            return new TreeParser (constructor, this.descriptor.Pointer, this.GetReader ());
        }

        public IParser<T> GetParser ()
        {
        	return this.GetParser (Generator.Constructor<T> ());
        }

        public IParserDescriptor<U> ForChildren<U> (DescriptorSet<T, U> assign, DescriptorGet<T, U> create)
        {
            return this.descriptor.ForChildren (assign, create);
        }

        public IParserDescriptor<T> ForChildren (IParserDescriptor<T> descriptor)
        {
            return this.descriptor.ForChildren (descriptor);
        }

		public IParserDescriptor<U> ForChildren<U> (DescriptorSet<T, U> store)
		{
            return this.descriptor.ForChildren (store);
		}

		public IParserDescriptor<T> ForChildren ()
		{
			return this.descriptor.ForChildren ();
		}

        public IParserDescriptor<U> ForField<U> (string name, DescriptorSet<T, U> assign, DescriptorGet<T, U> create)
        {
            return this.descriptor.ForField (name, assign, create);
        }

        public IParserDescriptor<T> ForField (string name, IParserDescriptor<T> descriptor)
        {
            return this.descriptor.ForField (name, descriptor);
        }

		public IParserDescriptor<U> ForField<U> (string name, DescriptorSet<T, U> store)
		{
			return this.descriptor.ForField (name, store);
		}

		public IParserDescriptor<T> ForField (string name)
		{
			return this.descriptor.ForField (name);
		}

        public void ForValue<U> (DescriptorSet<T, U> store)
        {
            this.descriptor.ForValue (store);
        }

        #endregion

        #region Types

        private class TreeParser : IParser<T>
        {
            private readonly Func<T>									constructor;

            private readonly RecurseParserDescriptor.IPointer<T, C, V>	pointer;

            private readonly RecurseParserDescriptor.IReader<C, V>		reader;

            public TreeParser (Func<T> constructor, RecurseParserDescriptor.IPointer<T, C, V> pointer, RecurseParserDescriptor.IReader<C, V> reader)
            {
                this.constructor = constructor;
                this.pointer = pointer;
                this.reader = reader;
            }

            public bool Parse (Stream input, out T output)
            {
                C		context;
                bool	result;

                if (!this.reader.Begin (input, out context))
                {
                    output = default (T);

                    return false;
                }

                try
                {
                	output = this.constructor ();
                	result = this.reader.Read (ref output, this.pointer, context);
                }
                finally
                {
                	this.reader.End (context);
                }

                return result;
            }
        }

        #endregion
    }
}
