using System;
using System.Text;
using Verse.BuilderDescriptors;
using Verse.ParserDescriptors;
using Verse.Schemas.JSON;

namespace Verse.Schemas
{
	public class JSONSchema<T> : AbstractSchema<T>
	{
		#region Properties

		public override IBuilderDescriptor<T> BuilderDescriptor
		{
			get
			{
				return this.builderDescriptor;
			}
		}

		public override IParserDescriptor<T> ParserDescriptor
		{
			get
			{
				return this.parserDescriptor;
			}
		}

		#endregion

		#region Attributes

		private readonly RecurseBuilderDescriptor<T, WriterContext, Value>	builderDescriptor;

		private readonly Encoding											encoding;

		private readonly JSON.Decoder										jsonDecoder;

		private readonly JSON.Encoder										jsonDncoder;

		private readonly RecurseParserDescriptor<T, ReaderContext, Value>	parserDescriptor;

		#endregion

		#region Constructors

		public JSONSchema (Encoding encoding)
		{
			JSON.Decoder	decoder;
			JSON.Encoder	encoder;

			decoder = new JSON.Decoder ();
			encoder = new JSON.Encoder ();

			this.builderDescriptor = new RecurseBuilderDescriptor<T, WriterContext, Value> (encoder);
			this.encoding = encoding;
			this.jsonDecoder = decoder;
			this.jsonDncoder = encoder;
			this.parserDescriptor = new RecurseParserDescriptor<T, ReaderContext, Value> (decoder);
		}

		public JSONSchema () :
			this (new UTF8Encoding (false))
		{
		}

		#endregion

		#region Methods

		public override IBuilder<T> GenerateBuilder ()
		{
			return this.builderDescriptor.GetBuilder (new Writer (this.encoding));
		}

		public override IParser<T> GenerateParser (Func<T> constructor)
		{
			return this.parserDescriptor.GetParser (constructor, new Reader (this.encoding));
		}

		public void SetDecoder<U> (Converter<Value, U> converter)
		{
			this.jsonDecoder.Set (converter);
		}

		public void SetEncoder<U> (Converter<U, Value> converter)
		{
			this.jsonDncoder.Set (converter);
		}

		#endregion
	}
}
