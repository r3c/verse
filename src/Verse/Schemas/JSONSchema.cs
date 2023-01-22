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
	public sealed class JSONSchema<TEntity> : ISchema<JSONValue, TEntity>
	{
		/// <inheritdoc/>
		public IDecoderAdapter<JSONValue> DecoderAdapter => decoderAdapter;

		/// <inheritdoc/>
		public IDecoderDescriptor<JSONValue, TEntity> DecoderDescriptor => decoderDescriptor;

		/// <inheritdoc/>
		public IEncoderAdapter<JSONValue> EncoderAdapter => encoderAdapter;

		/// <inheritdoc/>
		public IEncoderDescriptor<JSONValue, TEntity> EncoderDescriptor => encoderDescriptor;

		private readonly JSONConfiguration configuration;

		private readonly JSONDecoderAdapter decoderAdapter;

		private readonly TreeDecoderDescriptor<ReaderState, JSONValue, int, TEntity> decoderDescriptor;

		private readonly JSONEncoderAdapter encoderAdapter;

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
			decoderAdapter = new JSONDecoderAdapter();
			decoderDescriptor = new TreeDecoderDescriptor<ReaderState, JSONValue, int, TEntity>(readerDefinition);
			encoderAdapter = new JSONEncoderAdapter();
			encoderDescriptor = new TreeEncoderDescriptor<WriterState, JSONValue, TEntity>(writerDefinition);
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
			var configuration = this.configuration;
			var reader = new Reader(configuration.Encoding ?? new UTF8Encoding(false),
				configuration.ReadObjectValuesAsArray, configuration.ReadScalarAsOneElementArray);

			return decoderDescriptor.CreateDecoder(reader);
		}

		/// <inheritdoc/>
		public IEncoder<TEntity> CreateEncoder()
		{
			var encoding = configuration.Encoding ?? new UTF8Encoding(false);
			var reader = new Writer(encoding, configuration.OmitNull);

			return encoderDescriptor.CreateEncoder(reader);
		}
	}
}
