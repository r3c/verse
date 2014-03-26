using System;
using System.Text;
using Verse.ParserDescriptors;
using Verse.Schemas.JSON;

namespace Verse.Schemas
{
    public class JSONSchema<T> : TreeSchema<T, Context, Value>
    {
        #region Attributes

        private readonly Adapter	adapter;

        private readonly Encoding	encoding;

        #endregion

        #region Constructors / Public

        public JSONSchema (Encoding encoding) :
        	this (new Adapter (), encoding)
        {
        }

        public JSONSchema () :
            this (Encoding.UTF8)
        {
        }

        #endregion

        #region Constructors / Private

        private JSONSchema (Adapter adapter, Encoding encoding) :
        	base (adapter)
        {
        	this.adapter = adapter;
        	this.encoding = encoding;
        }

        #endregion

        #region Methods / Public

        public void Register<U> (Converter<Value, U> converter)
        {
        	this.adapter.Set (converter);
        }

        #endregion

        #region Methods / Protected

        protected override RecurseParserDescriptor.IReader<Context, Value> GetReader ()
        {
            return new Reader (this.encoding);
        }

        #endregion
    }
}
