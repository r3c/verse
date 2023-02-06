using Verse.DecoderDescriptors;
using Verse.EncoderDescriptors;
using Verse.Formats.RawProtobuf;
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
    public IDecoderDescriptor<RawProtobufValue, TEntity> DecoderDescriptor => _decoderDescriptor;

    /// <inheritdoc/>
    public IEncoderDescriptor<RawProtobufValue, TEntity> EncoderDescriptor => _encoderDescriptor;

    private readonly RawProtobufConfiguration _configuration;

    private readonly TreeDecoderDescriptor<ReaderState, RawProtobufValue, char, TEntity> _decoderDescriptor;

    private readonly TreeEncoderDescriptor<WriterState, RawProtobufValue, TEntity> _encoderDescriptor;

    public RawProtobufSchema(RawProtobufConfiguration configuration)
    {
        var readerDefinition = new ReaderDefinition<TEntity>();
        var writerDefinition = new WriterDefinition<TEntity>();

        _configuration = configuration;
        _decoderDescriptor = new TreeDecoderDescriptor<ReaderState, RawProtobufValue, char, TEntity>(readerDefinition);
        _encoderDescriptor = new TreeEncoderDescriptor<WriterState, RawProtobufValue, TEntity>(writerDefinition);
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