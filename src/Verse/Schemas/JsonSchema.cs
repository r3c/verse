using System.Text;
using Verse.DecoderDescriptors;
using Verse.EncoderDescriptors;
using Verse.Schemas.Json;

namespace Verse.Schemas;

/// <inheritdoc />
/// <summary>
/// JSON serialization implementation.
/// See: https://www.json.org/
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public sealed class JsonSchema<TEntity> : ISchema<JsonValue, TEntity>
{
    /// <inheritdoc/>
    public IDecoderAdapter<JsonValue> DecoderAdapter => decoderAdapter;

    /// <inheritdoc/>
    public IDecoderDescriptor<JsonValue, TEntity> DecoderDescriptor => decoderDescriptor;

    /// <inheritdoc/>
    public IEncoderAdapter<JsonValue> EncoderAdapter => encoderAdapter;

    /// <inheritdoc/>
    public IEncoderDescriptor<JsonValue, TEntity> EncoderDescriptor => encoderDescriptor;

    private readonly JsonConfiguration configuration;

    private readonly JsonDecoderAdapter decoderAdapter;

    private readonly TreeDecoderDescriptor<ReaderState, JsonValue, int, TEntity> decoderDescriptor;

    private readonly JsonEncoderAdapter encoderAdapter;

    private readonly TreeEncoderDescriptor<WriterState, JsonValue, TEntity> encoderDescriptor;

    /// <summary>
    /// Create new JSON schema using given settings
    /// </summary>
    /// <param name="configuration">Text encoding, ignore null...</param>
    public JsonSchema(JsonConfiguration configuration)
    {
        var writerDefinition = new WriterDefinition<TEntity>();
        var readerDefinition = new ReaderDefinition<TEntity>();

        this.configuration = configuration;
        decoderAdapter = new JsonDecoderAdapter();
        decoderDescriptor = new TreeDecoderDescriptor<ReaderState, JsonValue, int, TEntity>(readerDefinition);
        encoderAdapter = new JsonEncoderAdapter();
        encoderDescriptor = new TreeEncoderDescriptor<WriterState, JsonValue, TEntity>(writerDefinition);
    }

    /// <summary>
    /// Create JSON schema using default UTF8 encoding.
    /// </summary>
    public JsonSchema()
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