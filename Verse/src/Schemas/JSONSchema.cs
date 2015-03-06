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

		private readonly RecurseBuilderDescriptor<T, WriterContext, Value> builderDescriptor;

		private readonly Encoding encoding;

		private readonly RecurseParserDescriptor<T, ReaderContext, Value> parserDescriptor;

		private readonly ValueDecoder valueDecoder;

		private readonly ValueEncoder valueEncoder;

		#endregion

		#region Constructors

		public JSONSchema (Encoding encoding)
		{
			ValueDecoder decoder;
			ValueEncoder encoder;

			decoder = new ValueDecoder ();
			encoder = new ValueEncoder ();

			this.builderDescriptor = new RecurseBuilderDescriptor<T, WriterContext, Value> (encoder);
			this.encoding = encoding;
			this.parserDescriptor = new RecurseParserDescriptor<T, ReaderContext, Value> (decoder);
			this.valueDecoder = decoder;
			this.valueEncoder = encoder;
		}

		public JSONSchema () :
			this (new UTF8Encoding (false))
		{
		}

		#endregion

		#region Methods

		public override IBuilder<T> CreateBuilder ()
		{
			return this.builderDescriptor.CreateBuilder (new Writer (this.encoding));
		}

		public override IParser<T> CreateParser ()
		{
			return this.parserDescriptor.CreateParser (new Reader (this.encoding));
		}

		public void SetDecoder<U> (Converter<Value, U> converter)
		{
			this.valueDecoder.Set (converter);
		}

		public void SetEncoder<U> (Converter<U, Value> converter)
		{
			this.valueEncoder.Set (converter);
		}

		#endregion
	}
}
