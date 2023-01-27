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
internal class JsonSchema<TEntity> : ISchema<JsonValue, TEntity>
{
    /// <inheritdoc/>
    public IDecoderAdapter<JsonValue> DecoderAdapter => _decoderAdapter;

    /// <inheritdoc/>
    public IDecoderDescriptor<JsonValue, TEntity> DecoderDescriptor => _decoderDescriptor;

    /// <inheritdoc/>
    public IEncoderAdapter<JsonValue> EncoderAdapter => _encoderAdapter;

    /// <inheritdoc/>
    public IEncoderDescriptor<JsonValue, TEntity> EncoderDescriptor => _encoderDescriptor;

    /// <inheritdoc/>
    public JsonValue NullValue => JsonValue.Undefined;

    private readonly JsonConfiguration _configuration;

    private readonly JsonDecoderAdapter _decoderAdapter;

    private readonly TreeDecoderDescriptor<ReaderState, JsonValue, int, TEntity> _decoderDescriptor;

    private readonly JsonEncoderAdapter _encoderAdapter;

    private readonly TreeEncoderDescriptor<WriterState, JsonValue, TEntity> _encoderDescriptor;

    /// <summary>
    /// Create new JSON schema using given settings
    /// </summary>
    /// <param name="configuration">Text encoding, ignore null...</param>
    public JsonSchema(JsonConfiguration configuration)
    {
        var writerDefinition = new WriterDefinition<TEntity>();
        var readerDefinition = new ReaderDefinition<TEntity>();

        _configuration = configuration;
        _decoderAdapter = new JsonDecoderAdapter();
        _decoderDescriptor = new TreeDecoderDescriptor<ReaderState, JsonValue, int, TEntity>(readerDefinition);
        _encoderAdapter = new JsonEncoderAdapter();
        _encoderDescriptor = new TreeEncoderDescriptor<WriterState, JsonValue, TEntity>(writerDefinition);
    }

    /// <inheritdoc/>
    public IDecoder<TEntity> CreateDecoder()
    {
        var configuration = _configuration;
        var reader = new Reader(configuration.Encoding ?? new UTF8Encoding(false),
            configuration.ReadObjectValuesAsArray, configuration.ReadScalarAsOneElementArray);

        return _decoderDescriptor.CreateDecoder(reader);
    }

    /// <inheritdoc/>
    public IEncoder<TEntity> CreateEncoder()
    {
        var encoding = _configuration.Encoding ?? new UTF8Encoding(false);
        var reader = new Writer(encoding, _configuration.OmitNull);

        return _encoderDescriptor.CreateEncoder(reader);
    }
}