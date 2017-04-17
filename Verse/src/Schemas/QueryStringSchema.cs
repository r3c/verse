using System;
using System.Text;
using Verse.DecoderDescriptors;
using Verse.Schemas.QueryString;

namespace Verse.Schemas
{
	public sealed class QueryStringSchema<TEntity> : AbstractSchema<TEntity>
	{
		#region Properties

		public override IDecoderDescriptor<TEntity> DecoderDescriptor
		{
			get
			{
				return this.decoderDescriptor;
			}
		}

		public override IEncoderDescriptor<TEntity> EncoderDescriptor
		{
			get
			{
				throw new NotImplementedException("encoding not implemented");
			}
		}

		#endregion

		#region Attributes

		private readonly DecoderConverter decoderConverter;

		private readonly FlatDecoderDescriptor<TEntity, ReaderState, string> decoderDescriptor;

		#endregion

		#region Constructor

		public QueryStringSchema(Encoding encoding)
		{
			var sourceConverter = new DecoderConverter();

			this.decoderConverter = sourceConverter;
			this.decoderDescriptor = new FlatDecoderDescriptor<TEntity, ReaderState, string>(sourceConverter, new ReaderSession(encoding), new Reader<TEntity>());
		}

		public QueryStringSchema() :
			this(new UTF8Encoding(false))
		{
		}

		#endregion

		#region Methods

		/// <inheritdoc/>
		public override IDecoder<TEntity> CreateDecoder()
		{
			return this.decoderDescriptor.CreateDecoder();
		}

		/// <inheritdoc/>
		public override IEncoder<TEntity> CreateEncoder()
		{
			throw new NotImplementedException("encoding not implemented");
		}

		public void SetDecoderConverter<U>(Converter<string, U> converter)
		{
			this.decoderConverter.Set(converter);
		}

		#endregion
	}
}
