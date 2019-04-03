using System;
using System.Text;
using Verse.DecoderDescriptors;
using Verse.EncoderDescriptors;
using Verse.Schemas.JSON;

namespace Verse.Schemas
{
	/// <summary>
	/// Schema implementation using JSON format.
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	public class JSONSchema<TEntity> : ISchema<TEntity>
	{
		/// <inheritdoc/>
		public IDecoderDescriptor<TEntity> DecoderDescriptor => this.decoderDescriptor;

	    /// <inheritdoc/>
		public IEncoderDescriptor<TEntity> EncoderDescriptor => this.encoderDescriptor;

	    private readonly DecoderConverter decoderConverter;

		private readonly RecurseDecoderDescriptor<TEntity, ReaderState, JSONValue> decoderDescriptor;

		private readonly EncoderConverter encoderConverter;

		private readonly RecurseEncoderDescriptor<TEntity, WriterState, JSONValue> encoderDescriptor;

		/// <summary>
		/// Create new JSON schema using given settings
		/// </summary>
		/// <param name="settings">Text encoding, ignore null...</param>
		public JSONSchema(JSONConfiguration settings)
		{
			var encoding = settings.Encoding ?? new UTF8Encoding(false);

			this.decoderConverter = new DecoderConverter();
			this.encoderConverter = new EncoderConverter();
			this.decoderDescriptor = new RecurseDecoderDescriptor<TEntity, ReaderState, JSONValue>(this.decoderConverter, new ReaderSession(encoding), new Reader<TEntity>());
			this.encoderDescriptor = new RecurseEncoderDescriptor<TEntity, WriterState, JSONValue>(this.encoderConverter, new WriterSession(encoding, settings.OmitNull), new Writer<TEntity>());
		}

	    /// <summary>
	    /// Create JSON schema using default UTF8 encoding.
	    /// </summary>
	    public JSONSchema()
	        : this(default)
	    {
	    }

	    /// <inheritdoc/>
		public IDecoder<TEntity> CreateDecoder()
		{
			return this.decoderDescriptor.CreateDecoder();
		}

		/// <inheritdoc/>
		public IEncoder<TEntity> CreateEncoder()
		{
			return this.encoderDescriptor.CreateEncoder();
		}

		/// <summary>
		/// Declare decoder to convert JSON native value into target output type.
		/// </summary>
		/// <typeparam name="TOutput">Target output type</typeparam>
		/// <param name="converter">Converter from JSON native value to output type</param>
		public void SetDecoderConverter<TOutput>(Converter<JSONValue, TOutput> converter)
		{
			this.decoderConverter.Set(converter);
		}

		/// <summary>
		/// Declare encoder to convert target input type into JSON native value.
		/// </summary>
		/// <typeparam name="TInput">Target input type</typeparam>
		/// <param name="converter">Converter from input type to JSON native value</param>
		public void SetEncoderConverter<TInput>(Converter<TInput, JSONValue> converter)
		{
			this.encoderConverter.Set(converter);
		}
	}
}