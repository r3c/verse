using System.IO;
using Verse.DecoderDescriptors;
using Verse.EncoderDescriptors;
using Verse.Formats.Protobuf;
using Verse.Schemas.Protobuf;
using Verse.Schemas.Protobuf.Definition;

namespace Verse.Schemas;

/// <inheritdoc />
/// <summary>
/// Protobuf serialization implementation following proto3 specification.
/// See: https://developers.google.com/protocol-buffers/docs/encoding
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
internal sealed class ProtobufSchema<TEntity> : ISchema<ProtobufValue, TEntity>
{
    /// <inheritdoc/>
    public IDecoderDescriptor<ProtobufValue, TEntity> DecoderDescriptor => _decoderDescriptor;

    /// <inheritdoc/>
    public IEncoderDescriptor<ProtobufValue, TEntity> EncoderDescriptor => _encoderDescriptor;

    private readonly TreeDecoderDescriptor<ReaderState, ProtobufValue, int, TEntity> _decoderDescriptor;

    private readonly TreeEncoderDescriptor<WriterState, ProtobufValue, TEntity> _encoderDescriptor;

    public ProtobufSchema(TextReader proto, string messageName, ProtobufConfiguration configuration)
    {
        var bindings = Parser.Parse(proto).Resolve(messageName);
        var reader = new ProtobufReaderDefinition<TEntity>(bindings, configuration.RejectUnknown);
        var writer = new ProtobufWriterDefinition<TEntity>(bindings);

        _decoderDescriptor = new TreeDecoderDescriptor<ReaderState, ProtobufValue, int, TEntity>(reader);
        _encoderDescriptor = new TreeEncoderDescriptor<WriterState, ProtobufValue, TEntity>(writer);
    }

    public ProtobufSchema(TextReader proto, string messageName)
        : this(proto, messageName, default)
    {
    }

    public IDecoder<TEntity> CreateDecoder()
    {
        return _decoderDescriptor.CreateDecoder(new Reader());
    }

    public IEncoder<TEntity> CreateEncoder()
    {
        return _encoderDescriptor.CreateEncoder(new Writer());
    }
}