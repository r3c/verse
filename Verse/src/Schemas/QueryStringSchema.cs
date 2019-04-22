using System;
using System.Text;
using Verse.DecoderDescriptors;
using Verse.DecoderDescriptors.Tree;
using Verse.Schemas.QueryString;

namespace Verse.Schemas
{
	public sealed class QueryStringSchema<TEntity> : ISchema<TEntity>
	{
		public IDecoderDescriptor<TEntity> DecoderDescriptor => this.decoderDescriptor;

		public IEncoderDescriptor<TEntity> EncoderDescriptor => throw new NotImplementedException("encoding not implemented");

		private readonly DecoderConverter decoderConverter;

		private readonly TreeDecoderDescriptor<ReaderState, string, TEntity> decoderDescriptor;

		private readonly Encoding encoding;

		public QueryStringSchema(Encoding encoding)
		{
			var decoderConverter = new DecoderConverter();

			this.decoderConverter = decoderConverter;
			this.decoderDescriptor = new TreeDecoderDescriptor<ReaderState, string, TEntity>(decoderConverter, new ReaderDefinition<ReaderState, string, TEntity>());
			this.encoding = encoding;
		}

		public QueryStringSchema() :
			this(new UTF8Encoding(false))
		{
		}

		/// <inheritdoc/>
		public IDecoder<TEntity> CreateDecoder(Func<TEntity> constructor)
		{
			return this.decoderDescriptor.CreateDecoder(new ReaderSession(this.encoding), constructor);
		}

		/// <inheritdoc/>
		public IEncoder<TEntity> CreateEncoder()
		{
			throw new NotImplementedException("encoding not implemented");
		}

		public void SetDecoderConverter<U>(Converter<string, U> converter)
		{
			this.decoderConverter.Set(converter);
		}
	}
}
