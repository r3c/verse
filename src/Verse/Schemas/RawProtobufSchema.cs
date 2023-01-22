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
public sealed class RawProtobufSchema<TEntity> : ISchema<RawProtobufValue, TEntity>
{
    /// <inheritdoc/>
    public IDecoderAdapter<RawProtobufValue> DecoderAdapter => decoderAdapter;

    /// <inheritdoc/>
    public IDecoderDescriptor<RawProtobufValue, TEntity> DecoderDescriptor => decoderDescriptor;

    private readonly RawProtobufConfiguration configuration;

    /// <inheritdoc/>
    public IEncoderAdapter<RawProtobufValue> EncoderAdapter => encoderAdapter;

    /// <inheritdoc/>
    public IEncoderDescriptor<RawProtobufValue, TEntity> EncoderDescriptor => encoderDescriptor;

    private readonly RawProtobufDecoderAdapter decoderAdapter;

    private readonly TreeDecoderDescriptor<ReaderState, RawProtobufValue, char, TEntity>
        decoderDescriptor;

    private readonly RawProtobufEncoderAdapter encoderAdapter;

    private readonly TreeEncoderDescriptor<WriterState, RawProtobufValue, TEntity> encoderDescriptor;

    public RawProtobufSchema(RawProtobufConfiguration configuration)
    {
        var readerDefinition = new ReaderDefinition<TEntity>();
        var writerDefinition = new WriterDefinition<TEntity>();

        this.configuration = configuration;
        decoderAdapter = new RawProtobufDecoderAdapter();
        decoderDescriptor =
            new TreeDecoderDescriptor<ReaderState, RawProtobufValue, char, TEntity>(readerDefinition);
        encoderAdapter = new RawProtobufEncoderAdapter();
        encoderDescriptor =
            new TreeEncoderDescriptor<WriterState, RawProtobufValue, TEntity>(writerDefinition);
    }

    public RawProtobufSchema() :
        this(new RawProtobufConfiguration())
    {
    }

    public IDecoder<TEntity> CreateDecoder()
    {
        return decoderDescriptor.CreateDecoder(new Reader(configuration.NoZigZagEncoding));
    }

    public IEncoder<TEntity> CreateEncoder()
    {
        return encoderDescriptor.CreateEncoder(new Writer(configuration.NoZigZagEncoding));
    }
}