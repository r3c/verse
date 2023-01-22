using System.IO;
using Verse.DecoderDescriptors;
using Verse.EncoderDescriptors;
using Verse.Schemas.Protobuf;
using Verse.Schemas.Protobuf.Definition;

namespace Verse.Schemas;

/// <inheritdoc />
/// <summary>
/// Protobuf serialization implementation following proto3 specification.
/// See: https://developers.google.com/protocol-buffers/docs/encoding
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public sealed class ProtobufSchema<TEntity> : ISchema<ProtobufValue, TEntity>
{
    /// <inheritdoc/>
    public IDecoderAdapter<ProtobufValue> DecoderAdapter => decoderAdapter;

    /// <inheritdoc/>
    public IDecoderDescriptor<ProtobufValue, TEntity> DecoderDescriptor => decoderDescriptor;

    /// <inheritdoc/>
    public IEncoderAdapter<ProtobufValue> EncoderAdapter => encoderAdapter;

    /// <inheritdoc/>
    public IEncoderDescriptor<ProtobufValue, TEntity> EncoderDescriptor => encoderDescriptor;

    private readonly ProtobufDecoderAdapter decoderAdapter;

    private readonly TreeDecoderDescriptor<ReaderState, ProtobufValue, int, TEntity> decoderDescriptor;

    private readonly ProtobufEncoderAdapter encoderAdapter;

    private readonly TreeEncoderDescriptor<WriterState, ProtobufValue, TEntity> encoderDescriptor;

    public ProtobufSchema(TextReader proto, string messageName, ProtobufConfiguration configuration)
    {
        var bindings = Parser.Parse(proto).Resolve(messageName);
        var reader = new ProtobufReaderDefinition<TEntity>(bindings, configuration.RejectUnknown);
        var writer = new ProtobufWriterDefinition<TEntity>(bindings);

        decoderAdapter = new ProtobufDecoderAdapter();
        decoderDescriptor = new TreeDecoderDescriptor<ReaderState, ProtobufValue, int, TEntity>(reader);
        encoderAdapter = new ProtobufEncoderAdapter();
        encoderDescriptor = new TreeEncoderDescriptor<WriterState, ProtobufValue, TEntity>(writer);
    }

    public ProtobufSchema(TextReader proto, string messageName)
        : this(proto, messageName, default)
    {
    }

    public IDecoder<TEntity> CreateDecoder()
    {
        return decoderDescriptor.CreateDecoder(new Reader());
    }

    public IEncoder<TEntity> CreateEncoder()
    {
        return encoderDescriptor.CreateEncoder(new Writer());
    }
}