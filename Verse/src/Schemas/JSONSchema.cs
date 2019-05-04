using System;
using System.Text;
using Verse.DecoderDescriptors;
using Verse.EncoderDescriptors;
using Verse.Schemas.JSON;

namespace Verse.Schemas
{
	/// <inheritdoc />
	/// <summary>
	/// JSON serialization implementation.
	/// See: https://www.json.org/
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	public sealed class JSONSchema<TEntity> : ISchema<TEntity>
	{
		/// <inheritdoc/>
		public IDecoderDescriptor<TEntity> DecoderDescriptor => this.decoderDescriptor;

		/// <inheritdoc/>
		public IEncoderDescriptor<TEntity> EncoderDescriptor => this.encoderDescriptor;

		private readonly JSONConfiguration configuration;

		private readonly DecoderConverter decoderConverter;

		private readonly TreeDecoderDescriptor<ReaderState, JSONValue, int, TEntity> decoderDescriptor;

		private readonly EncoderConverter encoderConverter;

		private readonly TreeEncoderDescriptor<WriterState, JSONValue, TEntity> encoderDescriptor;

		/// <summary>
		/// Create new JSON schema using given settings
		/// </summary>
		/// <param name="configuration">Text encoding, ignore null...</param>
		public JSONSchema(JSONConfiguration configuration)
		{
			var writerDefinition = new WriterDefinition<TEntity>();
			var readerDefinition = new ReaderDefinition<TEntity>();

			this.configuration = configuration;
			this.decoderConverter = new DecoderConverter();
			this.encoderConverter = new EncoderConverter();
			this.decoderDescriptor = new TreeDecoderDescriptor<ReaderState, JSONValue, int, TEntity>(this.decoderConverter, readerDefinition);
			this.encoderDescriptor = new TreeEncoderDescriptor<WriterState, JSONValue, TEntity>(this.encoderConverter, writerDefinition);
		}

		/// <summary>
		/// Create JSON schema using default UTF8 encoding.
		/// </summary>
		public JSONSchema()
			: this(default)
		{
		}

		/// <inheritdoc/>
		public IDecoder<TEntity> CreateDecoder(Func<TEntity> constructor)
		{
			var configuration = this.configuration;
			var reader = new Reader(configuration.Encoding ?? new UTF8Encoding(false),
				configuration.ReadObjectValuesAsArray, configuration.ReadScalarAsOneElementArray);

			return this.decoderDescriptor.CreateDecoder(reader, constructor);
		}

		/// <inheritdoc/>
		public IEncoder<TEntity> CreateEncoder()
		{
			var encoding = this.configuration.Encoding ?? new UTF8Encoding(false);
			var reader = new Writer(encoding, this.configuration.OmitNull);

			return this.encoderDescriptor.CreateEncoder(reader);
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
