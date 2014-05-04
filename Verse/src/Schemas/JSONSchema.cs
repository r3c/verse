using System;
using System.Text;
using Verse.BuilderDescriptors.Recurse;
using Verse.ParserDescriptors.Recurse;
using Verse.Schemas.JSON;

namespace Verse.Schemas
{
	public class JSONSchema<T> : TreeSchema<T, ReaderContext, WriterContext, Value>
	{
		#region Attributes

		private readonly JSON.Decoder	decoder;

		private readonly JSON.Encoder	encoder;

		private readonly Encoding		encoding;

		#endregion

		#region Constructors / Public

		public JSONSchema (Encoding encoding) :
			this (new JSON.Decoder (), new JSON.Encoder (), encoding)
		{
		}

		public JSONSchema () :
			this (new UTF8Encoding (false))
		{
		}

		#endregion

		#region Constructors / Private

		private JSONSchema (JSON.Decoder decoder, JSON.Encoder encoder, Encoding encoding) :
			base (decoder, encoder)
		{
			this.decoder = decoder;
			this.encoder = encoder;
			this.encoding = encoding;
		}

		#endregion

		#region Methods / Public

		public void SetDecoder<U> (Converter<Value, U> converter)
		{
			this.decoder.Set (converter);
		}

		public void SetEncoder<U> (Converter<U, Value> converter)
		{
			this.encoder.Set (converter);
		}

		#endregion

		#region Methods / Protected

		protected override IReader<ReaderContext, Value> GetReader ()
		{
			return new Reader (this.encoding);
		}

		protected override IWriter<WriterContext, Value> GetWriter()
		{
			return new Writer (this.encoding);
		}

		#endregion
	}
}
