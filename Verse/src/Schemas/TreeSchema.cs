using System;
using System.IO;
using Verse.Dynamics;
using Verse.ParserDescriptors;
using Verse.ParserDescriptors.Recurse;

namespace Verse.Schemas
{
    public abstract class TreeSchema<T, C, V> : ISchema<T>
    {
    	#region Properties

    	public IBuilderDescriptor<T> BuilderDescriptor
    	{
    		get
    		{
    			throw new NotImplementedException ();
    		}
    	}

        public IParserDescriptor<T> ParserDescriptor
        {
        	get
        	{
        		return this.parserDescriptor;
        	}
        }

    	#endregion

        #region Attributes

        private readonly RecurseParserDescriptor<T, C, V>	parserDescriptor;

        #endregion

        #region Constructors

        protected TreeSchema (IAdapter<V> adapter)
        {
        	this.parserDescriptor = new RecurseParserDescriptor<T, C, V> (adapter);
        }

        #endregion

        #region Methods / Abstract

        protected abstract IReader<C, V> GetReader ();

        #endregion

        #region Methods / Public

        public IBuilder<T> GenerateBuilder ()
        {
        	throw new NotImplementedException ();
        }

        public IParser<T> GenerateParser (Func<T> constructor)
        {
            return new TreeParser (constructor, this.parserDescriptor.Pointer, this.GetReader ());
        }

        public IParser<T> GenerateParser ()
        {
        	return this.GenerateParser (Generator.Constructor<T> ());
        }

        #endregion

        #region Types

        private class TreeParser : IParser<T>
        {
            private readonly Func<T>			constructor;

            private readonly IPointer<T, C, V>	pointer;

            private readonly IReader<C, V>		reader;

            public TreeParser (Func<T> constructor, IPointer<T, C, V> pointer, IReader<C, V> reader)
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
