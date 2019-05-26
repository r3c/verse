using System;
using System.Text;
using Verse.DecoderDescriptors;
using Verse.Schemas.QueryString;

namespace Verse.Schemas
{
	/// <inheritdoc />
	/// <summary>
	/// URI query string serialization implementation following RFC-3986. This implementation has no support for
	/// encoding yet.
	/// See: https://tools.ietf.org/html/rfc3986#section-3.4
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	public sealed class QueryStringSchema<TEntity> : ISchema<string, TEntity>
	{
		/// <inheritdoc/>
		public IDecoderAdapter<string> DecoderAdapter => this.decoderAdapter;

		/// <inheritdoc/>
		public IDecoderDescriptor<string, TEntity> DecoderDescriptor => this.decoderDescriptor;

		/// <inheritdoc/>
		public IEncoderAdapter<string> EncoderAdapter => throw new NotImplementedException("encoding not implemented");

		/// <inheritdoc/>
		public IEncoderDescriptor<string, TEntity> EncoderDescriptor =>
			throw new NotImplementedException("encoding not implemented");

		private readonly QueryStringDecoderAdapter decoderAdapter;

		private readonly TreeDecoderDescriptor<ReaderState, string, char, TEntity> decoderDescriptor;

		private readonly Encoding encoding;

		public QueryStringSchema(Encoding encoding)
		{
			var readerDefinition = new ReaderDefinition<TEntity>();

			this.decoderAdapter = new QueryStringDecoderAdapter();
			this.decoderDescriptor = new TreeDecoderDescriptor<ReaderState, string, char, TEntity>(readerDefinition);
			this.encoding = encoding;
		}

		public QueryStringSchema() :
			this(new UTF8Encoding(false))
		{
		}

		/// <inheritdoc/>
		public IDecoder<TEntity> CreateDecoder(Func<TEntity> constructor)
		{
			return this.decoderDescriptor.CreateDecoder(new Reader(this.encoding), constructor);
		}

		/// <inheritdoc/>
		public IEncoder<TEntity> CreateEncoder()
		{
			throw new NotImplementedException("encoding not implemented");
		}
	}
}
