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
    public IDecoderDescriptor<JsonValue, TEntity> DecoderDescriptor => _decoderDescriptor;

    /// <inheritdoc/>
    public IEncoderDescriptor<JsonValue, TEntity> EncoderDescriptor => _encoderDescriptor;

    /// <inheritdoc/>
    public JsonValue DefaultValue => JsonValue.Undefined;

    /// <inheritdoc/>
    public IEncoderAdapter<JsonValue> NativeFrom => _encoderAdapter;

    /// <inheritdoc/>
    public IDecoderAdapter<JsonValue> NativeTo => _decoderAdapter;

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
        var reader = new Reader(
            _configuration.Encoding ?? new UTF8Encoding(false),
            _configuration.ReadObjectValuesAsArray,
            _configuration.ReadScalarAsOneElementArray);

        return _decoderDescriptor.CreateDecoder(reader);
    }

    /// <inheritdoc/>
    public IEncoder<TEntity> CreateEncoder()
    {
        var reader = new Writer(
            _configuration.Encoding ?? new UTF8Encoding(false),
            _configuration.OmitNull);

        return _encoderDescriptor.CreateEncoder(reader);
    }
}