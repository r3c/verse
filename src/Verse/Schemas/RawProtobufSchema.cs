using Verse.DecoderDescriptors;
using Verse.EncoderDescriptors;
using Verse.Schemas.RawProtobuf;

namespace Verse.Schemas;

/// <inheritdoc />
/// <summary>
/// Protobuf serialization implementation using implicitly-typed fields (no schema declaration).
/// See: https://developers.google.com/protocol-buffers/docs/encoding
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
internal class RawProtobufSchema<TEntity> : ISchema<RawProtobufValue, TEntity>
{
    /// <inheritdoc/>
    public IDecoderAdapter<RawProtobufValue> DecoderAdapter => _decoderAdapter;

    /// <inheritdoc/>
    public IDecoderDescriptor<RawProtobufValue, TEntity> DecoderDescriptor => _decoderDescriptor;

    private readonly RawProtobufConfiguration _configuration;

    /// <inheritdoc/>
    public IEncoderAdapter<RawProtobufValue> EncoderAdapter => _encoderAdapter;

    /// <inheritdoc/>
    public IEncoderDescriptor<RawProtobufValue, TEntity> EncoderDescriptor => _encoderDescriptor;

    /// <inheritdoc/>
    public RawProtobufValue NullValue => new(0, RawProtobufWireType.VarInt);

    private readonly RawProtobufDecoderAdapter _decoderAdapter;

    private readonly TreeDecoderDescriptor<ReaderState, RawProtobufValue, char, TEntity>
        _decoderDescriptor;

    private readonly RawProtobufEncoderAdapter _encoderAdapter;

    private readonly TreeEncoderDescriptor<WriterState, RawProtobufValue, TEntity> _encoderDescriptor;

    public RawProtobufSchema(RawProtobufConfiguration configuration)
    {
        var readerDefinition = new ReaderDefinition<TEntity>();
        var writerDefinition = new WriterDefinition<TEntity>();

        _configuration = configuration;
        _decoderAdapter = new RawProtobufDecoderAdapter();
        _decoderDescriptor =
            new TreeDecoderDescriptor<ReaderState, RawProtobufValue, char, TEntity>(readerDefinition);
        _encoderAdapter = new RawProtobufEncoderAdapter();
        _encoderDescriptor =
            new TreeEncoderDescriptor<WriterState, RawProtobufValue, TEntity>(writerDefinition);
    }

    public IDecoder<TEntity> CreateDecoder()
    {
        return _decoderDescriptor.CreateDecoder(new Reader(_configuration.NoZigZagEncoding));
    }

    public IEncoder<TEntity> CreateEncoder()
    {
        return _encoderDescriptor.CreateEncoder(new Writer(_configuration.NoZigZagEncoding));
    }
}