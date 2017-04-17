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
	public class JSONSchema<TEntity> : AbstractSchema<TEntity>
	{
		#region Properties

		/// <inheritdoc/>
		public override IDecoderDescriptor<TEntity> DecoderDescriptor
		{
			get
			{
				return this.decoderDescriptor;
			}
		}

		/// <inheritdoc/>
		public override IEncoderDescriptor<TEntity> EncoderDescriptor
		{
			get
			{
				return this.encoderDescriptor;
			}
		}

		#endregion

		#region Attributes

		private readonly DecoderConverter decoderConverter;

		private readonly RecurseDecoderDescriptor<TEntity, ReaderState, Value> decoderDescriptor;

		private readonly EncoderConverter encoderConverter;

		private readonly RecurseEncoderDescriptor<TEntity, WriterState, Value> encoderDescriptor;

		#endregion

		#region Constructors

		/// <summary>
		/// Create new JSON schema using given settings
		/// </summary>
		/// <param name="settings">Text encoding, ignore null...</param>
		public JSONSchema(JSONSettings settings)
		{
			var decoderConverter = new DecoderConverter();
			var encoderConverter = new EncoderConverter();

			this.decoderConverter = decoderConverter;
			this.encoderConverter = encoderConverter;
			this.decoderDescriptor = new RecurseDecoderDescriptor<TEntity, ReaderState, Value>(decoderConverter, new ReaderSession(settings.Encoding), new Reader<TEntity>());
			this.encoderDescriptor = new RecurseEncoderDescriptor<TEntity, WriterState, Value>(encoderConverter, new WriterSession(settings), new Writer<TEntity>());
		}

		/// <summary>
		/// Create new JSON schema using given text encoding.
		/// </summary>
		/// <param name="encoding">Text encoding</param>
		public JSONSchema(Encoding encoding)
			: this(new JSONSettings(encoding, false))
		{
		}

		/// <summary>
		/// Create JSON schema using default UTF8 encoding.
		/// </summary>
		public JSONSchema()
			: this(new UTF8Encoding(false))
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
			return this.encoderDescriptor.CreateEncoder();
		}

		/// <summary>
		/// Declare decoder to convert JSON native value into target output type.
		/// </summary>
		/// <typeparam name="TOutput">Target output type</typeparam>
		/// <param name="converter">Converter from JSON native value to output type</param>
		public void SetDecoderConverter<TOutput>(Converter<Value, TOutput> converter)
		{
			this.decoderConverter.Set(converter);
		}

		/// <summary>
		/// Declare encoder to convert target input type into JSON native value.
		/// </summary>
		/// <typeparam name="TInput">Target input type</typeparam>
		/// <param name="converter">Converter from input type to JSON native value</param>
		public void SetEncoderConverter<TInput>(Converter<TInput, Value> converter)
		{
			this.encoderConverter.Set(converter);
		}

		#endregion
	}
}